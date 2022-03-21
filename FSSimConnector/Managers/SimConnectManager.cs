using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static FSSimConnector.FSSimConnectorManager;
using static FSSimConnector.Program;

namespace FSSimConnector
{
    internal class SimConnectManager : SimulatorHelpers
    {
        private static bool sendAllData = false;
        private static bool showVariablesOnScreen = false;

        private static SimConnect my_simconnect = null;

        private static Timer timer = null;

        static msgManager handleMessage;

        private static Struct1 previousData = new Struct1();

        public void requestSendAllData()
        {
            Console.WriteLine("Next data report from simulator will contain all requested data.");
            sendAllData = true;
        }

        private static void simconnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (data.dwRequestID == 0)
            {
                Struct1 currentData = (Struct1)data.dwData[0];

                FieldInfo[] fi = typeof(Struct1).GetFields(BindingFlags.Public | BindingFlags.Instance);

                Console.Clear();

                foreach (FieldInfo info in fi)
                {
                    string parameterName = info.Name;
                    string parameterCurrentValue = info.GetValue(currentData).ToString();
                    string parameterPreviousValue = info.GetValue(previousData).ToString();

                    if (showVariablesOnScreen)
                    {
                        string flagIsDifferent = parameterCurrentValue != parameterPreviousValue ? "*" : " ";
                        Console.WriteLine(flagIsDifferent + " " + info.Name + " " + parameterCurrentValue);
                    }

                    ProcessVarsAndSendToArduino(parameterName, parameterCurrentValue, parameterPreviousValue);
                }

                sendAllData = false;

                previousData = currentData;
            }
        }

        private static void sendCommand(string command)
        {
            Message msg = new Message(command, Message.MessageOrigin.SIMULATOR,Message.MessageDestination.UNDEFINED);
            handleMessage(msg);
        }

        private static void ProcessVarsAndSendToArduino(string name, string parameterCurrentValue, string parameterPreviousValue)
        {

            if ((!parameterCurrentValue.Equals(parameterPreviousValue)) || sendAllData)
            {
                string variableName = name;
                //Console.WriteLine(variableName);
                string variableValue = MapValues(parameterCurrentValue);
                structToID.TryGetValue(variableName, out int variableID);
                sendCommand("@" + variableID + "/" + variableName + "=" + variableValue + "$");
            }
        }

        internal void ProcessInternalSimulatorMessage(string msgPayload)
        {
            //Console.WriteLine("Not implemented function. Message {0} ignored.", msgPayload);

            Console.WriteLine("App -> Sim: " + msgPayload);

            if (msgPayload == "@899/SA=1$")
            {
                Console.WriteLine("Received Request to send all in next update.");
                requestSendAllData();
            }
        }

        private static string MapValues(string value)
        {
            string mappedValue = value;

            if (value == "True")
            {
                mappedValue = "1";
            }
            else if (value == "False")
            {
                mappedValue = "0";
            }

            return mappedValue;
        }

        public void ProcessMessageFromArduinoToSimulator(string command)
        {
            string pattern = @"^@(.*)\/(.*)=(.*)\$$";
            Match match = Regex.Match(command, pattern);
            if (match.Success)
            {
                string eventName = match.Groups[2].Value;
                uint value = uint.Parse(match.Groups[3].Value);
                sendEvent(eventName, value);
                RequestSimulatorData();
            }

        }

        public void StartSimDataInterchange(int refreshIntervalMillis, bool sendAllDataAtStart = true, bool showVariables = false)
        {
            showVariablesOnScreen = showVariables;

            if (sendAllDataAtStart)
            {
                requestSendAllData();
            }
            try
            {
                my_simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                my_simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);
                my_simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);

                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT MASTER", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT FLIGHT DIRECTOR ACTIVE", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT HEADING LOCK", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT ALTITUDE LOCK", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT ALTITUDE LOCK VAR", "feet", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT VERTICAL HOLD", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT VERTICAL HOLD VAR", "feet/minute", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT FLIGHT LEVEL CHANGE", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT AIRSPEED HOLD VAR", "knots", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT THROTTLE ARM", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT BACKCOURSE HOLD", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT APPROACH HOLD", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                my_simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT YAW DAMPER", null, SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                my_simconnect.RegisterDataDefineStruct<Struct1>(DEFINITIONS.Struct1);
                my_simconnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(simconnect_OnRecvSimobjectDataBytype);

                timer = new Timer(TimerCallback, null, 0, refreshIntervalMillis);

                //updateArduinoCallback = callback;

                RequestSimulatorData();



            }
            catch (COMException exception1)
            {
                Console.WriteLine("Exception while initializing data request: {0}", exception1.Message);
                closeConnection();
            }
        }

        public bool connect(msgManager callback, int reconnectInterval, int maxReconnectRetries = 5)
        {
            int retryNumber = 0;

            while (my_simconnect == null)
            {
                retryNumber++;

                if (retryNumber > maxReconnectRetries)
                {
                    Console.WriteLine("Maximum number of {0} attempts reached. Exiting simulator connector module...", maxReconnectRetries);
                    return false;
                }

                Console.WriteLine("Connecting to simulator. (attempt {0} out of {1})", retryNumber, maxReconnectRetries);
                try
                {
                    my_simconnect = new Microsoft.FlightSimulator.SimConnect.SimConnect("Managed Data Request", IntPtr.Zero, 0x402, null, 0);

                    Console.WriteLine("Successfully connected to simulator");
                }
                catch (COMException)
                {
                    Console.WriteLine("Unable to connect to sim. Reconnecting in {0} seconds...", (float)reconnectInterval / 1000);
                    Thread.Sleep(reconnectInterval);
                }
            }

            handleMessage = callback;

            return true;
        }

        private static void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {

        }

        private static void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {

        }

        private static void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            timer.Dispose();
            Console.WriteLine("Simulator has exited. Closing connection and exiting Simulator module");
            closeConnection();
            return;

        }

        private static void closeConnection()
        {
            if (my_simconnect != null)
            {
                my_simconnect.Dispose();
                my_simconnect = null;
            }

            return;
        }

        private static void RequestSimulatorData()
        {
            sendDataRequestToSimulator();
            receiveMessagesFromSimulator();
        }

        private static void sendDataRequestToSimulator()
        {
            if (my_simconnect != null)
            {
                my_simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_1, DEFINITIONS.Struct1, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
        }

        private static void receiveMessagesFromSimulator()
        {
            if (my_simconnect != null)
            {
                my_simconnect.ReceiveMessage();
            }
        }

        public static void TimerCallback(Object o)
        {
            RequestSimulatorData();
        }

        private static void sendEvent(string eventName, uint value)
        {
            if (Enum.IsDefined(typeof(EVENTS), eventName))
            {
                EVENTS eventToSend = (EVENTS)Enum.Parse(typeof(EVENTS), eventName);
                my_simconnect.MapClientEventToSimEvent((Enum)eventToSend, eventName);
                my_simconnect.TransmitClientEvent(0U, (Enum)eventToSend, value, (Enum)NOTIFICATION_GROUPS.GROUP0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
            }
            else
            {
                Console.WriteLine("Received event {0} is not defined. Will not be processed nor sent to the simulator.");
            }
        }

        public bool isAlive()
        {
            return (my_simconnect != null);
        }

    }
}
