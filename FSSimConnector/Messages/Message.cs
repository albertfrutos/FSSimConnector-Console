

namespace FSSimConnector
{
    internal class Message
    {
        public string msgPayload { get; set; }
        public MessageOrigin msgOrigin { get; set; }
        public MessageDestination msgDestination { get; set; }

        public Message(string payload, MessageOrigin origin, MessageDestination destination)
        {
            msgPayload = payload;
            msgOrigin = origin;
            msgDestination = destination;
        }

        internal enum MessageOrigin
        {
            SIMULATOR,
            SERIAL,
            UNDEFINED
        }

        internal enum MessageDestination
        {
            SIMULATOR_DATA,
            SIMULATOR_INTERNAL,
            SERIAL_INTERNAL,
            SERIAL_DATA,
            UNDEFINED
        }
    }
}
