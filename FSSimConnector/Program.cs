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

            FSSimConnectorManager fsSimConnectorManager = new FSSimConnectorManager();

            fsSimConnectorManager.Start();
            Console.WriteLine("Type 'exit' and press enter to quit.");

            
            
        }



    }
}
