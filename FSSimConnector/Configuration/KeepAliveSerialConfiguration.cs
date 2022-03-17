using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSSimConnector
{
    internal class KeepAliveSerialConfiguration
    {
        public int keepAliveMillis { get; set; }
        public int keepAliveTimeoutMillis { get; set; }
        public bool  enableKeepAlive { get; set; }
    }
}
