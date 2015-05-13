using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace MapNoReduce
{

    class ClientServices : MarshalByRefObject, IClient
    {
        private string jobTrackerURL;
        private int nSplits;   
        private string filePath;        
        private string outputPath;

        public override object InitializeLifetimeService()
        {

            return null;

        }


        public int NSplits
        {
            get { return nSplits; }
            set { nSplits = value; }
        }

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }
        
        public string OutputPath
        {
            get { return outputPath; }
            set { outputPath = value; }
        }
        public string JobTrackerURL
        {
            get { return jobTrackerURL; }
            set { jobTrackerURL = value; }
        }


        //devolve split entre as posicoes splitBegin e splitEnd (inclusive)
        public string GetSplitService(long splitBegin, long splitEnd)
        {
            int splitSize = (int)(splitEnd - splitBegin + 1);
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
    }
}
