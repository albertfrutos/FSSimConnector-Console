using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSSimConnector
{
    internal class FSSimConnectorManager
    {
        public delegate void serialManager(string command);
        public delegate void simManager(string command);

        public static SimConnectManager simConnection = new SimConnectManager();
        public static SerialPortManager serialPort = new SerialPortManager();

        public List<Thread> Start()
        {
            bool simConnStatus = false;
            bool serialPortStatus = false;
            bool exit = false;

            List<Thread> threads = new List<Thread>();

            Configuration configuration = new Configuration();
            configuration = configuration.LoadConfiguration();

            serialManager updateArduinoCallback = new serialManager(SendToArduino);
            simManager updateSimulatorCallback = new simManager(SendToSimulator);

            serialPortStatus = serialPort.initialize(configuration.serialPort);
            simConnStatus = true simConnection.connect(configuration.simulator.reconnectInterval, configuration.simulator.maxReconnectRetries);

            Thread simulatorDataManager = new Thread(() => simConnection.StartSimDataInterchange(updateArduinoCallback, configuration.simulator.simDataRefreshIntervalMillis, configuration.sendAllDataAtStart, configuration.showVariablesOnScreen));
            Thread serialDataManager = new Thread(() => serialPort.StartSerialDataInterchange(updateSimulatorCallback, configuration.serialPort));

            if (serialPortStatus && simConnStatus)
            {
                Console.WriteLine("Starting simulator communication thread");
                simulatorDataManager.Name = "SimulatorThread";
                simulatorDataManager.Start();
                threads.Add(simulatorDataManager);

                Console.WriteLine("Starting serial port communication thread");
                serialDataManager.Name = "SerialPortThread";
                serialDataManager.Start();
                threads.Add((Thread)serialDataManager);
            }

            Console.WriteLine("Type 'exit' and press enter to quit.");

            while (!exit)
            {
                string input = Console.ReadLine();
                exit = input.Equals("exit");
            }

            foreach (Thread thread in threads)
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

            return threads;
        }

        public static void SendToSimulator(string command)
        {
            Console.WriteLine("Ard -> Sim : " + command);
            simConnection.ProcessCommandFromArduino(command);
        }

        public static void SendToArduino(string command)
        {
            Console.WriteLine("Sim -> Ard : " + command);
            serialPort.SerialSendData(command);
        }
    }
}
