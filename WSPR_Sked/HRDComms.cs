using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;


namespace HRDComms
{
    public class HRDComm
    {
        public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
        public readonly string ErrorMessage;

        public readonly string ErrorMessageDetails = string.Empty;

        string host = "127.0.0.1"; // HRD Rig Control host (localhost if on the same computer)
        int port = 7809;           // HRD Rig Control default port
        string frequency = "set_frequency 14074000"; // Example frequency (14.074 MHz for FT8)



        //public HRDComm(string message)
        public HRDComm(byte[] bytes)
        {
            try
            {
                // Create a TCP client and connect to HRD Rig Control
                using (TcpClient client = new TcpClient())
                {
                    MessageBox.Show($"Connecting to HRD Rig Control at {host}:{port}...");
                    client.Connect(host, port);
                    MessageBox.Show("Connected successfully.");

                    // Get the network stream for reading and writing
                    using (NetworkStream stream = client.GetStream())
                    {
                        // Command to set frequency
                        //string command = $"{message}\n";
                        //string command = $"set_frequency {message}\n";



                        //string command = $"{message}";                                            
                        //byte[] commandBytes = Encoding.ASCII.GetBytes(command);
                        byte[] commandBytes = bytes;

                        // Send the command
                        //MessageBox.Show($"{command.Trim()}");
                        stream.Write(commandBytes, 0, commandBytes.Length);

                        // Read the response from HRD
                        byte[] buffer = new byte[1024];
                        //int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        //string response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                        string response = "Test";
                        // Display the response
                        MessageBox.Show($"Response from HRD: {response}");
                    }
                    client.Close();
                }

                ErrorMessage = string.Empty;
            }
            catch (SocketException ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show($"Socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show($"Error: {ex.Message}");
            }

            MessageBox.Show("Test complete");
        }
    }
}

        

/*
 * try
{
    // Set the server IP address and port
    string serverIp = "127.0.0.1"; // Localhost
    int port = 8000;

    // Create a TCP client
    TcpClient client = new TcpClient(serverIp, port);
    Console.WriteLine("Connected to server!");

    // Get the network stream for writing
    NetworkStream stream = client.GetStream();

    // The string to send
    string message = "Hello, Server!";

    // Convert the string to bytes
    byte[] data = Encoding.UTF8.GetBytes(message);

    // Send the data to the server
    stream.Write(data, 0, data.Length);
    Console.WriteLine("Message sent: " + message);

    // Close the client
    stream.Close();
    client.Close();
    Console.WriteLine("Client disconnected.");
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}
}*/


