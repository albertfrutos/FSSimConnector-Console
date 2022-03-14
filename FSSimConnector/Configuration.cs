using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSSimConnector
{
    internal class Configuration
    {
        public SimulatorConfiguration simulator { get; set; }
        public SerialPortConfiguration serialPort { get; set; }

        public Configuration LoadConfiguration()
        {
            var json = File.ReadAllText(@"Files\config.json");
            Configuration config = deserializeConfiguration(json);
            return config;
        }
        
        private Configuration deserializeConfiguration(string json)
        {
            Configuration config = JsonConvert.DeserializeObject<Configuration>(json);
            return config;
        }
    }

    internal class SimulatorConfiguration
    {
        public int simDataRefreshIntervalMillis { get; set; }

        public int reconnectInterval { get; set; }

        public int maxReconnectRetries { get; set; }
    }

    internal class SerialPortConfiguration
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public string Parity { get; set; } //not used, always 'None'
        public int DataBits { get; set; }
        public int StopBits { get; set; }  //not used, always 'One'
    }
}
