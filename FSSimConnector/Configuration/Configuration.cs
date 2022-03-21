using Newtonsoft.Json;
using System.IO;

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
            Configuration config = DeserializeConfiguration(json);
            return config;
        }
        
        private Configuration DeserializeConfiguration(string json)
        {
            Configuration config = JsonConvert.DeserializeObject<Configuration>(json);
            return config;
        }
    }
}
