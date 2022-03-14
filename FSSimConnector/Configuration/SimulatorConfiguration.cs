using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSSimConnector
{
    internal class SimulatorConfiguration
    {
        public int simDataRefreshIntervalMillis { get; set; }

        public int reconnectInterval { get; set; }

        public int maxReconnectRetries { get; set; }
    }
}
