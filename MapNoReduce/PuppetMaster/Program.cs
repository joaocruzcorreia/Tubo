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

        public static void runUser(string[] comand)
        {

            string userPath = @"..\..\..\Client\bin\Debug\Client\.exe";

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = Path.GetFileName(userPath);
            processInfo.WorkingDirectory = Path.GetDirectoryName(userPath);
            processInfo.Arguments = comand[1] + " " + comand[2] + " " + comand[3] + " " + comand[4] + " " + comand[5] + " " + comand[6];
            Process.Start(processInfo);

        }

        public static void scriptReader(String scriptPath)
        {
            Queue<string> scriptQueue = new Queue<string>();

            foreach (string line in File.ReadLines(scriptPath))
	        { 
                scriptQueue.Enqueue(line);
	        }

            foreach (string command in scriptQueue)
            {
                cmdReader(command);
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

                    runUser(comand);
                    jobTrackerURL = comand[1];

                }

                if (comand[0].Equals("STATUS")){
                    IWorker jobTracker = (IWorker)Activator.GetObject(
                  typeof(IWorker),
                  jobTrackerURL);
                    
                foreach (KeyValuePair<int, string> entry in jobTracker.getWorkersMap() ){
                          string url = entry.Value;
                          IWorker worker = (IWorker)Activator.GetObject(
                          typeof(IWorker),url);           
                          worker.GetStatus();   
                }
            }

             if (comand[0].Equals("WAIT")){
                     int secs = int.Parse(comand[1]);
                     Thread.Sleep(secs * 1000);
                }


        }

        private static void MessageBox(string jobTrackerURL)
        {
            throw new NotImplementedException();
        }
    }
}
