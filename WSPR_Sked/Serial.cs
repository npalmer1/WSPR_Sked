using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO.Ports;
using System.IO;
using System.Windows.Forms;
using static Mysqlx.Notice.Warning.Types;

namespace SerialComms
{
    public class SerialComm
    {
        public readonly byte[] Message;

        public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
        public readonly string ErrorMessage;
       
        public readonly string ErrorMessageDetails = string.Empty;

        // Create the serial port with basic settings
        //SerialPort port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.Two);
        SerialPort serialPort = new SerialPort();

       

        public SerialComm(string code) //test set freq on Yaesu ft450
        {
            serialPort.PortName = "COM3"; // Replace with your COM port name
            serialPort.BaudRate = 9600;   // Set the baud rate (must match the receiving device)
            serialPort.Parity = Parity.None; // Set parity (None, Even, Odd, etc.)
            serialPort.DataBits = 8;       // Set data bits
            serialPort.StopBits = StopBits.Two; // Set stop bits
            serialPort.Handshake = Handshake.None; // Set flow control (None, RTS/CTS, etc.)

            // Set read/write timeouts (optional)
            serialPort.ReadTimeout = 500;
            serialPort.WriteTimeout = 500;
            try
            {
                serialPort.Open();
                

                // String to send
                string message = "FA014250000;";

                // Send the string
                serialPort.WriteLine(message);
                

                // Close the serial port
                serialPort.Close();
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;             
                return;

            }
        }
       

    }
}


