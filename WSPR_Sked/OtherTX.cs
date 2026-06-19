using MathNet.Numerics;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WSPR_Sked;

namespace Other_TX
{
    public class OtherTX
    {
        public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage;
        public string reply;
        public readonly string replymsg;        

        public readonly string ErrorMessageDetails = string.Empty;

        public readonly byte[] Message;

        // Create the serial port with basic settings
      
        SerialPort serialPort = new SerialPort();


        public OtherTX(string protocol, string ip, string port, string baud, string serial, string message, string url)
        {
            if (protocol.Contains("Serial"))
            {
                runSerial(port, serial, baud, message);
            }
            else
            {
                runIP(ip, port, message,url);

            }
        }
        private void runIP(string ip, string port, string message,string url)
        {  // message may be single line or multiple lines separated by ";"

            try
            {
                message.Split(";").ToList().ForEach(m =>
                {
                    m = m.Replace(" ", "+");
                    sendIPMsg(ip, port, url+m);
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                reply = "error";
                return;

            }
        }

        private async Task<string> sendIPMsg(string ip, string port, string url)
        {
            string result = "error";
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
              
                var response = await client.GetAsync(url);
                result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                result = "error";
            }
            return result;
        }
        private void sendIPMsg_OLD(string ip, string port, string message)
        {
            reply = "error";
            int portno = 0;
            try
            {
                portno = Convert.ToInt32(port);
            }
            catch
            {
                reply = "error";
                return;
            }

            using (TcpClient client = new TcpClient())
            {
                try
                {
                    var connectTask = client.ConnectAsync(ip, portno);
                    if (!connectTask.Wait(2000)) 
                    {
                        ErrorMessage = "Connection timed out";
                        reply = "error";
                        return;
                    }

                    NetworkStream stream = client.GetStream();
                    byte[] dataToSend = Encoding.ASCII.GetBytes(message + "\n");
                    stream.Write(dataToSend, 0, dataToSend.Length);

                    stream.ReadTimeout = 2000; 

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    reply = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }
                catch (Exception e)
                {
                    ErrorMessage = e.Message;
                    reply = "error";
                }
            }
        }
      

        private void runSerial(string port, string serial, string baud, string message)
        {
            string parity = "none";
            int bits = 0;
            int stop = 1;
            string handshake = "none";
            if (message.Trim() == "")
            {
                return;
            }
            try
            {
                string[] S = serial.Split(',');
                try
                {
                    parity = S[0];
                    bits = Convert.ToInt32(S[1]);
                    stop = Convert.ToInt32(S[2]);
                    handshake = S[3];
                }
                catch
                {
                    parity = "none";
                    bits = 8;
                    stop = 1;
                    handshake = "none";
                }

                int baudrate = 9600;
                baudrate = Convert.ToInt32(baud);
                sendSerial(port, baudrate, bits, parity, stop, handshake, message);
                if (reply =="error")
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                reply = ex.Message;
                return;
            }
        }
        public void sendSerial(string port, int baudrate, int bits, string parity, int stop, string handshake, string message)
        {

            serialPort.PortName = port; 
            serialPort.BaudRate = baudrate;  
            Parity p;
            if (parity == "none")
            {
                p = Parity.None;
            }
            else if (parity == "even")
            {
                p = Parity.Even;
            }
            else
            {
                p = Parity.Odd;
            }

            serialPort.Parity = p; // Set parity (None, Even, Odd, etc.)
            serialPort.DataBits = bits;      
            StopBits s;
            if (stop == 1)
            {
                s = StopBits.One;
            }
            else
            {
                s = StopBits.Two;
            }
            Handshake h;
            if (handshake == "none")
            {
                h = Handshake.None;
            }
            else if (handshake == "xon/xoff")
            {
                h = Handshake.XOnXOff;
            }
            else if (handshake == "RTS")
            {
                h = Handshake.RequestToSend;
            }
            else
            {
                h = Handshake.None;
            }
            serialPort.StopBits = s; // Set stop bits
            serialPort.Handshake = h; // Set flow control (None, RTS/CTS, etc.)

            // Set read/write timeouts (optional)
            serialPort.ReadTimeout = 1000;
            serialPort.WriteTimeout = 1000;
            try
            {
                serialPort.Open();

                // Send the message
                serialPort.WriteLine(message);
                Task.Delay(500).Wait();

                reply = serialPort.ReadLine();

                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                //reply = ex.Message;
                reply = "error";

            }
            finally
            {
                // Close the serial port
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
        }

    }


}
        