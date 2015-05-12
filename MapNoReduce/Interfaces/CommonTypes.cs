﻿using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MapNoReduce
{
    public interface IClient
    {
        void Init(string entryURL);
        void Submit(string filePath, int nSplits, string outputPath, string mapClass, string dllPath);
        string GetSplitService(long splitBegin, long splitEnd);
        void SubmitResultService(IList<KeyValuePair<string, string>>[] mapResults, int splitNumber);
    }

    public interface IPuppetMaster
    {
        void cmdReader(string allInput);
        
    }

    public interface IWorker
    {
        //worker methods
        void ProcessSplit(long splitStart, long splitEnd, string clientURL, string mapClass, byte[] dll, int splitNumber);

        //job tracker methods 
        void SubmitJobToWorker(long fileSize, int nSplits, string clientURL, string mapClass, byte[] dll);
        void AddWorker(int id, string serviceURL);
        void SetWorkersMap(ConcurrentDictionary<int, string> oldWorkersMap);
        void AddAvailableWorker(int id, string serviceURL);
        void RemoveAvailableWorker(int id, string serviceURL);
        string GetWorkerURL(int id);

        //puppet master commands
        void GetStatus(); //STATUS
        void GetWorkersStatus(); //WORKERS STATUS
        void Slow(int sec); //SLOWW
        void FreezeWorker(); //FREEZEW
        void UnfreezeWorker(); //UNFREEZEW
        void FreezeCommunication(); //FREEZEC
        void UnfreezeCommunication(); //UNFREEZEC
    }


    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}
