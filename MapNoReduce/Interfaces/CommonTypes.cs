using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapNoReduce
{
    public interface IClient
    {
        void Init(string entryURL);
        void Submit(string filePath, int nSplits, string outputPath, string mapClass, string dllPath);
        string GetSplitService(long splitBegin, long splitEnd);
        void SubmitResultService(IList<KeyValuePair<string, string>> mapResults, int splitNumber);
    }

    public interface IPuppetMaster
    {
        
    }

    public interface IWorker
    {
        //worker methods
        void GetJob();
        void ProcessSplit(long splitStart, long splitEnd, string clientURL);
        void SubmitResult();


        //job tracker methods 
        void SubmitJobToWorker(long fileSize, int nSplits, string clientURL);
        void AddWorker(int id, string serviceURL);//apenas usado pelo job tracker
        void SetWorkersMap(IDictionary<int, string> oldWorkersMap);
        void AddAvailableWorker(int id);
        void RemoveAvailableWorker(int id);
    }


    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}
