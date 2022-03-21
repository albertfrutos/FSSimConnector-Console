using System;
using System.IO.Ports;
using System.IO;
using System.Timers;
using static FSSimConnector.FSSimConnectorManager;
using System.Text.RegularExpressions;

namespace FSSimConnector
{
    internal class SerialPortManager
    {
        SerialPort MyCOMPort = new SerialPort();

        static msgManager handleMessage;

        Timer timerKeepAliveTimer = null;
        Timer timerKeepAliveTimeout = null;

        bool isSerialAlive = false;

        public bool Initialize(msgManager callback, SerialPortConfiguration serialConfig)
        {
            isSerialAlive = false;

            SerialPortConfiguration serialPortConfiguration = new SerialPortConfiguration();

            MyCOMPort = serialPortConfiguration.LoadSerialConfiguration(serialConfig);

            isSerialAlive = serialPortConfiguration.IsPortReady(MyCOMPort);

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
                CloseConnection();
                isSerialAlive = false;
                return isSerialAlive;
            }

            handleMessage = callback;

            return isSerialAlive;
        }

        public void StartKeepAliveWorkflow(KeepAliveSerialConfiguration keepAliveConfig)
        {
            timerKeepAliveTimer = new System.Timers.Timer(keepAliveConfig.keepAliveMillis);
            timerKeepAliveTimer.Elapsed += new ElapsedEventHandler(SendKeepAlive);
            timerKeepAliveTimer.Start();


            timerKeepAliveTimeout = new System.Timers.Timer(keepAliveConfig.keepAliveTimeoutMillis);
            timerKeepAliveTimeout.Elapsed += new ElapsedEventHandler(KeepAliveIsTimeout);
            timerKeepAliveTimeout.Start();
        }

        internal void ProcessMessageFromSimulatorToArduino(string msgPayload)
        {
            SerialSendData(msgPayload);
        }

        public void StartSerialDataInterchange(SerialPortConfiguration config)
        {
            if (config.keepAlive.enableKeepAlive)
            {
                StartKeepAliveWorkflow(config.keepAlive);
            }

            while (isSerialAlive)
            {
                try
                {
                    if (MyCOMPort.BytesToRead > 0)
                    {
                        string message = MyCOMPort.ReadLine();
                        ProcessMessage(message);
                    }
                }
                catch (IOException ex)
                {
                    Console.Write("There was an error reading the serial port: " + ex.ToString());
                    CloseConnection();
                    return;
                }
            }
        }

        private void ProcessMessage(string command)
        {
            Message msg = new Message(command, Message.MessageOrigin.SERIAL, Message.MessageDestination.UNDEFINED);
            handleMessage(msg);
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

        public void ProcessSerialInternalMessage(string command)
        {
            Console.WriteLine("Ard -> App : " + command);

            if (command == "@999/KA=1$" && timerKeepAliveTimeout != null)
            {
                isSerialAlive = true;
                Console.WriteLine("Received KeepAlive.");
                timerKeepAliveTimeout.Stop();
                timerKeepAliveTimeout.Start();
            }
        }

        public void SendKeepAlive(object source, ElapsedEventArgs e)
        {
            string cmdKeepAlive = "@999/KA=0$";
            Console.WriteLine("App -> Ard : " + cmdKeepAlive);
            SerialSendData(cmdKeepAlive);
            Console.WriteLine("KA Sent");
        }

        public void CloseConnection()
        {
            MyCOMPort.Dispose();
            MyCOMPort = null;
        }
        public void SerialSendData(string cmdToSend)
        {
            try
            {
                MyCOMPort.WriteLine(cmdToSend);
            }
            catch (IOException ex)
            {
                Console.Write("There was a problem sending serial data: {0}", ex.ToString());
                CloseConnection();
            }
        }

        public bool IsAlive()
        {
            return ((MyCOMPort != null) && isSerialAlive);
        }
    }
}
