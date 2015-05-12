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
           private int port;
           

           public PuppetMaster(int port)
           {
               this.port = port;
               TcpChannel channel = new TcpChannel(port);
               ChannelServices.RegisterChannel(channel, true);
               RemotingConfiguration.RegisterWellKnownServiceType(
                   typeof(IPuppetMaster),
                   "PM",
                   WellKnownObjectMode.Singleton);
           }

        [STAThread]
        public static void Main(){

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }

        public void runWorker(string[] comand)
        {

                    string workerPath = @"..\..\..\Worker\bin\Debug\Worker.exe";
                   
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.FileName = Path.GetFileName(workerPath);
                    processInfo.WorkingDirectory = Path.GetDirectoryName(workerPath);

                    if (comand.Length == 4) { 
                        processInfo.Arguments = comand[1] + " " + comand[2] + " " + comand[3];
                        jobTrackerURL = comand[3];
                    }
                    else if (comand.Length == 5)
                    {
                        processInfo.Arguments = comand[1] + " " + comand[2] + " " + comand[3] + " " + comand[4];
                    }
                    
                    Process.Start(processInfo);
                  
           }

        public static void scriptReader(String scriptPath)
        {
            foreach (string line in File.ReadLines(scriptPath))
	        {
                cmdReader(line);
	        }

        }

        public static void cmdReader(String allInput){
                string[] comand = allInput.Split(' ');

                Console.WriteLine("0 = {0}", comand[0]);


                if (comand[0].Equals("WORKER"))
                {

                    int prt = Int32.Parse(comand[2]);
                    PuppetMaster pm = new PuppetMaster(prt); //antes disto, verificar se o puppet master ja existe
                    pm.runWorker(comand);

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
                if (comand[0].Equals("SLOWW")){
                    IWorker jobTracker = (IWorker)Activator.GetObject(
                     typeof(IWorker),
                     jobTrackerURL);

                    /*  string workerURL = jobTracker.GetWorkerURL(comand[1]);
                      IWorker worker = (IWorker)Activator.GetObject(
                       typeof(IWorker),
                       workerURL);
                       worker.Slow(int.Parse(comand[1]));*/
                }
              /*  if (comand[0].Equals("FREEZEW")

                if (comand[0].Equals("UNFREEZEW")
                if (comand[0].Equals("FREEZEC")
                if (comand[0].Equals("UNFREEZEC")*/

        }
    }
}
