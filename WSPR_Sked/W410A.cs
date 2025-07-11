using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W410A
{
    public class W410A_Switch
    {

        public readonly byte[] Message;
        public string reply;

        public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
        public readonly string ErrorMessage;

        public readonly string ErrorMessageDetails = string.Empty;

        // Create the serial port with basic settings
        //SerialPort port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.Two);
        SerialPort serialPort = new SerialPort();
        public W410A_Switch(string port, int baud, int bits, string parity, int stop, string handshake, string message)
        {
        
            serialPort.PortName = port; // Replace with your COM port name
            serialPort.BaudRate = baud;   // Set the baud rate (must match the receiving device)
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
            if (stop ==1)
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
