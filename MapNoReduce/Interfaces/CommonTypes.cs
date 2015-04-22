﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapNoReduce
{
    public interface IClient
    {
        public void Init(string entryURL);
        public void Submit(string filePath, int nSplits, string outputPath, string mapClass, string dllPath);
        public string GetSplitService(int splitBegin, int splitEnd);
        public void SubmitResultService(IList<KeyValuePair<string, string>> mapResults, int splitNumber);
    }

    public interface IPuppetMaster
    {
        
    }

    public interface IWorker
    {
        //job Tracker
        public void SubmitJob(long fileSize, int nSplits /*byte[] dll*/);
        public void GetJob();

        public void SubmitResult();
    }


    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}