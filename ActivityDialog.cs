using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tomato
{
    public partial class ActivityDialog : Form
    {
        public string Input
        {
            get
            {
                return textBox1.Text;
            }
        }

        public ActivityDialog(string prefill = null)
        {
            InitializeComponent();

            if (!String.IsNullOrWhiteSpace(prefill))
            {
                textBox1.Text = prefill;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
