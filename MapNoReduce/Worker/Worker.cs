using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows.Forms;

namespace MapNoReduce
{
    class Worker
    {
        
        private ConcurrentDictionary<int, string> workersMap;

        private int id;
        private bool isJobTracker;
        private ConcurrentDictionary<int, string> availableWorkers;
        private string status;
        private string previousStatus;
        private bool isFrozen;
        static WorkerServices workerServices;

        public Worker(int id, string serviceURL, string entryURL, bool isJobTracker)
        {
            


            this.isJobTracker = isJobTracker;
            this.workersMap = new ConcurrentDictionary<int, string>();
            this.availableWorkers = new ConcurrentDictionary<int, string>();
            this.status = "Being created";
            this.isFrozen = false;
        }

        public Worker() { }


        //recebe o id, port, serviceURL, entryURL(opcional)
        static void Main(string[] args)
        {
            
            int id = Convert.ToInt32(args[0]);
            string serviceURL = args[1];
            string entryURL = null;
            bool isJobTracker = true; // o worker e' job tracker quando nao existe entryURL
            if (args.Length == 3)
            {
                entryURL = args[2];
                isJobTracker = false;
            }

            workerServices = new WorkerServices(id, serviceURL, entryURL, isJobTracker);

            //this.status = "Creating channel";
            Uri sUri = new Uri(serviceURL);
            //registo do worker
            TcpChannel channel = new TcpChannel(sUri.Port); //port
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(workerServices, "W", typeof(IWorker));

            workerServices.Init();            
            
            Console.ReadLine();
        }


       

        //////////////////////////////////////////////////////////////////////
        //                                                                  //
        // MÉTODOS DO JOB TRACKER                                           //
        //                                                                  //
        //////////////////////////////////////////////////////////////////////
        


        //////////////////////////////////////////////////////////////////////
        //                                                                  //
        // MÉTODOS DOS WORKERS                                              //
        //                                                                  //
        //////////////////////////////////////////////////////////////////////
        

        
        
        //////////////////////////////////////////////////////////////////////
        //                                                                  //
        // COMANDOS DO PUPPET MASTER                                        //
        //                                                                  //
        //////////////////////////////////////////////////////////////////////

        



    }
}
