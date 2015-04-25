using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        private int port;
        private IDictionary<int, string> workersMap;
        private string serviceURL;
        private string entryURL; // apenas utilizado se o worker for jobTracker
        private bool isJobTracker;
        private ConcurrentQueue<int> availableWorkersList = new ConcurrentQueue<int>();


        public ConcurrentQueue<int> AvalilableWorkersList{
            get{return this.availableWorkersList;}
            set{this.availableWorkersList = value;}
        }


        public Worker(int id, string serviceURL, int port, string entryURL, bool isJobTracker)
        {
            this.id = id;
            this.serviceURL = serviceURL;
            this.port = port;
            this.entryURL = entryURL;
            this.isJobTracker = isJobTracker;
            this.workersMap = new ConcurrentDictionary<int, string>();
        }

        public Worker() { }


        //recebe o id, port, serviceURL, entryURL(opcional)
        static void Main(string[] args)
        {   
            int id = Convert.ToInt32(args[0]);
            int port = Convert.ToInt32(args[1]);
            string serviceURL = args[2];
            string entryURL = null;
            bool isJobTracker = true; // o worker e job tracker quando nao existe entryURL
            if (args.Length == 4)
            {
                entryURL = args[3];
                isJobTracker = false;
            }

            Worker worker = new Worker(id, serviceURL, port, entryURL, isJobTracker);
            worker.Init();
            
        }

        public void Init()
        {
            //registo do worker
            TcpChannel channel = new TcpChannel(port); //port
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IWorker),
                "W",
                WellKnownObjectMode.Singleton);

            if (isJobTracker)
                AddWorker(this.id, this.entryURL);
            else
            {
                IWorker jobTracker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    entryURL);
                jobTracker.AddWorker(this.id, this.serviceURL);
            }

        }

        public void SubmitJobToWorker(long fileSize, int nSplits, string clientURL)
        {
            long splitSize = SplitSize(fileSize, nSplits);
            long splitStart = 0;
            long splitEnd = splitSize - 1;
            int lastUsedID = 0;

            /*
            IWorker worker = (IWorker)Activator.GetObject(
                typeof(IWorker),
                availableWorkersList[0] + "/W");//alterar indice para lastUsedID and deixar o indice 0, mas remover o worker da lista sempre que ele tiver a fazer algo OU AINDA criar um boleano no worker para ver se ele está ocupado ou não
                         
            worker.ProcessSplit(splitStart, splitEnd, port);*/

            for (int i=0; i<nSplits; i++)
            {

                IWorker worker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    entry.Value/* + "/W"*/);

                worker.ProcessSplit(splitStart, splitEnd, clientURL);

                splitStart += splitSize;
                if (splitEnd + splitSize > fileSize)
                    splitEnd = fileSize; //ou splitEnd = fileSize-1;
                else
                    splitEnd += splitSize;

            }

            //ainda sobram splits

        }

        public void ProcessSplit(long splitStart, long splitEnd, string clientURL){
            IList<KeyValuePair<string, string>> result = null;

            IClient client = (IClient)Activator.GetObject(
                             typeof(IClient),
                             "tcp://localhost:" + (port.ToString()) + "/C");

            string partialSplitString = client.GetSplitService(splitStart, splitEnd);//verificar


             //depois de tudo processado:

            SubmitResult(result, client);
         }


        public void GetJob()
        {
            throw new NotImplementedException();
        }

        public void SubmitResult(IList<KeyValuePair<string, string>> result, IClient client)
        {
            //client.receiveSplitResult(result);
        }


        // devolve o tamanho do split a ser utilizado
        private long SplitSize(long fileSize, int nSplits){

            long splitSize;
            if (fileSize % nSplits > 0) // resto
                splitSize = fileSize/ (nSplits - 1); //se resto >0 e necessario mais um split para o resto
            else
                splitSize = fileSize / nSplits;

            return splitSize;
        }

        //apenas utilizado pelo job tracker
        //adiciona um worker ao workersMap
        public void AddWorker(int id, string serviceURL){

            //workersMap.AddOrUpdate(id, serviceURL, (k,v) => serviceURL);
            workersMap.Add(id, serviceURL);

            foreach (KeyValuePair<int, string> entry in workersMap)
            {
                if (entry.Key != this.id)
                {
                    IWorker worker = (IWorker)Activator.GetObject(
                        typeof(IWorker),
                        entry.Value);
                    worker.SetWorkersMap(this.workersMap);
                }
            }
        }

        public void SetWorkersMap(IDictionary<int, string> oldWorkersMap){
            this.workersMap = oldWorkersMap;
        }

        public void AddAvailableWorker(int id)
        {
            //TODO
        }

        public void RemoveAvailableWorker(int id)
        {
            //TODO
        }

        
        // apenas usado pelos workers. nao pode ser usado pelo job tracker
        private void Available()
        {
            IWorker jobTracker = (IWorker) Activator.GetObject(
                typeof(IWorker),
                entryURL);
 
            jobTracker.AddAvailableWorker(this.id);
        }
 
        // apenas usado pelos workers. nao pode ser usado pelo job tracker
        private void NotAvailabe()  {
            IWorker jobTracker = (IWorker) Activator.GetObject(
                typeof(IWorker),
                entryURL);
 
            jobTracker.RemoveAvailableWorker(this.id);
        }
 
        //apenas usado pelo job tracker
        private bool IsWorkerAvailable(int id){
            foreach (int i in availableWorkersList)
            {
                if (i == id)
                {
                    return true;
                }
            }

            return false;
        }
 
    }
}
