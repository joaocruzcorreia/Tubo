using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace MapNoReduce
{
    
    public class Client : MarshalByRefObject, IClient 
    {
        private string entryURL;
        private static int port;
        private int nSplits;
        private string filePath;
        private string outputPath;
        private List<string> splitList;

        private IList<KeyValuePair<string, string>>[] mapResults;

        public Client()
        {
            port = 10001;
        }

        public void Init(string entryURL){
            this.entryURL = entryURL;
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IClient),
                "C",
                WellKnownObjectMode.Singleton);
        }

        public void Submit(string filePath, int nSplits, string outputPath, IMapper map, string dllPath)
        {
            //tamanho do ficheiro
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            IWorker jobTracker = (IWorker) Activator.GetObject(
                typeof(IWorker),
                entryURL);

            jobTracker.SubmitJob(fileSize, nSplits, port);
            
        }


        //devolve split entre as posicoes splitBegin e splitEnd (inclusive)
        public string GetSplitService(int splitBegin, int splitEnd)
        {
            int splitSize = splitEnd - splitBegin + 1;
            byte[] s = new byte[splitSize];
            FileStream f = File.OpenRead(filePath);
            f.Seek(splitBegin, SeekOrigin.Begin);
            f.Read(s, 0, splitSize);
            f.Close();
            string split = System.Text.Encoding.UTF8.GetString(s);
            return split;
        }


        public void SubmitResultService(IList<KeyValuePair<string, string>> mapResults, int splitNumber)
        {
            string path = outputPath + Convert.ToString(splitNumber) + ".out";
            StreamWriter sw = new StreamWriter(path);

            foreach (KeyValuePair<string, string> kvp in mapResults)
            {
                sw.WriteLine(kvp.Key + " " + kvp.Value);
            }
            sw.Close();
        }

        /*
         * returns a list with the splits
         *
        // ir para o worker
        private string GetSplit(string filePath, int nSplits)
        {
            string split;

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;
                long splitSize;

                if(fileSize % nSplits > 0) // resto
                    splitSize = fileInfo.Length/(nSplits-1); //se resto >0 e necessario mais um split para o resto
                else
                    splitSize = fileInfo.Length/nSplits;

                StreamReader sr = new StreamReader(filePath);
                char[] s = new char[splitSize];
                string split;

                while (sr.Peek() >= 0)
                {
                    sr.Read(s, 0, s.Length);
                    split = new string(s);
                    splitList.Add(split);
                }

            }
            catch (FileNotFoundException e)
            {
                Console.Write(e.StackTrace);
            }

            return splitList;
        }*/
    }
}
