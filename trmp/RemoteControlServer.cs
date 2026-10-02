using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WSPR_Sked
{
    // Settings stored in RemoteAccess.json. Password and passcode are stored
    // only as salted PBKDF2 hashes, never as plain text.
    public sealed class RemoteConfig
    {
        public bool Enabled { get; set; }
        public int Port { get; set; } = 5005;
        public bool AllowRemoteEnable { get; set; }
        public string User { get; set; } = "";
        public string PwSalt { get; set; } = "";
        public string PwHash { get; set; } = "";
        public string CodeSalt { get; set; } = "";
        public string CodeHash { get; set; } = "";

        public bool HasCredentials =>
            User != "" && PwHash != "" && CodeHash != "";

        static byte[] Hash(string secret, byte[] salt) =>
            Rfc2898DeriveBytes.Pbkdf2(secret, salt, 200_000, HashAlgorithmName.SHA256, 32);

        public void SetPassword(string pw)
        {
            byte[] s = RandomNumberGenerator.GetBytes(16);
            PwSalt = Convert.ToBase64String(s);
            PwHash = Convert.ToBase64String(Hash(pw, s));
        }

        public void SetPasscode(string code)
        {
            byte[] s = RandomNumberGenerator.GetBytes(16);
            CodeSalt = Convert.ToBase64String(s);
            CodeHash = Convert.ToBase64String(Hash(code, s));
        }

        // All three checks always run, so timing doesn't reveal which one failed.
        public bool Check(string user, string pw, string code)
        {
            if (!HasCredentials) return false;
            bool ok = CryptographicOperations.FixedTimeEquals(
                          Encoding.UTF8.GetBytes(user ?? ""), Encoding.UTF8.GetBytes(User));
            ok &= CryptographicOperations.FixedTimeEquals(
                      Hash(pw ?? "", Convert.FromBase64String(PwSalt)), Convert.FromBase64String(PwHash));
            ok &= CryptographicOperations.FixedTimeEquals(
                      Hash(code ?? "", Convert.FromBase64String(CodeSalt)), Convert.FromBase64String(CodeHash));
            return ok;
        }

        public static RemoteConfig Load(string path)
        {
            try
            {
                if (File.Exists(path))
                    return JsonSerializer.Deserialize<RemoteConfig>(File.ReadAllText(path)) ?? new RemoteConfig();
            }
            catch { }
            return new RemoteConfig();
        }

        public void Save(string path) =>
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public sealed class RemoteControlServer : IDisposable
    {
        const int MaxFails = 5;
        static readonly TimeSpan LockTime = TimeSpan.FromMinutes(15);

        readonly string _certPath;
        readonly Func<string> _enableTx, _disableTx, _status;
        readonly Dictionary<string, (int fails, DateTime until)> _fails = new();

        volatile RemoteConfig _cfg = new RemoteConfig();
        TcpListener _listener;
        CancellationTokenSource _cts;
        X509Certificate2 _cert;

        public RemoteControlServer(string certPath, Func<string> enableTx,
                                   Func<string> disableTx, Func<string> status)
        {
            _certPath = certPath;
            _enableTx = enableTx; _disableTx = disableTx; _status = status;
        }

        public bool Listening => _listener != null;

        // Call at startup and again whenever settings are saved.
        // Throws if the port can't be opened, so the caller can show the reason.
        public void Apply(RemoteConfig cfg)
        {
            Stop();
            _cfg = cfg;
            if (!cfg.Enabled) return;
            if (!cfg.HasCredentials)
                throw new InvalidOperationException("Set a username, password and passcode first.");

            _cert ??= LoadOrCreateCert();
            var l = new TcpListener(IPAddress.Any, cfg.Port);
            l.Start();
            _listener = l;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _ = Task.Run(() => AcceptLoop(l, token));
            Log($"listening on port {cfg.Port} (remote enable {(cfg.AllowRemoteEnable ? "allowed" : "off")})");
        }

        void Stop()
        {
            try { _cts?.Cancel(); } catch { }
            try { _listener?.Stop(); } catch { }
            _cts = null; _listener = null;
        }

        X509Certificate2 LoadOrCreateCert()
        {
            if (!File.Exists(_certPath))
            {
                using var rsa = RSA.Create(2048);
                var req = new CertificateRequest("CN=WSPR Sked Remote", rsa,
                                                 HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using var tmp = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1),
                                                     DateTimeOffset.UtcNow.AddYears(5));
                File.WriteAllBytes(_certPath, tmp.Export(X509ContentType.Pfx));
            }
            // reload from the PFX - the in-memory key doesn't work with SslStream on Windows
            return new X509Certificate2(_certPath, (string)null, X509KeyStorageFlags.Exportable);
        }

        async Task AcceptLoop(TcpListener l, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient c;
                try { c = await l.AcceptTcpClientAsync(ct); }
                catch { break; }
                _ = Task.Run(() => Handle(c, ct));
            }
        }

        async Task Handle(TcpClient client, CancellationToken ct)
        {
            string ip = (client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "?";
            using (client)
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(10));   // slow or garbage clients get dropped
                    using var ssl = new SslStream(client.GetStream(), false);
                    await ssl.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                    {
                        ServerCertificate = _cert,
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                    }, cts.Token);

                    var cfg = _cfg;
                    var (method, form) = await ReadRequest(ssl, cts.Token);
                    string page = method == "POST" ? await Process(cfg, ip, form) : Page("", cfg);
                    await Send(ssl, page, cts.Token);
                }
                catch { /* malformed, timed out or bad TLS - just drop it */ }
            }
        }

        async Task<string> Process(RemoteConfig cfg, string ip, Dictionary<string, string> f)
        {
            lock (_fails)
                if (_fails.TryGetValue(ip, out var lk) && lk.until > DateTime.UtcNow)
                    return Page("Too many failed attempts - try later.", cfg);

            f.TryGetValue("user", out var u); f.TryGetValue("pass", out var p);
            f.TryGetValue("code", out var c); f.TryGetValue("action", out var action);

            if (!cfg.Check(u, p, c))
            {
                lock (_fails)
                {
                    _fails.TryGetValue(ip, out var fl);
                    int n = fl.fails + 1;
                    _fails[ip] = n >= MaxFails ? (0, DateTime.UtcNow + LockTime) : (n, DateTime.MinValue);
                }
                Log($"DENIED from {ip}");
                await Task.Delay(1000);
                return Page("Access denied.", cfg);
            }

            lock (_fails) _fails.Remove(ip);

            string result;
            try
            {
                switch (action)
                {
                    case "disable":
                        Log($"TX DISABLE from {ip}");
                        result = _disableTx();
                        break;
                    case "enable":
                        if (!cfg.AllowRemoteEnable)
                        {
                            Log($"TX ENABLE refused from {ip} (not allowed in settings)");
                            result = "Remote enable is switched off in the WSPR Sked settings.";
                        }
                        else
                        {
                            Log($"TX ENABLE from {ip}");
                            result = _enableTx();
                        }
                        break;
                    default:
                        result = _status();
                        break;
                }
            }
            catch (Exception ex) { result = "Error: " + ex.Message; }
            return Page(result, cfg);
        }

        static async Task<(string, Dictionary<string, string>)> ReadRequest(Stream s, CancellationToken ct)
        {
            var buf = new byte[8192]; int len = 0, hdrEnd = -1;
            while (hdrEnd < 0 && len < buf.Length)
            {
                int n = await s.ReadAsync(buf.AsMemory(len, buf.Length - len), ct);
                if (n == 0) break;
                len += n;
                hdrEnd = Encoding.ASCII.GetString(buf, 0, len).IndexOf("\r\n\r\n");
            }
            if (hdrEnd < 0) throw new InvalidDataException();

            string head = Encoding.ASCII.GetString(buf, 0, hdrEnd);
            string method = head.Split(' ')[0];
            int cl = 0;
            foreach (var line in head.Split("\r\n"))
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    int.TryParse(line.Substring(15).Trim(), out cl);
            if (cl > 2048) throw new InvalidDataException();

            var body = new List<byte>(buf.Skip(hdrEnd + 4).Take(len - hdrEnd - 4));
            while (body.Count < cl)
            {
                int n = await s.ReadAsync(buf.AsMemory(0, Math.Min(buf.Length, cl - body.Count)), ct);
                if (n == 0) break;
                body.AddRange(buf.Take(n));
            }

            var form = new Dictionary<string, string>();
            foreach (var kv in Encoding.UTF8.GetString(body.ToArray())
                                   .Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = kv.Split('=', 2);
                if (pair.Length == 2)
                    form[Uri.UnescapeDataString(pair[0].Replace('+', ' '))] =
                         Uri.UnescapeDataString(pair[1].Replace('+', ' '));
            }
            return (method, form);
        }

        static async Task Send(Stream s, string html, CancellationToken ct)
        {
            var body = Encoding.UTF8.GetBytes(html);
            var head = Encoding.ASCII.GetBytes(
                "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\n" +
                $"Content-Length: {body.Length}\r\nCache-Control: no-store\r\nConnection: close\r\n\r\n");
            await s.WriteAsync(head, ct);
            await s.WriteAsync(body, ct);
            await s.FlushAsync(ct);
        }

        static string Page(string msg, RemoteConfig cfg)
        {
            string enableBtn = cfg.AllowRemoteEnable
                ? "<button name=action value=enable style='background:#080;color:#fff'>ENABLE TX</button> "
                : "";
            return
                "<html><head><meta name=viewport content='width=device-width,initial-scale=1'></head>" +
                "<body style='font-family:sans-serif;max-width:340px;margin:2em auto'>" +
                $"<h3>WSPR Sked remote</h3><p>{WebUtility.HtmlEncode(msg)}</p>" +
                "<form method=post>User<br><input name=user autocomplete=username><br>" +
                "Password<br><input type=password name=pass autocomplete=current-password><br>" +
                "Passcode<br><input type=password name=code><br><br>" +
                "<button name=action value=status>Status</button> " + enableBtn +
                "<button name=action value=disable style='background:#c00;color:#fff'>DISABLE TX</button>" +
                "</form></body></html>";
        }

        static void Log(string m)
        {
            try { File.AppendAllText(@"C:\Users\Public\wspr_debug.txt", $"{DateTime.Now:HH:mm:ss} REMOTE {m}\n"); }
            catch { }
        }

        public void Dispose() => Stop();
    }
}
