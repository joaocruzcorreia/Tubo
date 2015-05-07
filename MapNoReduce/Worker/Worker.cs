using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace MapNoReduce
{
    class Worker : MarshalByRefObject, IWorker
    {
        private int id;
        private ConcurrentDictionary<int, string> workersMap;
        private string serviceURL;
        private string entryURL; // apenas utilizado se o worker for jobTracker
        private bool isJobTracker;
        private ConcurrentDictionary<int, string> availableWorkers;
        private string status;
        private string previousStatus;
        private bool isFrozen;


        public Worker(int id, string serviceURL, string entryURL, bool isJobTracker)
        {
            this.id = id;
            this.serviceURL = serviceURL;
        
            this.entryURL = entryURL;
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
            Console.WriteLine("ID");
            Console.ReadLine();
            
            int id = Convert.ToInt32(args[0]);
            String pm = args[1];
            string serviceURL = args[2];
            string entryURL = null;
            bool isJobTracker = true; // o worker e' job tracker quando nao existe entryURL
            if (args.Length == 4)
            {
                entryURL = args[3];
                isJobTracker = false;
            }

            Worker worker = new Worker(id, serviceURL, entryURL, isJobTracker);
            worker.Init();
        }


        public void Init()
        {
            this.status = "Creating channel";

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
                jobTracker.AddAvailableWorker(id, serviceURL);

            }

        }

        //////////////////////////////////////////////////////////////////////
        //                                                                  //
        // MÉTODOS DO JOB TRACKER                                           //
        //                                                                  //
        //////////////////////////////////////////////////////////////////////
        public void SubmitJobToWorker(long fileSize, int nSplits, string clientURL, string mapClass, byte[] dll)
        {
            this.status = "Submiting jobs to the workers";

            long splitSize = SplitSize(fileSize, nSplits);
            long splitStart = 0;
            long splitEnd = splitSize - 1;

            for (int i=0; i<nSplits; i++)
            {
                // bloqueia enquanto nao houver workers disponiveis
                while (availableWorkers.IsEmpty) { }

                KeyValuePair<int, string> entry = availableWorkers.First();
                
                IWorker worker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    entry.Value + @"\W");

                worker.ProcessSplit(splitStart, splitEnd, clientURL, mapClass, dll, i+1);

                splitStart += splitSize;
                if (splitEnd + splitSize > fileSize)
                    splitEnd = fileSize; //ou splitEnd = fileSize-1;
                else
                    splitEnd += splitSize;
            }
        }

        // devolve o tamanho do split a ser utilizado
        private long SplitSize(long fileSize, int nSplits)
        {
            long splitSize;
            if (fileSize % nSplits > 0) // resto
                splitSize = fileSize / (nSplits - 1); //se resto >0 e necessario mais um split para o resto
            else
                splitSize = fileSize / nSplits;

            return splitSize;
        }

        //apenas utilizado pelo job tracker
        //adiciona um worker ao workersMap
        public void AddWorker(int id, string serviceURL)
        {

            //workersMap.AddOrUpdate(id, serviceURL, (k,v) => serviceURL);
            workersMap.TryAdd(id, serviceURL);

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

        public void AddAvailableWorker(int id, string serviceURL)
        {
            availableWorkers.TryAdd(id, serviceURL);
        }

        public void RemoveAvailableWorker(int id, string serviceURL)
        {
            availableWorkers.TryRemove(id, out serviceURL);
        }



        //////////////////////////////////////////////////////////////////////
        //                                                                  //
        // MÉTODOS DOS WORKERS                                              //
        //                                                                  //
        //////////////////////////////////////////////////////////////////////
        public void ProcessSplit(long splitStart, long splitEnd, string clientURL, string mapClass, byte[] dll, int splitNumber){
            
            IWorker jobTracker = (IWorker)Activator.GetObject(
                typeof(IWorker),
                entryURL);
            jobTracker.RemoveAvailableWorker(this.id, this.serviceURL);

            IList<KeyValuePair<string, string>> result = null;
            
            IClient client = (IClient)Activator.GetObject(
                             typeof(IClient),
                             clientURL + @"\C");

            string split = client.GetSplitService(splitStart, splitEnd);

            Assembly assembly = Assembly.Load(dll);

            string[] delimitors = { "\n", "\r\n" };
            string[] splitPart = split.Split(delimitors, StringSplitOptions.None);

            foreach(string s in splitPart)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass == true)
                    {
                        if (type.FullName.EndsWith("." + mapClass))
                        {
                            // create an instance of the object
                            object ClassObj = Activator.CreateInstance(type);

                            // Dynamically Invoke the method
                            object[] args = new object[] { s }; //parse split 1 linha de cada vez
                            object resultObject = type.InvokeMember("Map",
                              BindingFlags.Default | BindingFlags.InvokeMethod,
                                   null,
                                   ClassObj,
                                   args);
                            result = (IList<KeyValuePair<string, string>>)resultObject;
                        }
                    }
                }
            }

            //depois de tudo processado:
            client.SubmitResultService(result, splitNumber);

            jobTracker.AddAvailableWorker(this.id, this.serviceURL);
         }

        public void SetWorkersMap(ConcurrentDictionary<int, string> oldWorkersMap){
            this.workersMap = oldWorkersMap;
        }
        
        //////////////////////////////////////////////////////////////////////
        //                                                                  //
        // COMANDOS DO PUPPET MASTER                                        //
        //                                                                  //
        //////////////////////////////////////////////////////////////////////

        public void GetStatus(){

            Console.WriteLine("ID = {0} <-> serviceURL: {1} <-> isJobTracker: {2} <-> Status: {3}", this.id, this.serviceURL, this.isJobTracker, this.status);
 
        }

        public void GetWorkersStatus()
        {
            foreach (KeyValuePair<int, string> entry in workersMap)
            {
                if (entry.Key != this.id)
                {
                    IWorker worker = (IWorker)Activator.GetObject(
                        typeof(IWorker),
                        entry.Value);
                    worker.GetStatus();
                }
            }
            GetStatus();
        }

        public void Slow(int sec){
            previousStatus = status;
            status = "Sleeping";
            Thread.Sleep(sec*1000);
            status = previousStatus;
        }
        
        public void FreezeWorker(){
            previousStatus = status;
            status = "Frozen";
            this.isFrozen = true;
        }

        public void UnfreezeWorker(){
            this.isFrozen = false;
            status = previousStatus;
        }

        public void FreezeCommunication(){
            //TODO
        }

        public void UnfreezeCommunication(){
            //TODO
        }
    }
}
