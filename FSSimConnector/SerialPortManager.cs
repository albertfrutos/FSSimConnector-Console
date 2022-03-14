using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using static FSSimConnector.Program;

namespace FSSimConnector
{
    internal class SerialPortManager
    {
        SerialPort MyCOMPort = new SerialPort();

        static simManager updateSimCallback;
        public bool ConfigureSerialPort(SerialPortConfiguration config)
        {
            bool portExists = false;
            bool portAvailable = false;
            
            MyCOMPort.PortName = config.PortName;
            MyCOMPort.BaudRate = config.BaudRate;
            MyCOMPort.Parity = Parity.None;
            MyCOMPort.DataBits = config.DataBits;
            MyCOMPort.StopBits = StopBits.One;
            MyCOMPort.RtsEnable = true;

            portExists = CheckPortExists(config.PortName);
            portAvailable = CheckPortIsAvailable(MyCOMPort);

            return portExists && portAvailable;

        }

        public bool LoadConfiguration(SerialPortConfiguration config)
        {
            bool successfulConfiguration = ConfigureSerialPort(config);

            if (!successfulConfiguration)
            {
                Console.WriteLine("Errors found duting configuration verification or load.");
            }

            return successfulConfiguration;
        }

        private bool CheckPortIsAvailable(SerialPort myCOMPort)
        {            
                return !myCOMPort.IsOpen;   
        }

        private bool CheckPortExists(string portName)
        {
            bool isPortValid = true;

            string[] availablePorts = SerialPort.GetPortNames();

            if (!availablePorts.Any(x => x == portName)){
                Console.WriteLine("Configured port {0} not available in the system. Available ports: {1}. Will now quit serial module...", portName, String.Join(",",availablePorts));
                isPortValid = false;
            }

            return isPortValid;
        }

        public void initializeSerialPort(simManager callback, SerialPortConfiguration config)
        {
            updateSimCallback = callback;

            try
            {
                MyCOMPort.Open();
                Console.WriteLine("Serial communication started successfully.");
            }
            catch (IOException ex)
            {
                Console.Write("Port is already in use {0}. Exiting serial module...", ex.ToString());
                MyCOMPort.Dispose();
            }

            

            //MyCOMPort.WriteLine("@4/HEADING_BUG_SET=32$");

            int num;


            while (true)
            {



                try
                {
                    if (MyCOMPort.BytesToRead > 0)
                    {
                        
                        //string command = MyCOMPort.ReadLine();
                        MyCOMPort.ReadTo("@");
                        string command = MyCOMPort.ReadTo("$");
                        //Console.WriteLine("Command is: " + command);

                       // num = Int32.Parse(command.Split('=')[1]) + 1;
                        //Console.WriteLine("--> @4/HEADING_BUG_SET=" + num.ToString() + "$");
                        //SerialSendData("@4/HEADING_BUG_SET=" + num.ToString() + "$");
                        //updateSimCallback(command);
                        //SimConnectManager.ProcessCommandFromArduino(command);

                        updateSimCallback(command);

                    }
                }
                catch (IOException ex)
                {
                    Console.Write(ex.ToString());
                    MyCOMPort.Dispose();
                }

            }
            


        }




        public void SerialSendData(string cmdToSend)
        {
            try
            {
                MyCOMPort.WriteLine(cmdToSend);
            }
            catch (IOException ex)
            {
                Console.Write(ex.ToString());
                MyCOMPort.Dispose();
            }
        }
    }
}
