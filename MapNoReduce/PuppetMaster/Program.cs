using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows.Forms;
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
           private static String jobTrackerURL;


        public static void Main()
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

                Console.WriteLine("0 = {0}", comand[0]);

                if (comand[0].Equals("WORKER"))
                {

                    
                    string workerPath = @"..\..\..\Worker\bin\Debug\Worker.exe";
                   
                    ProcessStartInfo processInfo = new ProcessStartInfo(workerPath);
                    processInfo.Arguments = comand[1] + " " + comand[3] + " " + comand[4];
                    processInfo.FileName = workerPath;
                    Process.Start(processInfo);
                  
                }
                if (comand[0].Equals("SUBMIT"))
                {
                    String c = "criarCliente";
                    IClient client = (IClient)Activator.GetObject(
                       typeof(IClient),
                     "tcp://localhost:10001/C");
                    client.Init(comand[1]);
                    jobTrackerURL = comand[1];

                    client.Submit(comand[2], Int32.Parse(comand[4]), comand[3], comand[5], comand[6]);
                    

                }
                if (comand[0].Equals("STATUS")){

                    IWorker jobTracker = (IWorker)Activator.GetObject(
                     typeof(IWorker),
                     jobTrackerURL);

                    jobTracker.GetWorkersStatus();

                }
                 if (comand[0].Equals("WAIT")){
                     int secs = int.Parse(comand[1]);
                     Thread.Sleep(secs * 1000);
                }
                
        
        }
    }
}
