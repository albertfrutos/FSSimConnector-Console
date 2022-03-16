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
            bool configurationStatus = false;

            List<Thread> threads = new List<Thread>();

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
                simulatorDataManager.Name = "SimulatorThread";
                simulatorDataManager.Start();
                threads.Add(simulatorDataManager);

                Console.WriteLine("Starting serial port communication thread");
                serialDataManager.Name = "SerialPortThread";
                serialDataManager.Start();
                threads.Add((Thread)serialDataManager);
            }

            return threads;
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
