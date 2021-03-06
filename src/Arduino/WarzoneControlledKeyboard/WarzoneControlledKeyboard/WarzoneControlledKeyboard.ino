#include <SoftwareSerial.h>
#include <SerialCommand.h>      // Steven Cogswell ArduinoSerialCommand library from http://GitHub.com
#include <Keyboard.h>
#define ONE_WIRE_BUS 2
#define LED_LOW      3
#define LED_HIGH     6
#define DBGMSG(A)    if (dbg){ Serial.print("DBG: "); Serial.println(A);}

//
// Globals
//
SerialCommand     serialCommand;
boolean           dbg = true;

//
// Initialize the serial command table, I/O pins, and the temperature sensor
//
void setup() {
  Serial.begin(9600);
  serialCommand.addCommand("reload", Reload );
  serialCommand.addCommand("armor", Armor );
  serialCommand.addCommand("sprint", Sprint );
  serialCommand.addCommand("ping", Ping );
  serialCommand.addCommand("cut", CutChute );
  serialCommand.addCommand("jump", Jump );
  serialCommand.addCommand("crouch", Crouch );
  serialCommand.addCommand("prone", Prone );
  serialCommand.addCommand("forwards", Forward );
  serialCommand.addCommand("left", Left );
  serialCommand.addCommand("right", Right );
  serialCommand.addCommand("map", Map );
  serialCommand.addCommand("item", UseItem );
  serialCommand.addCommand("swap", SwapWeapon );
  serialCommand.addCommand("killstreak", Killstreak );
  serialCommand.addCommand("shoot", Shoot );
  serialCommand.addCommand("aim", Aim );
  serialCommand.addCommand("grenade", Grenade );
  serialCommand.addCommand("altgrenade", AlternateGrenade );

  serialCommand.addCommand("debug", SetDebug );
  serialCommand.setDefaultHandler(UnrecognizedCommand );
}

//
// Read and respond to commands recieved over the serial port
//
void loop() {
  serialCommand.readSerial();
}

void Reload() {
  SendBasicKeyCommand('r');
}
void UseItem() {
  SendBasicKeyCommand('f');
}
void Shoot() {
  SendBasicKeyCommand('9');
}
void Aim() {
  SendBasicKeyCommand('0');
}
void Map() {
  SendBasicKeyCommand('m');
}
void SwapWeapon() {
  SendBasicKeyCommand('2');
}

void Killstreak() {
  SendBasicKeyCommand('3');
}

void Sprint() {
  SendBasicKeyCommand(KEY_LEFT_SHIFT);
}
void Ping() {
  SendBasicKeyCommand(KEY_LEFT_ALT);
}
void CutChute() {
  SendBasicKeyCommand('c');
}
void Jump() {
  SendBasicKeyCommand(' ');
}
void Grenade() {
  SendBasicKeyCommand('g');
}
void AlternateGrenade() {
  SendBasicKeyCommand('q');
}
void Crouch() {
  SendBasicKeyCommand(KEY_LEFT_CTRL);
}
void Prone() {
  Keyboard.begin();
  Keyboard.press(KEY_LEFT_CTRL);
  delay(500);
  Keyboard.releaseAll();
  Keyboard.end();
}
void Armor() {
  SendBasicKeyCommand('4');
}
void Left() {

  char *arg = serialCommand.next();

  Keyboard.begin();
  Keyboard.press('a');
  
  if (arg != NULL) {
    delay(2500);    
  } else {
    delay(2500);    
  }
  Keyboard.releaseAll();
  Keyboard.end();
}

void Right() {

  char *arg = serialCommand.next();

  Keyboard.begin();
  Keyboard.press('d');
   if (arg != NULL) {
    delay(2500);    
  } else {
    delay(2500);    
  }
  Keyboard.releaseAll();
  Keyboard.end();
}
void Forward() {

  char *arg = serialCommand.next();

  Keyboard.begin();
  Keyboard.press('w');
  
  if (arg != NULL) {
    delay(2500);    
  } else {
    delay(2500);    
  }
  Keyboard.releaseAll();
  Keyboard.end();
}

void Backwards() {

  char *arg = serialCommand.next();

  Keyboard.begin();
  Keyboard.press('s');
   if (arg != NULL) {
    delay(2500);    
  } else {
    delay(2500);    
  }
  Keyboard.releaseAll();
  Keyboard.end();
}
void SendBasicKeyCommand(char key) {
  Keyboard.begin();
  Keyboard.press(key);
  Keyboard.releaseAll();
  Keyboard.end();
}
//
// Enable or disable debug messages from being printed
// on the serial terminal
//
void SetDebug() {
  char *arg = serialCommand.next();

  if (arg != NULL) {
    if ( strcmp(arg, "on" ) == 0) {
      dbg = true;
      DBGMSG(F("Turn on debug"));
    }
    if ( strcmp(arg, "off" ) == 0) {
      DBGMSG(F("Turn off debug"));
      dbg = false;
    }
  }
}

//
// An unrecognized command was recieved
//
void UnrecognizedCommand() {
  DBGMSG(F("Unrecognized command"));
  DBGMSG(F(" ledon 3  - turn on led connected to digital I/O 3"));
  DBGMSG(F(" ledon 4  - turn on led connected to digital I/O 4"));
  DBGMSG(F(" ledon 5  - turn on led connected to digital I/O 5"));
  DBGMSG(F(" ledon 6  - turn on led connected to digital I/O 6"));
  DBGMSG(F(" ledoff 3 - turn off led connected to digital I/O 3"));
  DBGMSG(F(" ledoff 4 - turn off led connected to digital I/O 4"));
  DBGMSG(F(" ledoff 5 - turn off led connected to digital I/O 5"));
  DBGMSG(F(" ledoff 6 - turn off led connected to digital I/O 6"));
  DBGMSG(F(" temp     - read temperature" ));
  DBGMSG(F(" debug on - turn on debug messages" ));
  DBGMSG(F(" debug off- turn off debug messages" ));
}
