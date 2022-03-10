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
        System.IO.Ports.SerialPort MyCOMPort = new System.IO.Ports.SerialPort();

        static simManager updateSimCallback;
        public void ConfigureSerialPort(SerialPortConfiguration config)
        {


            MyCOMPort.PortName = config.PortName;
            MyCOMPort.BaudRate = config.BaudRate;
            MyCOMPort.Parity = Parity.None;
            MyCOMPort.DataBits = config.DataBits;
            MyCOMPort.StopBits = StopBits.One;
            MyCOMPort.RtsEnable = true;
        }

        public void initializeSerialPort(simManager callback, SerialPortConfiguration config, Microsoft.FlightSimulator.SimConnect.SimConnect simconnect)
        {
            ConfigureSerialPort(config);
            updateSimCallback = callback;

            try
            {
                MyCOMPort.Open();
            }
            catch (IOException ex)
            {
                Console.Write(ex.ToString());
                MyCOMPort.Dispose();
            }

            Console.WriteLine("start");

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

                        num = Int32.Parse(command.Split('=')[1]) + 1;
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
