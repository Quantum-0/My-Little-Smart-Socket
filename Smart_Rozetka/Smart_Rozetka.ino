#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <EEPROM.h>
 
const char* ssid = "DSL-2600U";
const char* password = "";
 
int relayPin = D2;
ESP8266WebServer server(80);
IPAddress ip(192, 168, 1, 75);
IPAddress gateway(192, 168, 1, 1);
bool CurrentState = LOW;
bool DefaultState = LOW;
int DefaultTimer = 0;
ulong Timer = 0;

void handle_Defaults()
{
  bool setDefaultState = false;
  bool setDefaultTimer = false;
  for (uint8_t i=0; i<server.args(); i++)
  {
    if (server.argName(i) == "timer")
    {
      DefaultTimer = server.arg(i).toInt();
      setDefaultTimer = true;
      EEPROM.write(1, DefaultTimer);
    }
    if (server.argName(i) == "state")
    {
      byte temp = server.arg(i).toInt();
      if (temp != 1 && temp != 0)
      {
        server.send(400, "text/plain", "STATE MUST BE 1 OR 0");
        return;
      }
      DefaultState = temp;
      setDefaultState = true;
      EEPROM.write(0, DefaultState);
    }
  }

  if (setDefaultState || setDefaultTimer)
  {
    EEPROM.commit();
    server.send(200, "text/plain", "OK");
  }
  else
  {
    String message = "STATE: ";
    message += (int)DefaultState;
    message += "\nTIMER: ";
    message += DefaultTimer;
    server.send(200, "text/plain", message);
  }
}

void handle_Off()
{
  if (CurrentState == LOW)
  {
    server.send(200, "text/plain", "ALREADY OFF");
    return;
  }
  Timer = 0;
  CurrentState = LOW;
  digitalWrite(relayPin, !CurrentState);
  server.send(200, "text/plain", "OK");
}

void handle_On()
{
  if (CurrentState == HIGH)
  {
    server.send(200, "text/plain", "ALREADY ON");
    return;
  }
  Timer = 0;
  CurrentState = HIGH;
  digitalWrite(relayPin, !CurrentState);
  for (uint8_t i=0; i<server.args(); i++)
  {
    if (server.argName(i) == "timer")
    {
      int timer = server.arg(i).toInt();
      Timer = millis() + (timer * 60000);
    }
  }
  server.send(200, "text/plain", "OK");
}

void handle_Status()
{
  String message = "STATE: ";
  if (CurrentState)
    message += "ON\n";
  else
    message += "OFF\n";
  if (Timer == 0)
    message += "NO TIMER";
  else
  {
    message += "TIMER: ";
    message += (Timer - millis()) / 60000;
    message += ':';
    message += ((Timer - millis()) / 1000) % 60;
  }
  message += "\nUPTIME: ";
  message += millis() / 1000;
  server.send(200, "text/plain", message);
}

void loadDefaults()
{
  byte val = EEPROM.read(0); // STATE: OFF 0; ON 1
  if (val == 1)
    DefaultState = HIGH;
  if (val == 0)
    DefaultState = LOW;
  CurrentState = DefaultState;
  digitalWrite(relayPin, !CurrentState);

  int timer = EEPROM.read(1); // TIMER in minutes
  DefaultTimer = timer;
  Timer = timer * 60000;
}
 
void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);
  Serial.begin(115200);
  EEPROM.begin(512);
  delay(10); 
  pinMode(relayPin, OUTPUT);
  loadDefaults();
  IPAddress subnet(255, 255, 255, 0);
  WiFi.config(ip, gateway, subnet); 
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
    delay(500);

  server.onNotFound([](){server.send(404, "text/plain", "");});
  server.on("/on", handle_On);
  server.on("/off", handle_Off);
  server.on("/status", handle_Status);
  server.on("/defaults", handle_Defaults);
  server.begin();
}
 
void loop() {
  server.handleClient();
  if (Timer != 0 && millis() >= Timer)
  {
    Timer = 0;
    CurrentState = LOW;
    digitalWrite(relayPin, !CurrentState);
  }
  digitalWrite(LED_BUILTIN, (millis() / 250) % 10 != 0);
  delay(1);
}
