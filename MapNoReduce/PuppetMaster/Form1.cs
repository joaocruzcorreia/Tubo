using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapNoReduce
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void runScriptClick(object sender, EventArgs e)
        {
            PuppetMaster.scriptReader(scriptTb.Text);
        }

        private void scriptPathTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void commandButton_Click(object sender, EventArgs e)
        {
            PuppetMaster.cmdReader(commandTb.Text);
            commandTb.Text = string.Empty;
        }
    }
}
