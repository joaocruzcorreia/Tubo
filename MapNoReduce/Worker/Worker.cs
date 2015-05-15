using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PADIMapNoReduce
{
    class Worker
    {
     
        private static string status;
        static WorkerServices workerServices;
        public static string jobTrackerURL = null;

        //recebe o id, serviceURL, entryURL(opcional)
        static void Main(string[] args)
        {
            
            int id = Convert.ToInt32(args[0]);
            string serviceURL = args[1];
            string entryURL = null;
            bool isJobTracker = true; // o worker e' job tracker quando nao existe entryURL
            jobTrackerURL = serviceURL; 
            if (args.Length == 3)
            {
                entryURL = args[2];
                isJobTracker = false;
            }


       

                status = "Creating channel";

                Uri sUri = new Uri(serviceURL);
                workerServices = new WorkerServices(id, serviceURL, entryURL, isJobTracker, sUri.Port, status);

                //registo do worker
                TcpChannel channel = new TcpChannel(sUri.Port); //port
                ChannelServices.RegisterChannel(channel, false);
                RemotingServices.Marshal(workerServices, "W", typeof(IWorker));

                Console.WriteLine(sUri.Port);

                workerServices.Init();

                Console.ReadLine();
            
        }
    }
}
