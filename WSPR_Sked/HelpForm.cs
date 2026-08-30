using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WSPR_Sked
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }
        public string helprtf = "";
        private void HelpForm_Load(object sender, EventArgs e)
        {
            LoadHelpRtf(helprtf);
        }

        private void HelpForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //this.Hide();
        }

        MessageClass Msg = new MessageClass();
        private void LoadHelpRtf(string filePath)
        {
            if (!File.Exists(filePath))
            {               
                return;
            }

            try
            {
                richTextBox1.LoadFile(filePath, RichTextBoxStreamType.RichText);
            }
            catch (Exception ex)
            {
                Msg.TMessageBox("Error loading help file: " + ex.Message, "Help file",3000);
            }
        }
    }
}
