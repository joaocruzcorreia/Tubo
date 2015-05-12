﻿using System;
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
        private static int port = 10001;
        private int nSplits;
        private string filePath;
        private string outputPath;
        private List<string> splitList;

        private IList<KeyValuePair<string, string>>[] mapResults;

        public void Init(string entryURL)
        {
            this.jobTrackerURL = entryURL;

        }

        public void Submit(string filePath, int nSplits, string outputPath, string mapClass, string dllPath)
        {
            //tamanho do ficheiro
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            byte[] dll = File.ReadAllBytes(dllPath);

            IWorker jobTracker = (IWorker)Activator.GetObject(
                typeof(IWorker),
                jobTrackerURL);

            jobTracker.SubmitJobToWorker(fileSize, nSplits, this.jobTrackerURL, mapClass, dll);

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
