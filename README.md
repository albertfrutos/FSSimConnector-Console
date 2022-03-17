# FSSimConnector (Console)
## What's this about?
This is a project that allows to connect Flight Simulator with an Arduino board via serial port and transfers basic automatic pilot parameters to and from it. This consists of two parts:

* A C# console application for Windows that is responsible for the interfacing tasks themselves: data and communication management (serial port to Arduino and connection with the simulator).
* An Arduino sketch that handles the data receives data from the simulator (through the C# application) and allows to see/modify them and send the update to the sim.

The Arduino board has several devices attached so the different parameters and its values can be seen and modified. Once the change in a paremeter is accepted in the Arduino side, it is transferred to the application and updated in the simulator. You can make your own physical AP panel!

**NOTE:** The board used during development is an Arduino Micro, so Arduino code refers to this board's pinout.

## Components attached to the board

* OLED 128x62 screen connected by I2C
* Rotary Encoder
* Arduino Micro (serial communication via micro USB port in the board, but TX/RX pin connections should work as well)

| Device         | Device pin | Arduino(u) pin |
|----------------|------------|----------------|
| Rotary Encoder | CLK        | 8              |
|                | DT         | 9              |
|                | SW         | 10             |
| OLED Screen    |            |                |
|                |            |                |

## How does the application work?
Once the application starts, it validates the configuration (this can be found in a JSON file under FSSimConnector/Files/config.json), in case this is not valid or the simulator or the COM (serial) port are not available, it stops and finishes. 

If all the checks are passed, then it starts two different threads: one for the simulator and one for the board communication, which can interchange information.

Application also sends all data from simulator to Arduino if configured properly.

## Application JSON configuration file example
```
{
  "sendAllDataAtStart": true,
  "showVariablesOnScreen": true,
  "serialPort": {
    "PortName": "COM50",
    "BaudRate": "115200",
    "Parity": "None",
    "DataBits": "8",
    "StopBits": "1",
    "keepAlive": {
      "enableKeepAlive": false,
      "keepAliveMillis": "2000",
      "keepAliveTimeoutMillis": "5000"
    }
  },
  "simulator": {
    "simDataRefreshIntervalMillis": 500,
    "reconnectInterval": 2000,
    "maxReconnectRetries": 5
  }
}
```
## Arduino board KeepAlive feature
As data is only transferred from one end to the other end when there is an update in the parameters (take into account that only the parameters that have change its value are transmitted) and long time can pass without update, the application has a KeepAlive feature that checks for the board every X seconds (keep alive and timeout intervals  can be configured and enabled/disabled in config.json). If enabled and no keep alive response is received from the board within the configured time since the application has sent a keep alive request, the communication is closed.
