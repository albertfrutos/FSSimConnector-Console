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
        public bool sendAllDataAtStart { get; set; }
        public bool showVariablesOnScreen { get; set; }
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
}
