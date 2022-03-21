
namespace FSSimConnector
{
    internal class SimulatorConfiguration
    {
        public int simDataRefreshIntervalMillis { get; set; }

        public int reconnectInterval { get; set; }

        public int maxReconnectRetries { get; set; }
    }
}
