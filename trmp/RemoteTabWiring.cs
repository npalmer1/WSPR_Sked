using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WSPR_Sked
{
    // Binds RemoteControlServer to the "Remote" tab controls declared in
    // Form1.Designer.cs (remoteEnabledCheckBox, remotePortTextBox, etc).
    // Call RemoteTabWiring.Install(this) once from Form1_Load, after
    // Callsign/Frequency/enableTXcheckBox/stopTX/wsprTXtimer are set up.
    public sealed class RemoteTabWiring
    {
        RemoteControlServer _server;
        RemoteConfig _cfg;
        readonly string _cfgPath;
        readonly string _certPath;
        readonly Form1 _form;

        RemoteTabWiring(Form1 form, string userdir)
        {
            _form = form;
            _cfgPath = Path.Combine(userdir, "RemoteAccess.json");
            _certPath = Path.Combine(userdir, "RemoteAccess.pfx");
        }

        public static RemoteTabWiring Install(Form1 form, string userdir,
                                              Func<string> enableTx, Func<string> disableTx, Func<string> status)
        {
            var w = new RemoteTabWiring(form, userdir);
            w._cfg = RemoteConfig.Load(w._cfgPath);
            w.LoadIntoUI();

            w._server = new RemoteControlServer(w._certPath, enableTx, disableTx, status);
            w.TryApply(announce: false);

            form.remoteSaveButton.Click += (s, e) => w.Save();
            form.remoteStatusButton.Click += (s, e) => MessageBox.Show(status(), "Status");
            form.FormClosing += (s, e) => w._server.Dispose();
            return w;
        }

        void LoadIntoUI()
        {
            _form.remoteEnabledCheckBox.Checked = _cfg.Enabled;
            _form.remotePortTextBox.Text = _cfg.Port.ToString();
            _form.remoteAllowEnableCheckBox.Checked = _cfg.AllowRemoteEnable;
            _form.remoteUserTextBox.Text = _cfg.User;
            // password/passcode left blank - never redisplay a hash
        }

        void Save()
        {
            if (!int.TryParse(_form.remotePortTextBox.Text.Trim(), out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Enter a valid port number (1-65535).", "Remote access");
                return;
            }
            if (_form.remoteUserTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Enter a username.", "Remote access");
                return;
            }
            bool havePw = _cfg.HasCredentials;
            if (!havePw && (_form.remotePasswordTextBox.Text == "" || _form.remotePasscodeTextBox.Text == ""))
            {
                MessageBox.Show("Set a password and passcode - this is the first time remote access is being configured.",
                                 "Remote access");
                return;
            }

            _cfg.Enabled = _form.remoteEnabledCheckBox.Checked;
            _cfg.Port = port;
            _cfg.AllowRemoteEnable = _form.remoteAllowEnableCheckBox.Checked;
            _cfg.User = _form.remoteUserTextBox.Text.Trim();
            if (_form.remotePasswordTextBox.Text != "") _cfg.SetPassword(_form.remotePasswordTextBox.Text);
            if (_form.remotePasscodeTextBox.Text != "") _cfg.SetPasscode(_form.remotePasscodeTextBox.Text);

            _cfg.Save(_cfgPath);
            _form.remotePasswordTextBox.Text = "";
            _form.remotePasscodeTextBox.Text = "";

            TryApply(announce: true);
        }

        void TryApply(bool announce)
        {
            try
            {
                _server.Apply(_cfg);
                _form.remoteStatusLabel.ForeColor = Color.DarkGreen;
                _form.remoteStatusLabel.Text = _cfg.Enabled
                    ? $"Running on port {_cfg.Port}. Forward this port on your router to reach it from outside."
                    : "Remote access is switched off.";
                if (announce) MessageBox.Show("Saved.", "Remote access");
            }
            catch (Exception ex)
            {
                _form.remoteStatusLabel.ForeColor = Color.Firebrick;
                _form.remoteStatusLabel.Text = "Could not start: " + ex.Message;
                if (announce) MessageBox.Show(_form.remoteStatusLabel.Text, "Remote access");
            }
        }
    }
}
