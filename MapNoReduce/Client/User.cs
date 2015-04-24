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
        string entryURL = "tcp://localhost:30001/W";
        string filePath = @"c:\padi\file.txt";
        int nSplits = 10;
        string outputPath = @"c:\padi\";
        string dllPath;
        IMapper map;
        Client client;

        

        static void Main(string[] args)
        {
            string command;
            string entryURL = "";
            string filePath;
            int nSplits;
            string outputPath;
            string mapClass;
            string dllPath;
            IClient client = new Client();
            
            
            while (true) { 
                Console.WriteLine("Enter a command (init or submit):");
                command = Console.ReadLine();

                switch (command)
                {
                    case ("init"):
                        Console.WriteLine("Entry URL:");
                        entryURL = Console.ReadLine();

                        client.Init(entryURL);

                        break;

                    case ("submit"):
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

                        break;

                    default:
                        Console.WriteLine("Not a valid command.");
                        break;
                }    
            }
        }

    }
}
