using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MapNoReduce
{
    class User
    {

        // recebe entryURL, file path, output path, number of splits, map class name, dll path 
        static void Main(string[] args)
        {
            Client client = new Client();

            if (args.Length == 6)
            {
                string entryURL = args[0];
                string filePath = args[1];
                string outputPath = args[2];
                int nSplits = Convert.ToInt32(args[3]);
                string mapClass = args[4];
                string dllPath = args[5];

                client.Init(entryURL);
                client.Submit(filePath, nSplits, outputPath, mapClass, dllPath);
            }
            else
                UserCmd(client);  
            
        }

        public static void UserCmd (Client client)
        {
            bool isInit = false;
            string command;
            string entryURL;
            string filePath;
            int nSplits;
            string outputPath;
            string mapClass;
            string dllPath;

            while (true)
            {
                Console.WriteLine("Enter a command (INIT or SUBMIT):");
                command = Console.ReadLine();

                switch (command)
                {
                    case ("INIT"):
                        isInit = true;
                        Console.WriteLine("Entry URL:");
                        entryURL = Console.ReadLine();

                        client.Init(entryURL);

                        break;

                    case ("SUBMIT"):
                        if (isInit)
                        {
                            Console.WriteLine("File Path:");
                            filePath = Console.ReadLine();

                            Console.WriteLine("Number of Splits:");
                            nSplits = Convert.ToInt32(Console.ReadLine());

                            Console.WriteLine("Output Path:");
                            outputPath = Console.ReadLine();

                            Console.WriteLine("Map class:");
                            mapClass = Console.ReadLine();

                            Console.WriteLine("DLL Path:");
                            dllPath = Console.ReadLine();

                            client.Submit(filePath, nSplits, outputPath, mapClass, dllPath);

                            Console.WriteLine("Job Completed.");
                        }
                        else
                            Console.WriteLine("Must execute INIT command first.");

                        break;

                    default:
                        Console.WriteLine("Not a valid command.");
                        break;
                }
            }

        }

    }
}
