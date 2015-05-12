﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace MapNoReduce
{
    class WorkerServices : MarshalByRefObject, IWorker
    {

        private int id;
        private int port; //falta fazer
        
        private string serviceURL;
        private string entryURL; // apenas utilizado se o worker for jobTracker
        private bool isJobTracker;
        
        private string status;
        private string previousStatus;

        private ConcurrentDictionary<int, string> workersMap;
        private ConcurrentDictionary<int, string> availableWorkers;

        public WorkerServices(int id, string serviceUrl, string entryUrl, bool isJobTracker)
        {
            this.id = id;
            this.serviceURL = serviceURL;
            this.entryURL = entryURL;
            this.isJobTracker = isJobTracker;
        }


        public void Init()
        {
            

            /*
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IWorker),
                "W",
                WellKnownObjectMode.Singleton);
            */
            Console.WriteLine(port);

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

        public void ProcessSplit(long splitStart, long splitEnd, string clientURL, string mapClass, byte[] dll, int splitNumber)
        {

            IWorker jobTracker = (IWorker)Activator.GetObject(
                typeof(IWorker),
                entryURL);
            jobTracker.RemoveAvailableWorker(this.id, this.serviceURL);

            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            IClient client = (IClient)Activator.GetObject(
                             typeof(IClient),
                             clientURL + @"\C");

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
                            result.Concat((IList<KeyValuePair<string, string>>)resultObject);
                        }
                    }
                }
            }

            //depois de tudo processado:
            client.SubmitResultService(result, splitNumber);

            jobTracker.AddAvailableWorker(this.id, this.serviceURL);
        }

        public void SubmitJobToWorker(long fileSize, int nSplits, string clientURL, string mapClass, byte[] dll)
        {
            this.status = "Submiting jobs to the workers";

            long splitSize = SplitSize(fileSize, nSplits);
            long splitStart = 0;
            long splitEnd = splitSize - 1;

            for (int i = 0; i < nSplits; i++)
            {
                // bloqueia enquanto nao houver workers disponiveis
                while (availableWorkers.IsEmpty) { }

                KeyValuePair<int, string> entry = availableWorkers.First();

                IWorker worker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    entry.Value + @"\W");

                worker.ProcessSplit(splitStart, splitEnd, clientURL, mapClass, dll, i + 1);

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


        public string GetWorkerURL(int id)
        {
            if (workersMap.ContainsKey(id))
                return workersMap[id];
            else
                return null;
        }

        public void SetWorkersMap(ConcurrentDictionary<int, string> oldWorkersMap)
        {
            this.workersMap = oldWorkersMap;
        }

        public void GetStatus()
        {

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

        public void Slow(int sec)
        {
            throw new NotImplementedException();
        }

        public void FreezeWorker()
        {
            throw new NotImplementedException();
        }

        public void UnfreezeWorker()
        {
            throw new NotImplementedException();
        }

        public void FreezeCommunication()
        {
            throw new NotImplementedException();
        }

        public void UnfreezeCommunication()
        {
            throw new NotImplementedException();
        }
    }
}