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
            simConnStatus = simConnection.connect(configuration.simulator.reconnectInterval, configuration.simulator.maxReconnectRetries);

            Thread simulatorDataManager = new Thread(() => simConnection.initDataRequest(updateArduinoCallback, configuration.simulator.simDataRefreshIntervalMillis, true));
            Thread serialDataManager = new Thread(() => serialPort.startSerialPort(updateSimulatorCallback, configuration.serialPort));

            if (configurationStatus && simConnStatus)
            {
                Console.WriteLine("Starting simulator communication thread");
                simulatorDataManager.Start();

                Console.WriteLine("Starting serial port communication thread");
                serialDataManager.Start();
            }

            Console.WriteLine("Type 'exit' and press enter to quit.");

            while (!exit)
            {
                string input = Console.ReadLine();
                exit = input.Equals("exit");
            }

            simulatorDataManager.Abort();
            serialDataManager.Abort();
        }

        public static void updateSim(string command)
        {
            Console.WriteLine("Ard -> Sim : " + command);
            simConnection.ProcessCommandFromArduino(command);
        }

        public static void updateArduino(string command)
        {
            Console.WriteLine("Sim -> Ard : " + command);
            serialPort.SerialSendData(command);
        }

    }
}
