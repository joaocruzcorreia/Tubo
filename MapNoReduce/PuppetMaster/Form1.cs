using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapNoReduce
{
    public partial class Form1 : Form
    {
        private string fileName;

        public Form1()
        {
            InitializeComponent();
        }

        private void runScriptClick(object sender, EventArgs e)
        {
            
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
            PuppetMaster.cmdReader(textBox2.Text);
            textBox2.Text = string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                fileName = dlg.FileName;
                pathLabel.Text = "Script Path -  " + dlg.FileName;
            }
            else
            {
              //  MessageBox("Select script first")
            }
        }

        private void runScriptButton_Click(object sender, EventArgs e)
        {
            if (fileName != null)
            {
                PuppetMaster.scriptReader(fileName);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TcpChannel channel = new TcpChannel(20001); //port
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(new PuppetMaster(), "PM", typeof(IPuppetMaster));
        }
    }
}
