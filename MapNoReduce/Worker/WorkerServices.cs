using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
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
        private ConcurrentDictionary<int, string> workersMap = new ConcurrentDictionary<int, string>();
        private ConcurrentDictionary<int, string> availableWorkers = new ConcurrentDictionary<int, string>();

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
            //Console.WriteLine(serviceURL);
            if (isJobTracker)
            {
                //Console.WriteLine("Job Tracker");
                AddWorker(this.id, this.serviceURL);
                AddAvailableWorker(this.id, this.serviceURL);
            }
            else
            {
                //Console.WriteLine("NOT JT");
                //Console.WriteLine(entryURL);
                IWorker jobTracker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    entryURL);
                jobTracker.AddWorker(this.id, this.serviceURL);
                jobTracker.AddAvailableWorker(this.id, this.serviceURL);
            }
        }


        //////////////////////////////////////////////////////////////////////
        //                                                                  //
        // MÉTODOS DOS WORKERS                                              //
        //                                                                  //
        //////////////////////////////////////////////////////////////////////   

        public void ProcessSplit(long splitStart, long splitEnd, string clientURL, string mapClass, byte[] dll, int splitNumber)
        {
            IWorker jobTracker = null;

            status = "Processing split";

            //Console.WriteLine();
            //Console.WriteLine("estou a processar um split");

            if (isJobTracker)
            {
                RemoveAvailableWorker(this.id, this.serviceURL);
                //Console.WriteLine("sou job tracker");
            }
            else
            {
                jobTracker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    entryURL);
                jobTracker.RemoveAvailableWorker(this.id, this.serviceURL);
                //Console.WriteLine("nao sou job tracker");
            }

            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            //Console.WriteLine("a contactar cliente");

            IClient client = (IClient)Activator.GetObject(
                             typeof(IClient),
                             clientURL);
            //Console.WriteLine(clientURL);

            //Console.WriteLine("a obter split");
            //Console.WriteLine("split start {0}  ----- split end {1}", splitStart, splitEnd);

            string split = client.GetSplitService(splitStart, splitEnd);
         //   Console.WriteLine(split[0]);
         //   Console.ReadLine();
            Assembly assembly = Assembly.Load(dll);

            string[] delimitors = { "\n", "\r\n" };

            //Console.WriteLine("a fazer split do split");

            string[] splitPart = split.Split(delimitors, StringSplitOptions.None);

            //Console.WriteLine("a entrar no ciclo para fazer o job");
            try
            {
                assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine("excepcao");
                foreach (Exception inner in ex.LoaderExceptions)
                {
                    Console.WriteLine(inner.Message);
                }
            }
            foreach (string s in splitPart)
            {
                Console.WriteLine(s);
                foreach (Type type in assembly.GetTypes())
                {
                    Console.WriteLine("primeiro if");
                    if (type.IsClass == true)
                    {
                        //Console.WriteLine("segunfo if");
                        if (type.FullName.EndsWith("." + mapClass))
                        {
                            //Console.WriteLine("terceiro if");
                            // create an instance of the object
                            object ClassObj = Activator.CreateInstance(type);
                            //Console.WriteLine("instancia criada");
                            // Dynamically Invoke the method
                            object[] args = new object[] { s }; //parse split 1 linha de cada vez
                            object resultObject = type.InvokeMember("Map",
                              BindingFlags.Default | BindingFlags.InvokeMethod,
                                   null,
                                   ClassObj,
                                   args);
                            //Console.WriteLine("cena feita");
                            //Console.WriteLine();
                            result = new List<KeyValuePair<string, string>>(result.Concat((IList<KeyValuePair<string, string>>)resultObject));
                        }
                    }
                }
            }

            Console.WriteLine("results");
            foreach (KeyValuePair<string, string> kvp in result)
            {
                Console.WriteLine(kvp.Key + " " + kvp.Value);
            }

            //Console.WriteLine("ja comi o split. vou gregar");

            //depois de tudo processado:
            client.SubmitResultService(result, splitNumber);

            //Console.WriteLine("greguei para cima do cliente");


            if (isJobTracker)
                AddAvailableWorker(this.id, this.serviceURL);
            else
                jobTracker.AddAvailableWorker(this.id, this.serviceURL);
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

            //Console.WriteLine("splitSize {0}", splitSize);
            //Console.WriteLine("fileSize {0}", fileSize);


            for (int i = 0; i < nSplits; i++)
            {
                // bloqueia enquanto nao houver workers disponiveis
                //Console.WriteLine("entrei no ciclo");

                while (availableWorkers.IsEmpty) { }

                KeyValuePair<int, string> entry = availableWorkers.First();

                //Console.WriteLine("vou contactar o worker");


                IWorker worker = (IWorker)Activator.GetObject(
                    typeof(IWorker),
                    entry.Value);

                worker.ProcessSplit(splitStart, splitEnd, clientURL, mapClass, dll, i + 1);

                //Console.WriteLine("worker a processar");


                splitStart += splitSize;
                if (splitEnd + splitSize > fileSize)
                    splitEnd = fileSize - 1; //ou splitEnd = fileSize;
                else
                    splitEnd += splitSize;

                //Console.WriteLine("a actualizar os splits e a recomecar");

            }
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
            Console.WriteLine("ID = {0} <-> serviceURL: {1} <-> isJobTracker: {2} <-> Status: {3}",
                this.id, this.serviceURL, this.isJobTracker, this.status);
        }

        public void GetWorkersStatus()
        {
            Console.WriteLine("ID");

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


    }
}
