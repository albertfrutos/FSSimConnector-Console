using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace FSSimConnector
{
    internal class SerialPortConfiguration
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public string Parity { get; set; } //not used, always 'None'
        public int DataBits { get; set; }
        public int StopBits { get; set; }  //not used, always 'One'
        public KeepAliveSerialConfiguration keepAlive { get; set; }

        public SerialPort LoadSerialConfiguration(SerialPortConfiguration config)
        {
            SerialPort MyCOMPort = new SerialPort();
            MyCOMPort.PortName = config.PortName;
            MyCOMPort.BaudRate = config.BaudRate;
            MyCOMPort.Parity = System.IO.Ports.Parity.None;
            MyCOMPort.DataBits = config.DataBits;
            MyCOMPort.StopBits = System.IO.Ports.StopBits.One;
            MyCOMPort.RtsEnable = true;
            MyCOMPort.NewLine = "\r\n";
            return MyCOMPort;
        }

        public bool IsPortReady(SerialPort myCOMPort)
        {
            bool portExists = false;
            bool portAvailable = false;

            portExists = CheckPortExists(myCOMPort.PortName);
            portAvailable = CheckPortIsAvailable(myCOMPort);

            return (portExists && portAvailable);
        }

        private bool CheckPortIsAvailable(SerialPort myCOMPort)
        {
            bool isPortAvailable = false;
            try
            {
                myCOMPort.Open();
                myCOMPort.Close();
                isPortAvailable = true;
            }
            catch
            {
                Console.WriteLine("Port is not available (maybe in use?)");
                return false;
            }
            return isPortAvailable;
        }

        private bool CheckPortExists(string portName)
        {
            bool isPortValid = true;

            string[] availablePorts = SerialPort.GetPortNames();

            if (!availablePorts.Any(x => x == portName))
            {
                Console.WriteLine("Configured port {0} not available in the system. Available ports: {1}. Will now quit serial module...", portName, String.Join(",", availablePorts));
                isPortValid = false;
            }

            return isPortValid;
        }
    }
}
