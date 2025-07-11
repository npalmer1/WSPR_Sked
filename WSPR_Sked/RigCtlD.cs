using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using WSPR_Sked;
using System.Windows.Forms;
using MySqlX.XDevAPI;

namespace RigControlDaemon
{
    public class RigCtlD
    {
        public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
        public readonly string ErrorMessage;
        public string reply;
        public readonly string replymsg;        

        public readonly string ErrorMessageDetails = string.Empty;


        public RigCtlD(string ip, string portno, string message)
        {
            int port = 0;
            reply = "error";            

            
                try
                {
                    port = Convert.ToInt32(portno);
                }
                catch
                {
                    
                    reply = "error";
                    return;
                   
                }
                // Create a TCP client
                TcpClient client = new TcpClient(ip, port);
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
                client.Close();
                client.Dispose();
                reply = "error";
                return;
            }
            
        }
           
    }
        
        
}
