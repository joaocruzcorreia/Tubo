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
    
    public class Client 
    {
        private string jobTrackerURL;
        private static int port = 10001;

        private ClientServices clientServices = new ClientServices();
        private bool isChCreated = false;


        public void Init(string entryURL){
            
            this.jobTrackerURL = entryURL;
            clientServices.JobTrackerURL = entryURL;
            
            if (!isChCreated)
            {
                TcpChannel channel = new TcpChannel(port);
                ChannelServices.RegisterChannel(channel, false);
                RemotingServices.Marshal(clientServices, "C", typeof(IClient));
                isChCreated = true;
            }
        }

        public void Submit(string filePath, int nSplits, string outputPath, string mapClass, string dllPath)
        {
            clientServices.FilePath = filePath;
            clientServices.NSplits = nSplits;
            clientServices.OutputPath = outputPath;

            //tamanho do ficheiro
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            byte[] dll = File.ReadAllBytes(dllPath);

            IWorker jobTracker = (IWorker) Activator.GetObject(
                typeof(IWorker),
                jobTrackerURL);

            jobTracker.SubmitJobToWorker(fileSize, nSplits, this.jobTrackerURL, mapClass, dll);
            
        }

    }
}
