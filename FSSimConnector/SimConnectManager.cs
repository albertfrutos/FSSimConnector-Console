using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FSSimConnector.Program;

namespace FSSimConnector
{
    internal class SimConnectManager : SimulatorHelpers
    {
        private static SimConnect my_simconnect = null;
        public static Timer timer1 = null;
        private static bool sendAllData = false;

        static serialManager updateArduinoCallback;

        internal static Struct1 previousData = new Struct1();
  
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
                
                /*
                Console.Clear();
                Console.WriteLine("ApMaster " + currentData.ApMaster);
                Console.WriteLine("ApFlightDirector " + currentData.ApFlightDirector);
                Console.WriteLine("ApHdgStatus " + currentData.ApHdgStatus);
                Console.WriteLine("ApHdgValue " + currentData.ApHdgValue);
                Console.WriteLine("ApAltitudeStatus " + currentData.ApAltitudeStatus);
                Console.WriteLine("ApAltitudeValue " + currentData.ApAltitudeValue);
                Console.WriteLine("ApVerticalSpeedStatus " + currentData.ApVerticalSpeedStatus);
                Console.WriteLine("ApVerticalSpeedValue " + currentData.ApVerticalSpeedValue);
                Console.WriteLine("ApFLCStatus " + currentData.ApFLCStatus);
                Console.WriteLine("ApFLCValue " + currentData.ApFLCValue);
                Console.WriteLine("ApAutoThrottle " + currentData.ApAutoThrottle);
                Console.WriteLine("ApBackCourse " + currentData.ApBackCourse);
                Console.WriteLine("ApApproach " + currentData.ApApproach);
                Console.WriteLine("ApYawDamper " + currentData.ApYawDamper);
                */

                if (!previousData.Equals(currentData))
                {
                    ProcessVarsAndSendToArduino(currentData,previousData);
                    previousData = currentData;
                }              
            }
        }

        private static void ProcessVarsAndSendToArduino(Struct1 currentData, Struct1 previousData)
        {
            FieldInfo[] fi = typeof(Struct1).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo info in fi)
            {
                if ((!info.GetValue(currentData).Equals(info.GetValue(previousData))) || sendAllData )
                {
                    string variableName = info.Name;
                    string variableValue = MapValues(info.GetValue(currentData).ToString());
                    int variableID;
                    structToID.TryGetValue(variableName, out variableID);
                    updateArduinoCallback("@" + variableID + "/" + variableName + "=" + variableValue + "$");
                }
            }
            sendAllData = false;
        }

        private static string MapValues(string value)
        {
            string mappedValue = value;

            if (value == "True")
            {
                mappedValue = "1";
            }
            else if (value == "False"){
                mappedValue = "0";
            }

            return mappedValue;
        }

        public void ProcessCommandFromArduino(string command)
        {
            var splittedCommand = command.Split('=');
            string eventName = splittedCommand[0];
            uint value = uint.Parse(splittedCommand[1]);
            //Console.WriteLine(eventName + " - " + value);
            sendEvent(eventName.Split('/')[1], value);
            RequestSimulatorData();
        }

        public void initDataRequest(serialManager callback, int refreshIntervalMillis, bool sendAllDataAtStart)
        {
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

                timer1 = new Timer(TimerCallback, null, 0, refreshIntervalMillis);

                RequestSimulatorData();

                updateArduinoCallback = callback;
            }
            catch (COMException exception1)
            {
                //displayText(exception1.Message);
            }
        }

        public bool connect(int reconnectInterval, int maxReconnectRetries = 5)
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
                    Console.WriteLine("Unable to connect to sim. Reconnecting in {0} seconds...", reconnectInterval);
                    Thread.Sleep(reconnectInterval);
                }
            }
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
            closeConnection();
            timer1 = null;
        }

        private static void closeConnection()
        {
            if (my_simconnect != null)
            {
                my_simconnect.Dispose();
                my_simconnect = null;
                //label_status.Text = "Connection closed";
            }
        }

        private static void RequestSimulatorData()
        {
            my_simconnect.ReceiveMessage();
            my_simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_1, DEFINITIONS.Struct1, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }


        private static void TimerCallback(Object o)
        {
            RequestSimulatorData();            
        }

        private static void sendEvent(string eventName, uint value)
        {
            EVENTS eventToSend = (EVENTS)Enum.Parse(typeof(EVENTS), eventName);
            my_simconnect.MapClientEventToSimEvent((Enum)eventToSend, eventName);
            my_simconnect.TransmitClientEvent(0U, (Enum)eventToSend, value, (Enum)NOTIFICATION_GROUPS.GROUP0, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }
    }
}
