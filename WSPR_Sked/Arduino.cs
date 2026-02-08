using M0LTE.WsjtxUdpLib.Messages;
using Org.BouncyCastle.Pqc.Crypto.Ntru;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Arduino
{
    public class ArduinoComms
    {
        //public string reply;
        public bool ok;
        public string response;
        public ArduinoComms(string serverIP, int port, int pin, bool high, string type, string freq)
        {
            //string serverIP = "192.168.0.207"; // Replace with Arduino's IP
            //int port = 5000;
            if (type.Contains("switch)"))
            {
                response = setSWitch(serverIP, port, pin, high);
            }
            else if (type.Contains("filter"))
            {
                    response = setFilter(serverIP, port, freq);
            }
            if (response !="error" || response != "")
            {
                ok = true;
            }
            else
            {
                ok = false;
            }            

        }

        private string setSWitch(string serverIP, int port, int pin, bool high)
        {
            string reply = "";
            string hilo;
            if (high)
            {
                hilo = "high";
            }
            else
            {
                hilo = "low";
            }
            try
            {
                using (TcpClient client = new TcpClient(serverIP, port))
                using (NetworkStream stream = client.GetStream())
                {
                    // Send antenna settiings to Arduino
                    string message = "set-pin " + pin + " " + hilo;
                    byte[] data = Encoding.ASCII.GetBytes(message + "\n");
                    stream.Write(data, 0, data.Length);
                    

                    // Receive response from Arduino
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //MessageBox.Show("Received: " + response);
                    reply = response;                    
                }
            }
            catch (Exception ex)
            {

                //reply = ex.Message;
                reply = "Switching error";
                return reply;
            }
            return reply;
        }

        private string setFilter(string serverIP, int port, string freq)
        {
            string reply = "";
          
            try
            {
                using (TcpClient client = new TcpClient(serverIP, port))
                using (NetworkStream stream = client.GetStream())
                {
                    // Send antenna settiings to Arduino
                    string message = "filter:" + freq;
                    byte[] data = Encoding.ASCII.GetBytes(message + "\n");
                    stream.Write(data, 0, data.Length);


                    // Receive response from Arduino
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //MessageBox.Show("Received: " + response);
                    reply = response;
                }
            }
            catch (Exception ex)
            {

                //reply = ex.Message;
                reply = "Filter error";
                return reply;
            }
            return reply;
        }
        private void testArduino(string serverIP, int port)
        {

            try
            {
                using (TcpClient client = new TcpClient(serverIP, port))
                using (NetworkStream stream = client.GetStream())
                {
                    MessageBox.Show("Connected to Arduino");

                    // Send data to Arduino
                    string message = "Hello, Arduino!";
                    byte[] data = Encoding.ASCII.GetBytes(message + "\n");
                    stream.Write(data, 0, data.Length);
                    MessageBox.Show("Sent: " + message);

                    // Receive response from Arduino
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    MessageBox.Show("Received: " + response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }

}
//--------------------Arduino code in C++ --------------------:
/*
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

*/
