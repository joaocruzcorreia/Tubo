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
       public class PuppetMaster : MarshalByRefObject, IPuppetMaster
    {
           private static String jobTrackerURL;
           private int port;
           public static PuppetMaster pm = null;
           

           public PuppetMaster()
           {
              
           }

        [STAThread]
        public static void Main(){

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            
        }

        public static void runWorker(string[] comand)
        {

                    string workerPath = @"..\..\..\Worker\bin\Debug\Worker.exe";
                   
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.FileName = Path.GetFileName(workerPath);
                    processInfo.WorkingDirectory = Path.GetDirectoryName(workerPath);

                    if (comand.Length == 4) { 
                        processInfo.Arguments = comand[1] + " " + comand[3];
                        jobTrackerURL = comand[3];
                    }
                    else if (comand.Length == 5)
                    {
                        processInfo.Arguments = comand[1] + " " + comand[3] + " " + comand[4];
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
                    Uri pmUri = new Uri(comand[2]);
                    int prt = pmUri.Port;
                    
                    runWorker(comand);

                }
                if (comand[0].Equals("SUBMIT"))
                {
                    
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
                    Debug.WriteLine(jobTrackerURL);
                    jobTracker.GetStatus();

                }

                 if (comand[0].Equals("WAIT")){
                     int secs = int.Parse(comand[1]);
                     Thread.Sleep(secs * 1000);
                }


        }
    }
}
