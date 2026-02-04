using MathNet.Numerics;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
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


        public OtherTX(string protocol, string ip, string port, string baud, string serial, string message)
        {
            if (protocol.Contains("Serial"))
            {
                runSerial(port, serial, baud, message);
            }
            else
            {
                runIP(ip, port, message);

            }
        }
        private void runIP(string ip, string port, string message)
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
            // Create a TCP client
            TcpClient client = new TcpClient(ip, portno);
            try
            {



                // Get the network stream for reading and writing
                NetworkStream stream = client.GetStream();

                // Send a message to the server

                byte[] dataToSend = Encoding.ASCII.GetBytes(message + "\n");
                stream.Write(dataToSend, 0, dataToSend.Length);


                // Buffer to store the server's response
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // Convert the response to a string
                reply = Encoding.ASCII.GetString(buffer, 0, bytesRead);


                // Close the client connection
                client.Close();
                client.Dispose();


            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                client.Close();
                client.Dispose();
                reply = "error";
                return;
            }
        }
        private void runSerial(string port, string serial, string baud, string message)
        {
            try
            {
                string[] S = serial.Split(',');
                string parity = S[0];
                int bits = Convert.ToInt32(S[1]);
                int stop = Convert.ToInt32(S[2]);
                string handshake = S[3];

                int baudrate = 9600;
                baudrate = Convert.ToInt32(baud);
                sendSerial(port, baudrate, bits, parity, stop, handshake, message);
            }
            catch (Exception ex)
            {
                reply = ex.Message;
                return;
            }
        }
        public void sendSerial(string port, int baudrate, int bits, string parity, int stop, string handshake, string message)
        {

            serialPort.PortName = port; // Replace with your COM port name
            serialPort.BaudRate = baudrate;   // Set the baud rate (must match the receiving device)
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
            serialPort.DataBits = bits;       // Set data bits
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
                reply = ex.Message;
                return;

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
        