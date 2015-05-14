using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using PADIMapNoReduce;

namespace PADIMapNoReduce
{
    class User
    {
        private static string entryURL;
        private static string filePath;
        private static string outputPath;
        private static int nSplits;
        private static string mapClass;
        private static string dllPath;
        private static bool isInit;

        // recebe entryURL, file path, output path, number of splits, map class name, dll path 
        static void Main(string[] args)
        {
            Client client = new Client();

            if (args.Length == 6)
            {
                entryURL = args[0];
                filePath = args[1];
                outputPath = args[2];
                nSplits = Convert.ToInt32(args[3]);
                mapClass = args[4];
                dllPath = args[5];
                UserCmd(client);

            }
            else
                UserCmd(client);

        }

        public static void UserCmd(Client client)
        {
            isInit = false;
            string command;

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

                    case ("default"):
                        client.Init("tcp://localhost:30001/W");
                        client.Submit(@"C:\padi\pl.txt", 2, @"C:\padi", "CharCountMapper", @"C:\padi\LibMapperCharCount.dll");
                        break;
                    case ("default1"):
                        client.Init("tcp://localhost:30001/W");
                        client.Submit(@"C:\padi\pl.txt", 2, @"C:\padi", "ParadiseCountMapper", @"C:\padi\LibMapperParadiseCount.dll");
                        break;

                    default:
                        Console.WriteLine("Not a valid command.");
                        break;
                }
            }

        }

    }
}
