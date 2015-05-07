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
                   
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.FileName = Path.GetFileName(workerPath);
                    processInfo.WorkingDirectory = Path.GetDirectoryName(workerPath);
                    processInfo.Arguments = comand[1] + " " + comand[2] + " " + comand[3];
                    Process.Start(processInfo);
/*
                    string fullPath = @"C:\Users\Joao\Documents\IST\4 Ano\2 semestre\Plataformas para Aplicações Distribuídas na Internet\Tubo\MapNoReduce\Worker\bin\Release\Worker.exe";
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                    psi.FileName = System.IO.Path.GetFileName(fullPath);
                    psi.WorkingDirectory = System.IO.Path.GetDirectoryName(fullPath);
                    psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    psi.UseShellExecute = true;
                    psi.Arguments = String.Format("SDFSD");
                    process.StartInfo = psi;
                    process.Start();*/
             /*       try
                    {
                        Process.Start(processInfo);
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.NativeErrorCode.ToString());
                    }*/

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
