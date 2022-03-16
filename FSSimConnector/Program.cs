using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Messaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;
using System.Collections;

namespace FSSimConnector
{

    internal class Program
    {



        static void Main(string[] args)
        {
            bool exit = false;

            FSSimConnectorManager fsSimConnectorManager = new FSSimConnectorManager();

            List<Thread> appThreads = fsSimConnectorManager.Start();

            Console.WriteLine("Type 'exit' and press enter to quit.");

            while (!exit)
            {
                string input = Console.ReadLine();
                exit = input.Equals("exit");
            }

            foreach (Thread thread in appThreads)
            {
                Console.WriteLine("Attempting to abort thread {0}", thread.Name);
                try
                {
                    thread.Abort();
                    thread.Join();
                    Console.WriteLine("Thread {0} aborted.", thread.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to abort thread {0}. Error is {1} ", thread.Name, ex.Message);
                }
            }
            
        }



    }
}
