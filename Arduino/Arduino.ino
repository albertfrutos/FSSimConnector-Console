
#include <SPI.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels

#define OLED_RESET     4 // Reset pin # (or -1 if sharing Arduino reset pin)
#define SCREEN_ADDRESS 0x3C ///< See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32

Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

// Rotary Encoder Inputs
#define CLK 8
#define DT 9
#define SW 10

int counter = 0;
int currentStateCLK;
int lastStateCLK;
String currentDir = "";
unsigned long lastButtonPress = 0;

char *events[] = {"DUMMY_0","AP_MASTER","TOGGLE_FLIGHT_DIRECTOR","AP_HEADING_HOLD","HEADING_BUG_SET","AP_ALT_HOLD",
                    "AP_ALT_VAR_SET_ENGLISH","AP_PANEL_VS_HOLD","AP_VS_VAR_SET_ENGLISH","FLIGHT_LEVEL_CHANGE",
                    "AP_SPD_VAR_SET","AUTO_THROTTLE_ARM","AP_BC_HOLD","AP_APR_HOLD","YAW_DAMPER_SET"};
                    
char *menuLabels[] = {"DUMMY_MENU","AP MASTER","AP FD","HDG STATUS","HDG VALUE","ALT STATUS","ALT VALUE","VS STATUS","VS VALUE",
                      "FLC STATUS","FLC VALUE","AUTO THRO","BACK COURSE","APPR","YAW DAMP"};

int variablesValues[] =  {999,  0,  0,  0,  0,    0,  0,      0,  0,    0,  0,    0,  0,  0,  0};
int minValues[] =        {999,  0,  0,  0,  0,    0,  0,      0,  0,    0,  0,    0,  0,  0,  0};
int maxValues[] =        {999,  1,  1,  1,  359,  1,  15000,  1,  10000,1,  1500, 1,  1,  1,  1};
int stepValues[] =       {999,  1,  1,  1,  1,    1,  100,    1,  100,  1,  100,  1,  1,  1,  1};
int mode = 0; //0 = menu // 1 - edit variable value // 2 - save value & send to sim & back to mode 0
int menuIndex = 1;
int value = 0;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);

  Serial.begin(115200);

  if (!display.begin(SSD1306_SWITCHCAPVCC, SCREEN_ADDRESS)) {
    //Serial.println(F("SSD1306 allocation failed"));
    for (;;); // Don't proceed, loop forever
  }
  
  display.display();

  // Set encoder pins as inputs
  pinMode(CLK, INPUT);
  pinMode(DT, INPUT);
  pinMode(SW, INPUT_PULLUP);

  // Setup Serial Monitor
  
  // Read the initial state of CLK
  lastStateCLK = digitalRead(CLK);
}

void refreshScreen(bool clearDisplay, int x, int y) {
  
  if (clearDisplay) {
    display.clearDisplay();
  }

  display.setTextSize(1);
  display.setTextColor(SSD1306_WHITE);
  
  display.setCursor(10,0);
  display.println(menuLabels[menuIndex]);

  display.setCursor(10,25);
  display.println(variablesValues[menuIndex]);
  
  display.display();
}

int getVariableValue(int menuIndex) {
  return variablesValues[menuIndex];
}

int handleVariableChangeByEncoder(int value, int minValue = minValues[menuIndex], int maxValue = maxValues[menuIndex], int stepValue = stepValues[menuIndex] ) {

    currentStateCLK = digitalRead(CLK);
    if (currentStateCLK != lastStateCLK  && currentStateCLK == 1) {
      if (digitalRead(DT) != currentStateCLK) {
        value = value - stepValue;
        if(value < minValue){
          value = maxValue;
        }
        currentDir = "CCW";
      } else {
        // Encoder is rotating CW so increment
        value = value + stepValue;
        if(value > maxValue){
          value = minValue;
        }
        currentDir = "CW";
      }
    }
    // Remember last CLK state
    lastStateCLK = currentStateCLK;
  
    // Read the button state
    int btnState = digitalRead(SW);
    
    //If we detect LOW signal, button is pressed
    if (btnState == LOW) {
      //if 50ms have passed since last LOW pulse, it means that the
      //button has been pressed, released and pressed again
      if (millis() - lastButtonPress > 50) {
        if (mode == 0) { // mode 0 is vars menu
          mode = 1;
        } else if (mode == 1) { // mode 1 is variable value change menu
          mode = 2;
        }
      }
  
      // Remember last button press event
      lastButtonPress = millis();
    }
  return value;
}

void showVarsMenu() {
  menuIndex = handleVariableChangeByEncoder(menuIndex,1,14,1);
  if (menuIndex < 1) {
    menuIndex = 14;
  } else if (menuIndex > 14) {
    menuIndex = 1;
  }
  
  refreshScreen(1, 0, 0);
}

void loop() {

  if (mode == 0) {  // show menu
    showVarsMenu();
  } else if (mode == 1) { // edit variable
    value = getVariableValue(menuIndex);
    value = handleVariableChangeByEncoder(value);
    setVariableValue(menuIndex, value);
    refreshScreen(1, 0, 0);
  } else if (mode == 2) {  // save, send to sim and go back to mode 0
    sendUpdate(menuIndex, value);
    mode = 0;
  }
  manageSerialCommunication();
}

void sendUpdate(int eventID, int value){
  String command = "@" + String(eventID) + "/";
  command.concat(String(events[eventID]));
  command.concat("=" + String(getVariableValue(eventID)) + "$");
  Serial.println(command);
}

void setVariableValue(int varID, int value) {
  variablesValues[varID] = value;
  varID = 999;
}

void manageSerialCommunication() {
  if (Serial.available() > 0) {
    Serial.readStringUntil('@');
    int varID = (Serial.readStringUntil('/')).toInt();
    String name = Serial.readStringUntil('=');
    int value = (Serial.readStringUntil('$')).toInt();

    if (varID = 999){
      handleInternalCommand(varID, name, value);
    }else{
      variablesValues[varID] = value;
    }
  }
}

void handleInternalCommand(int varID, String name, int value){
  String internalCommandResponse;
  if (varID == "KA"){
    Serial.println("@999/KA=1$");
  }
}
