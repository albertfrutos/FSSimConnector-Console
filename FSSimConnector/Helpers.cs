using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSSimConnector
{
    internal class Helpers
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct Struct1
        {
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public bool ApMaster; // on/off bool - AUTOPILOT MASTER
            public bool ApFlightDirector;  // on/off bool - AUTOPILOT FLIGHT DIRECTOR ACTIVE

            public bool ApHdgStatus;  // on/off bool - AUTOPILOT HEADING LOCK
            public int ApHdgValue; // int degrees - AUTOPILOT HEADING LOCK DIR

            public bool ApAltitudeStatus;  // on/off - bool AUTOPILOT ALTITUDE LOCK
            public int ApAltitudeValue; //  int feet - AUTOPILOT ALTITUDE LOCK VAR

            public bool ApVerticalSpeedStatus; // on/off bool - AUTOPILOT VERTICAL HOLD	
            public int ApVerticalSpeedValue; //  int feet/minute - AUTOPILOT VERTICAL H
                                             //  OLD VAR
            public bool ApFLCStatus;  // on/off bool - AUTOPILOT AIRSPEED HOLD	
            public int ApFLCValue; // int knots - AUTOPILOT AIRSPEED HOLD	VAR

            public bool ApAutoThrottle;  // on/off bool - AUTOPILOT THROTTLE ARM	
            public bool ApBackCourse;  // on/off bool - AUTOPILOT BACKCOURSE HOLD		
            public bool ApApproach;  // on/off bool - AUTOPILOT APPROACH HOLD	
            public bool ApYawDamper;  // on/off bool - AUTOPILOT YAW DAMPER	
        }

        internal enum DATA_REQUESTS
        {
            REQUEST_1
        }

        internal enum DEFINITIONS
        {
            Struct1
        }

        internal enum EVENTS
        {
            AP_MASTER, //0,1
            TOGGLE_FLIGHT_DIRECTOR, //0,1 toggle

            AP_HEADING_HOLD,
            HEADING_BUG_SET, //int

            AP_ALT_HOLD,
            AP_ALT_VAR_SET_ENGLISH, //int

            AP_PANEL_VS_HOLD,
            AP_VS_VAR_SET_ENGLISH, //int

            FLIGHT_LEVEL_CHANGE,
            AP_SPD_VAR_SET, //int

            AUTO_THROTTLE_ARM, //0,1 toggle

            AP_BC_HOLD,

            AP_APR_HOLD,

            YAW_DAMPER_SET, //0,1 // YAW_DAMPER_TOGGLE ??
        }

        internal static Dictionary<string, int> structToID = new Dictionary<string, int>()
        {
            {"ApMaster",1},
            {"ApFlightDirector",2},
            {"ApHdgStatus",3},
            {"ApHdgValue",4},
            {"ApAltitudeStatus",5},
            {"ApAltitudeValue",6},
            {"ApVerticalSpeedStatus",7},
            {"ApVerticalSpeedValue",8},
            {"ApFLCStatus",9},
            {"ApFLCValue",10},
            {"ApAutoThrottle",11},
            {"ApBackCourse",12},
            {"ApApproach",13},
            {"ApYawDamper",14}
        };

        internal enum NOTIFICATION_GROUPS
        {
            GROUP0,
        }
    }
}
