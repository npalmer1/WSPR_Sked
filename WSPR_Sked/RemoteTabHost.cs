using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WSPR_Sked
{
    // Builds a "Remote" tab entirely in code and wires it to RemoteControlServer.
    // Call RemoteTabHost.Install(this, tabControl1) once, from Form1_Load, after
    // Callsign/Frequency/RXblock/enableTXcheckBox/stopTX/wsprTXtimer are all set up.
    public sealed class RemoteTabHost
    {
        RemoteControlServer _server;
        RemoteConfig _cfg;
        readonly string _cfgPath;
        readonly string _certPath;

        TextBox _portBox, _userBox, _pwBox, _codeBox;
        CheckBox _enabledBox, _allowEnableBox;
        Label _statusLabel;
        Button _saveBtn, _testBtn;

        readonly Func<string> _doEnable, _doDisable, _doStatus;

        RemoteTabHost(string userdir, Func<string> doEnable, Func<string> doDisable, Func<string> doStatus)
        {
            _cfgPath = Path.Combine(userdir, "RemoteAccess.json");
            _certPath = Path.Combine(userdir, "RemoteAccess.pfx");
            _doEnable = doEnable; _doDisable = doDisable; _doStatus = doStatus;
        }

        // enableTx / disableTx run on the server's background thread and must
        // marshal back to the UI thread themselves (Invoke) before touching controls.
        public static RemoteTabHost Install(Form owner, TabControl tabs, string userdir,
                                            Func<string> enableTx, Func<string> disableTx, Func<string> status)
        {
            var host = new RemoteTabHost(userdir, enableTx, disableTx, status);
            host.BuildTab(tabs);
            host._cfg = RemoteConfig.Load(host._cfgPath);
            host.LoadIntoUI();
            host._server = new RemoteControlServer(host._certPath, enableTx, disableTx, status);
            host.TryApply(announce: false);
            owner.FormClosing += (s, e) => host._server.Dispose();
            return host;
        }

        void BuildTab(TabControl tabs)
        {
            var page = new TabPage("Remote");
            tabs.TabPages.Add(page);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(16),
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            page.Controls.Add(layout);

            var title = new Label
            {
                Text = "Remote transmit control",
                Font = new Font(page.Font, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.SetColumnSpan(title, 2);
            layout.Controls.Add(title);

            var info = new Label
            {
                Text = "Lets you disable transmit (and optionally re-enable it) remotely over the " +
                       "internet, from a phone browser, using HTTPS with a username, password and passcode.",
                AutoSize = true,
                MaximumSize = new Size(480, 0),
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.SetColumnSpan(info, 2);
            layout.Controls.Add(info);

            _enabledBox = new CheckBox { Text = "Enable remote access", AutoSize = true, Margin = new Padding(0, 4, 0, 8) };
            layout.SetColumnSpan(_enabledBox, 2);
            layout.Controls.Add(_enabledBox);

            AddRow(layout, "Port:", _portBox = new TextBox { Width = 100 });

            _allowEnableBox = new CheckBox
            {
                Text = "Allow remote ENABLE as well as disable",
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 8)
            };
            layout.SetColumnSpan(_allowEnableBox, 2);
            layout.Controls.Add(_allowEnableBox);

            AddRow(layout, "Username:", _userBox = new TextBox { Width = 200 });
            AddRow(layout, "Password:", _pwBox = new TextBox { Width = 200, UseSystemPasswordChar = true });
            AddRow(layout, "Passcode:", _codeBox = new TextBox { Width = 200, UseSystemPasswordChar = true });

            var note = new Label
            {
                Text = "Leave password/passcode blank to keep the existing ones unchanged.",
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(0, 0, 0, 8)
            };
            layout.SetColumnSpan(note, 2);
            layout.Controls.Add(note);

            var btnPanel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 8, 0, 8) };
            _saveBtn = new Button { Text = "Save && Apply", AutoSize = true };
            _saveBtn.Click += SaveBtn_Click;
            _testBtn = new Button { Text = "Show status text", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
            _testBtn.Click += (s, e) => MessageBox.Show(_doStatus(), "Status");
            btnPanel.Controls.Add(_saveBtn);
            btnPanel.Controls.Add(_testBtn);
            layout.SetColumnSpan(btnPanel, 2);
            layout.Controls.Add(btnPanel);

            _statusLabel = new Label { AutoSize = true, MaximumSize = new Size(480, 0) };
            layout.SetColumnSpan(_statusLabel, 2);
            layout.Controls.Add(_statusLabel);
        }

        static void AddRow(TableLayoutPanel layout, string label, Control input)
        {
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
            input.Margin = new Padding(0, 3, 0, 3);
            layout.Controls.Add(input);
        }

        void LoadIntoUI()
        {
            _enabledBox.Checked = _cfg.Enabled;
            _portBox.Text = _cfg.Port.ToString();
            _allowEnableBox.Checked = _cfg.AllowRemoteEnable;
            _userBox.Text = _cfg.User;
            // password/passcode boxes are left blank - never redisplay a hash
        }

        void SaveBtn_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(_portBox.Text.Trim(), out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Enter a valid port number (1-65535).", "Remote access");
                return;
            }
            if (_userBox.Text.Trim() == "")
            {
                MessageBox.Show("Enter a username.", "Remote access");
                return;
            }
            bool havePw = _cfg.HasCredentials;
            if (!havePw && (_pwBox.Text == "" || _codeBox.Text == ""))
            {
                MessageBox.Show("Set a password and passcode - this is the first time remote access is being configured.",
                                 "Remote access");
                return;
            }

            _cfg.Enabled = _enabledBox.Checked;
            _cfg.Port = port;
            _cfg.AllowRemoteEnable = _allowEnableBox.Checked;
            _cfg.User = _userBox.Text.Trim();
            if (_pwBox.Text != "") _cfg.SetPassword(_pwBox.Text);
            if (_codeBox.Text != "") _cfg.SetPasscode(_codeBox.Text);

            _cfg.Save(_cfgPath);
            _pwBox.Text = ""; _codeBox.Text = "";   // never keep plaintext in the UI longer than needed

            TryApply(announce: true);
        }

        void TryApply(bool announce)
        {
            try
            {
                _server.Apply(_cfg);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = _cfg.Enabled
                    ? $"Running on port {_cfg.Port}. Forward this port on your router to reach it from outside."
                    : "Remote access is switched off.";
                if (announce) MessageBox.Show("Saved.", "Remote access");
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.Firebrick;
                _statusLabel.Text = "Could not start: " + ex.Message;
                if (announce) MessageBox.Show(_statusLabel.Text, "Remote access");
            }
        }
    }
}
