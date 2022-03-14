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

        public bool initializeSerialPort(SerialPortConfiguration serialConfig)
        {
            

            bool portReady = false;

            SerialPortConfiguration serialPortConfiguration = new SerialPortConfiguration();

            MyCOMPort = serialPortConfiguration.LoadSerialConfiguration(serialConfig);

            portReady = serialPortConfiguration.isPortReady(MyCOMPort);
            
            

            if (!portReady)
            {
                Console.WriteLine("Errors found during configuration verification or load or serial port inicialization.");
            }
            return portReady;
        }

        public void startSerialPort(simManager callback, SerialPortConfiguration config)
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
