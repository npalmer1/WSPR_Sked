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
        public LoadError()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() => Application.Exit()));
            //this.Close();
        }

        private void LoadError_Load(object sender, EventArgs e)
        {
            this.BringToFront();
        }
    }
}
