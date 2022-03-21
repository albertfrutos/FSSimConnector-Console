using System;
using System.Text.RegularExpressions;

namespace FSSimConnector
{
    internal class MessageHandler
    {
        private SimConnectManager simConnectManager;
        private SerialPortManager serialPortManager;

        public MessageHandler(SimConnectManager simConnectMgr, SerialPortManager serialPortMgr)
        {
            simConnectManager = simConnectMgr;
            serialPortManager = serialPortMgr;
        }

        public void HandleMessage(Message message)
        {
            string internalSerialCommandPatternID = @"^@(9)[0-9]{2,}\/"; // @9XX/
            string internalSimulatorCommandPatternID = @"^@(8)[0-9]{2,}\/"; // @8XX/ 
            string simulatorDataPatternID = @"^@[0-7]?[0-9]?[0-9]?\/"; // @0/ - @799/

            if (Regex.Match(message.msgPayload, internalSerialCommandPatternID).Success)
            {
                message.msgDestination = Message.MessageDestination.SERIAL_INTERNAL;
            }
            else if (Regex.Match(message.msgPayload, internalSimulatorCommandPatternID).Success)
            {
                message.msgDestination = Message.MessageDestination.SIMULATOR_INTERNAL;
            }
            else if (Regex.Match(message.msgPayload, simulatorDataPatternID).Success)
            {
                if (message.msgOrigin == Message.MessageOrigin.SERIAL)
                {
                    message.msgDestination = Message.MessageDestination.SIMULATOR_DATA;
                }
                else if (message.msgOrigin == Message.MessageOrigin.SIMULATOR)
                {
                    message.msgDestination = Message.MessageDestination.SERIAL_DATA;
                }
                else
                {
                    message.msgDestination = Message.MessageDestination.UNDEFINED;
                }
            }
            else
            {
                message.msgDestination = Message.MessageDestination.UNDEFINED;
            }

            MessageRouter(message);
        }

        internal void MessageRouter(Message message)
        {
            switch (message.msgDestination)
            {
                case Message.MessageDestination.SERIAL_INTERNAL:
                    serialPortManager.ProcessSerialInternalMessage(message.msgPayload);
                    break;
                case Message.MessageDestination.SIMULATOR_INTERNAL:
                    simConnectManager.ProcessInternalSimulatorMessage(message.msgPayload);
                    break;
                case Message.MessageDestination.SERIAL_DATA:
                    serialPortManager.ProcessMessageFromSimulatorToArduino(message.msgPayload);
                    break;
                case Message.MessageDestination.SIMULATOR_DATA:
                    simConnectManager.ProcessMessageFromArduinoToSimulator(message.msgPayload);
                    break;
                case Message.MessageDestination.UNDEFINED:
                    ProcessUndefinedMessageDestination(message.msgPayload);
                    break;
                default:
                    Console.WriteLine("No route for message {0} available", message.msgPayload);
                    break;
            }
        }

        private void ProcessUndefinedMessageDestination(string msgPayload)
        {
            Console.WriteLine("Undefined destination messages are not processed. Message {0} ignored.", msgPayload);
        }
    }

}