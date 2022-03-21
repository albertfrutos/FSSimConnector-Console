namespace FSSimConnector
{
    internal class KeepAliveSerialConfiguration
    {
        public int keepAliveMillis { get; set; }
        public int keepAliveTimeoutMillis { get; set; }
        public bool  enableKeepAlive { get; set; }
    }
}
