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

        public delegate void serialManager(string command);
        public delegate void simManager(string command);
        public static SimConnectManager simConnection = new SimConnectManager();
        public static SerialPortManager serialPort = new SerialPortManager();

        static void Main(string[] args)
        {

            Configuration configuration = new Configuration();
            configuration = configuration.LoadConfiguration();

            serialManager updateArduinoCallback = new serialManager(updateArduino);
            simManager updateSimCallback = new simManager(updateSim);

            SimConnect simconnect = simConnection.connect(configuration.simulator.reconnectInterval);
            Thread dataRequest = new Thread(() => simConnection.initDataRequest(updateArduinoCallback, configuration.simulator.refreshIntervalMillis,true));
            dataRequest.Start();

            serialPort.initializeSerialPort(updateSimCallback, configuration.serialPort, simconnect);

            simConnection.requestSendAllData();

            while (true)
            {

            }
        }

        public static void updateSim(string command)
        {
            simConnection.ProcessCommandFromArduino(command);
            Console.WriteLine("Ard -> Sim : " + command);
        }

        public static void updateArduino(string command)
        {
            serialPort.SerialSendData(command);
            Console.WriteLine("Sim -> Ard : " + command);
        }

    }
}
