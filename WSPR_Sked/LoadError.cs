using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WSPR_Sked
{
    public partial class LoadError : Form
    {
        public string labelText ="";
        public LoadError()
        {
           
            InitializeComponent();
            label1.BringToFront();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() => Application.Exit()));
            //this.Close();
        }

        private void LoadError_Load(object sender, EventArgs e)
        {
            if (labelText == "")
            {
                label1.Text = labelText;
            }
           
            label1.BringToFront();
        }
    }
}
