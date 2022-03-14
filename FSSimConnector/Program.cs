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
            bool exit = false;

            bool simConnStatus = false;
            bool configurationStatus = false;

            Configuration configuration = new Configuration();
            configuration = configuration.LoadConfiguration();

            serialManager updateArduinoCallback = new serialManager(updateArduino);
            simManager updateSimulatorCallback = new simManager(updateSim);

            configurationStatus = serialPort.initializeSerialPort(configuration.serialPort);
            simConnStatus = true; //simConnection.connect(configuration.simulator.reconnectInterval, configuration.simulator.maxReconnectRetries);

            
            if (configurationStatus && simConnStatus)
            {
                Console.WriteLine("Starting simulator communication thread");
                Thread dataRequest = new Thread(() => simConnection.initDataRequest(updateArduinoCallback, configuration.simulator.simDataRefreshIntervalMillis, true));
                //dataRequest.Start();

                Console.WriteLine("Starting serial port communication thread");
                serialPort.startSerialPort(updateSimulatorCallback, configuration.serialPort);
            }
            
            Console.WriteLine("");
            Console.WriteLine("Type 'exit' and press enter to quit.");

            while (!exit)
            {
                string input = Console.ReadLine();

                exit = input.Equals("exit");

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
