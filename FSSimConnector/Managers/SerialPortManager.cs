using System;
using System.IO.Ports;
using System.IO;
using System.Timers;
using static FSSimConnector.FSSimConnectorManager;

namespace FSSimConnector
{
    internal class SerialPortManager
    {
        SerialPort MyCOMPort = new SerialPort();

        static simManager updateSimCallback;

        System.Timers.Timer timerKeepAliveTimer = null;
        System.Timers.Timer timerKeepAliveTimeout = null;

        bool isSerialAlive = false;

        public bool initialize(SerialPortConfiguration serialConfig)
        {
            isSerialAlive = false;

            SerialPortConfiguration serialPortConfiguration = new SerialPortConfiguration();

            MyCOMPort = serialPortConfiguration.LoadSerialConfiguration(serialConfig);

            isSerialAlive = serialPortConfiguration.isPortReady(MyCOMPort);

            if((serialConfig.keepAlive.keepAliveMillis >= serialConfig.keepAlive.keepAliveTimeoutMillis) && serialConfig.keepAlive.enableKeepAlive)
            {
                Console.WriteLine("KeepAlive interval is higher that the KeepAlive timeout interval.");
                isSerialAlive = false;
                return isSerialAlive;
            }

            if (!isSerialAlive)
            {
                Console.WriteLine("Errors found during configuration load or serial port inicialization.");
                return isSerialAlive;
            }

            try
            {
                MyCOMPort.Open();
                Console.WriteLine("Serial communication started successfully.");
            }
            catch (IOException ex)
            {
                Console.Write("Port is already in use {0}. Exiting serial module...", ex.ToString());
                closeConnection();
                isSerialAlive = false;
                return isSerialAlive;
            }

            return isSerialAlive;
        }

        public void startKeepAliveWorkflow(KeepAliveSerialConfiguration keepAliveConfig)
        {
            timerKeepAliveTimer = new System.Timers.Timer(keepAliveConfig.keepAliveMillis);
            timerKeepAliveTimer.Elapsed += new ElapsedEventHandler(SendKeepAlive);
            timerKeepAliveTimer.Start();


            timerKeepAliveTimeout = new System.Timers.Timer(keepAliveConfig.keepAliveTimeoutMillis);
            timerKeepAliveTimeout.Elapsed += new ElapsedEventHandler(KeepAliveIsTimeout);
            timerKeepAliveTimeout.Start();
        }

        public void StartSerialDataInterchange(simManager callback, SerialPortConfiguration config)
        {
            updateSimCallback = callback;

            if (config.keepAlive.enableKeepAlive)
            {
                startKeepAliveWorkflow(config.keepAlive);
            }

            while (isSerialAlive)
            {
                try
                {
                    if (MyCOMPort.BytesToRead > 0)
                    {
                        string command = MyCOMPort.ReadLine();
                        if (command.StartsWith("@999"))
                        {
                            handleInternalMessage(command, config.keepAlive.keepAliveTimeoutMillis);
                        }
                        else
                        {
                            updateSimCallback(command);
                        }
                    }
                }
                catch (IOException ex)
                {
                    Console.Write("There was an error reading the serial port: " + ex.ToString());
                    closeConnection();
                    return;
                }
            }
        }



        private void KeepAliveIsTimeout(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Did not receive timeout response. Stopping and exiting.");
            isSerialAlive = false;
            MyCOMPort.Close();
            MyCOMPort = null;
            timerKeepAliveTimer.Stop();
            timerKeepAliveTimeout.Stop();
        }

        private void handleInternalMessage(string command, int keepAliveTimeoutMillis)
        {
            Console.WriteLine("Ard -> App : " + command);

            if (command == "@999/KA=1$")
            {
                isSerialAlive = true;
                Console.WriteLine("Received KeepAlive.");
                timerKeepAliveTimeout.Stop();
                timerKeepAliveTimeout.Start();
            }
        }

        public void SendKeepAlive(object source, ElapsedEventArgs e)
        {
            SerialSendData("@999/KA=0$");
            Console.WriteLine("KA Sent");
        }

        public void closeConnection()
        {
            MyCOMPort.Dispose();
            MyCOMPort = null;
        }
        public void SerialSendData(string cmdToSend)
        {
            try
            {
                Console.WriteLine("App -> Ard : " + cmdToSend);
                MyCOMPort.WriteLine(cmdToSend);
            }
            catch (IOException ex)
            {
                Console.Write("There was a problem sending serial data: {0}", ex.ToString());
                closeConnection();
            }
        }

        public bool isAlive()
        {
            return ((MyCOMPort != null) && isSerialAlive);
        }
    }
}
