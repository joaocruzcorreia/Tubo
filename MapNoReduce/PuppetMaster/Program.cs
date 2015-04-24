using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;

namespace MapNoReduce
{
       public class PuppetMaster
    {
        [STAThread]

        public void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            int prt = 10001;
            TcpChannel channel = new TcpChannel(prt);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IPuppetMaster),
                "PM",
                WellKnownObjectMode.Singleton);
        }

        public static void cmdReader(String allInput){
                string[] comand = allInput.Split(' ');

                if (comand[0].Equals("worker"))
                {

                }
                if (comand[0].Equals("submit"))
                {
                    String c = "criarCliente";
                    IClient client = (IClient)Activator.GetObject(
                       typeof(IClient),
                     "tcp://localhost:10001/C");
                    client.Init(comand[1]);

                    client.Submit(comand[2], Int32.Parse(comand[4]), comand[3], comand[5], comand[6]);
                }
                if (comand[0].Equals("status")){

                }
                 if (comand[0].Equals("wait")){

                }
                
        
        }
    }
}
