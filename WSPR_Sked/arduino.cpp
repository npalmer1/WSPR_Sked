#include <WiFi.h>

const char* ssid = "wifi_ssid"; //rewith yours
const char* password = "wifi_password"; //ditto
// Static IP configuration
//IPAddress local_IP(192, 168, 0, 185); // Set your desired static IP
//IPAddress gateway(192, 168, 0, 1);    // Set your network gateway
//IPAddress subnet(255, 255, 255, 0);   // Set your subnet mask
//IPAddress dns(192, 168, 0, 1);        // Optional: Set your DNS server


WiFiServer server(5000); // Port 5000 or other port if necessary


void setup() {
 
  // Configure static IP settings
  //WiFi.config(local_IP, gateway, subnet, dns);
  WiFi.setHostname("ArduinoSw");

  Serial.begin(9600);
  WiFi.begin(ssid, password);
  pinMode(LED_BUILTIN, OUTPUT);
  

  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Connecting to WiFi...");
    
  }

  Serial.println("Connected to WiFi");
  server.begin();
  Serial.println("Server started");
  delay(1000);
  Serial.println(WiFi.localIP());
}

void loop() {
  WiFiClient client = server.available();
  String msg = "error";
  if (client) {
    
    Serial.println("Client connected");
    
    while (client.connected()) {
      if (client.available()) {
        
        String data = client.readStringUntil('\n');
        int pin =1;
        int hilo;
        String S;
       
        
        Serial.println("Received: " + data);
        if (data.startsWith("set-pin "))
        {
          
           if (data.indexOf("high") > 0)
           {
             hilo = HIGH;
           }
           else
           {
             hilo = LOW;
           }
          S = "set-pin ";
          int L = S.length();
          pin = data.substring(L,L+1).toInt();
          String D = data.substring(L,L+1);
          
          //client.println("Echo: " + D);
          for (int i=0; i<8;i++)
          {
            if (i != pin)
            {
              pinMode(i, OUTPUT); // Set pin all other pins as an output
              digitalWrite(i, LOW); //set all others as low
            }
          }
         
            digitalWrite(LED_BUILTIN, hilo);  // turn the LED on or off
             pinMode(pin, OUTPUT); // Set pin as an output
            digitalWrite(pin, hilo); // Set pin to HIGH (5V)   
           String H = "low";
           if (data.indexOf("high") > 0)
           {
              H= "high";
           }
           
            msg = "Pin: " +D + " set: "+H; //response
        }
        delay(100); // Wait for 100mS
        
        
      }
    client.println(msg); // Send response back
    Serial.println("Returned: "+msg);
    client.stop();
    Serial.println("Client disconnected");
    }
  }
}