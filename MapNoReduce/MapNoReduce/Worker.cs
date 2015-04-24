using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Concurrent;

namespace MapNoReduce
{
    class Worker : MarshalByRefObject, IWorker
    {
        private int id;
        private string serviceURL;
        private int port;
        private string entryURL;
        private bool isJobTracker;
        private IClient client;
        private ConcurrentDictionary<int, string> workersMap = new ConcurrentDictionary<int, string>();
        private IList<string> listaDeWorkersDisponiveis = new List<string>();

        //recebe o id, port, serviceURL, entryURL(opcional)
        static void Main(string[] args)
        {   
            int id = Convert.ToInt32(args[0]);

            TcpChannel channel = new TcpChannel(Convert.ToInt32(args[1])); //port
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IWorker),
                "W",
                WellKnownObjectMode.Singleton);

            
            string serviceURL = args[2];
            string entryURL = "";
            if (args.Length == 4)
                entryURL = args[3];

            // set variable isTracker
            // When the value of 'entryURL' is the empty string this means that the current worker
            // instance will also assume the 'tracker' role
            // When the value of 'entryURL' is a non-empty string this means that the
            // Expose worker services at serviceURL
    /*        if (entryURL == string.Empty)
                isTracker = true;
            // set variable trackerURL
            trackerURL = isTracker ? serviceURL : entryURL;
            if (isTracker)
            {
                // Current worker is the tracker
                // The tracker is the first worker to be added to the workers table
                WorkerServices ws = new WorkerServices();
                ws.registerAtTracker(id, serviceURL);
                // ws.printWorkersTable();
            }
            else
            {
                // Current worker is not the tracker
                // Make a remote call to register at tracker
                WorkerServices rws = (WorkerServices)Activator.GetObject(
                    typeof(WorkerServices),
                     entryURL + "/W");
                rws.registerAtTracker(id, serviceURL);
                // rws.printWorkersTable();
            }
            System.Console.WriteLine("Press <enter> to terminate server...");
            System.Console.ReadLine(); */
        }

        public Worker(int id, int port, string entryURL)
        {
            this.id = id;
            this.port = port;
            this.entryURL = entryURL;
        }

        public void Init()
        {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            client = (IClient) Activator.GetObject(typeof(IClient), entryURL);
        }

        public void SubmitJob(long fileSize, int nSplits, int port)
        {
            long splitSize = SplitSize(fileSize, nSplits);
            int splitStart = 0;
            int splitEnd = 0;
            int lastUsedID = 0;

            IWorker worker = (IWorker)Activator.GetObject(
                typeof(IWorker),
                listaDeWorkersDisponiveis[0] + "/W");
                        
            worker.processSplit(splitStart, splitEnd, port);

        }

         public void processSplit(int splitStart, int splitEnd, int port){
            IList<KeyValuePair<string, string>> result = null;

            IClient client = (IClient)Activator.GetObject(
                             typeof(IClient),
                             "tcp://localhost:" + (port.ToString()) + "/C");

            string partialSplitString = client.GetSplitService(splitStart, splitEnd);//verificar

         }

        h

        public void GetJob()
        {
            throw new NotImplementedException();
        }

        public void SubmitResult()
        {
            throw new NotImplementedException();
        }

        private long SplitSize(long fileSize, int nSplits){

            long splitSize;
            if (fileSize % nSplits > 0) // resto
                splitSize = fileSize/ (nSplits - 1); //se resto >0 e necessario mais um split para o resto
            else
                splitSize = fileSize / nSplits;

            return splitSize;
        }
    }
}
