﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PADIMapNoReduce
{
    class WorkerServices : MarshalByRefObject, IWorker
    {

        private int id;
        private int port;
        private string serviceURL;
        private string entryURL; // apenas utilizado se o worker for jobTracker
        private bool isJobTracker;
        private string status;
        private static ConcurrentDictionary<int, string> workersMap = new ConcurrentDictionary<int, string>();
        //private static ConcurrentDictionary<int, string> availableWorkers = new ConcurrentDictionary<int, string>();
        private static ConcurrentStack<string> availableWorkers = new ConcurrentStack<string>();
        public static AutoResetEvent[] handles;

        public WorkerServices(int id, string serviceUrl, string entryUrl, bool isJobTracker, int port, string status)
        {
            this.id = id;
            this.serviceURL = serviceUrl;
            this.entryURL = entryUrl;
            this.isJobTracker = isJobTracker;
            this.port = port;
            this.status = status;
        }

        public ConcurrentDictionary<int, string> getWorkersMap()
        {
            return workersMap;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }


        public void Init()
        {
            AddWorker(this.id, this.serviceURL);                
            AddAvailableWorker(this.id, this.serviceURL);               
            Console.WriteLine("added to workersMap and availableWorkers");
        }


        //////////////////////////////////////////////////////////////////////
        //                                                                  //
        // MÉTODOS DOS WORKERS                                              //
        //                                                                  //
        //////////////////////////////////////////////////////////////////////   

        public void ProcessSplit(long splitStart, long splitEnd, string clientURL, string mapClass, byte[] dll, int splitNumber)
        {
            status = "Processing split number " + splitNumber;

            Console.WriteLine(status);

            //RemoveAvailableWorker(this.id, this.serviceURL);

            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            IClient client = (IClient)Activator.GetObject(
                             typeof(IClient),
                             clientURL);

            string split = client.GetSplitService(splitStart, splitEnd);
            Assembly assembly = Assembly.Load(dll);
            string[] delimitors = { "\n", "\r\n" };
            string[] splitPart = split.Split(delimitors, StringSplitOptions.None);

            foreach (string s in splitPart)
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
                            result = new List<KeyValuePair<string, string>>(result.Concat((IList<KeyValuePair<string, string>>)resultObject));
                        }
                    }
                }

                handles[this.id - 1].Set(); 
            }

            //depois de tudo processado:
            client.SubmitResultService(result, splitNumber);
            AddAvailableWorker(this.id, this.serviceURL);
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
            long splitEnd = splitSize;
            
            handles = new AutoResetEvent[workersMap.Count];
            for (int i = 0; i < handles.Length; i++ )
                handles[i] = new AutoResetEvent(false);



            for (int i = 0; i < nSplits; i++)
            {
                // bloqueia enquanto nao houver workers disponiveis
                while (availableWorkers.IsEmpty) { }

         /*       KeyValuePair<int, string> entry = availableWorkers.First();
                Console.WriteLine("Assigning job to worker id {0}", entry.Key);
                IWorker worker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    entry.Value);*/

                string workerURL = RemoveAvailableWorker();
                //availableWorkers.TryPeek(out workerURL);

                Console.WriteLine("Assigning job to worker id {0}", workerURL);
                IWorker worker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    workerURL);

                Thread workerThread = new Thread(()=>worker.ProcessSplit(splitStart, splitEnd, clientURL, mapClass, dll, i + 1));
                workerThread.Start();

                splitStart += splitSize;
                if (splitEnd + splitSize > fileSize)
                    splitEnd = fileSize - 1; //ou splitEnd = fileSize;
                else
                    splitEnd += splitSize;
            }
            WaitHandle.WaitAll(handles);
        }

        private string RemoveAvailableWorker()
        {
            string workerURL;
            while(!availableWorkers.TryPop(out workerURL));
            return workerURL;
        }

        // devolve o tamanho do split a ser utilizado
        private long SplitSize(long fileSize, int nSplits)
        {
            long splitSize;
            if (fileSize % nSplits > 0) // resto
                splitSize = fileSize / nSplits + 1; //se resto >0 e necessario mais um split para o resto
            else
                splitSize = fileSize / nSplits;

            return splitSize;
        }

        //adiciona um worker ao workersMap
        public void AddWorker(int id, string serviceURL)
        {
            workersMap.TryAdd(id, serviceURL);
        }
        public void AddAvailableWorker(int id, string serviceURL)
        {
            Console.WriteLine("id {0}   serviceURL {1}", id, serviceURL);
            availableWorkers.Push(serviceURL);
        }

        public void RemoveAvailableWorker(int id, string serviceURL)
        {
            while(!availableWorkers.TryPop(out serviceURL));
        }

        public string GetWorkerURL(int id)
        {
            if (workersMap.ContainsKey(id))
                return workersMap[id];
            else
                return null;
        }

        public void GetStatus()
        {
            Console.WriteLine("ID = {0} <-> serviceURL: {1} <-> isJobTracker: {2} <-> Status: {3}",
                this.id, this.serviceURL, this.isJobTracker, this.status);
        }

        public void Slow(int sec)
        {
            throw new NotImplementedException();
        }
    }
}
