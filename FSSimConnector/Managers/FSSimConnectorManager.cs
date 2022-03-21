using System;
using System.Collections.Generic;
using System.Threading;

namespace FSSimConnector
{
    internal class FSSimConnectorManager
    {
        public delegate void msgManager(Message message);

        public static SimConnectManager simConnection = new SimConnectManager();
        public static SerialPortManager serialPort = new SerialPortManager();

        public MessageHandler msgHandler = new MessageHandler(simConnection, serialPort);

        public List<Thread> Start()
        {
            bool simConnStatus = false;
            bool serialPortStatus = false;
            bool exit = false;

            List<Thread> threads = new List<Thread>();

            Configuration configuration = new Configuration();
            configuration = configuration.LoadConfiguration();

            msgManager msgHandlerManager = new msgManager(msgHandler.HandleMessage);

            serialPortStatus = serialPort.Initialize(msgHandlerManager, configuration.serialPort);
            simConnStatus = simConnection.Connect(msgHandlerManager, configuration.simulator.reconnectInterval, configuration.simulator.maxReconnectRetries);

            Thread simulatorDataManager = new Thread(() => simConnection.StartSimDataInterchange(configuration.simulator.simDataRefreshIntervalMillis, configuration.sendAllDataAtStart, configuration.showVariablesOnScreen));
            Thread serialDataManager = new Thread(() => serialPort.StartSerialDataInterchange(configuration.serialPort));

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
    }
}
