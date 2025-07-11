using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessagePack;


namespace WSJTX //experimental comms with WSJT-X
{
    class WSJT
    {
        public WSJT()
        {
            WSJT_talk();
            WSJT_Receive();
        }
        private bool WSJT_talk()
        {
            string wsjtxIp = "127.0.0.1"; // Replace with WSJT-X IP address
            int wsjtxPort = 2237; // Default WSJT-X UDP port


            Heartbeat data = new Heartbeat { hb = 0, Id = "1", Max = 3, version = "1", revision = "1" }; 

            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Connect(wsjtxIp, wsjtxPort);

                // Construct your message here following WSJT-X protocol
                string message = "Your WSPR message here";
                // byte[] data = Encoding.UTF8.GetBytes(message);

                byte[] binaryData = MessagePackSerializer.Serialize(data);


                //udpClient.Send(data, data.Length);
                udpClient.Send(binaryData, binaryData.Length);
                MessageBox.Show("Message sent to WSJT-X.");
               
            }
            

            return true;
        }
        private bool WSJT_Receive()
        {
            string wsjtxIp = "127.0.0.1"; // Replace with WSJT-X IP address
            int wsjtxPort = 2237; // Default WSJT-X UDP port

            IPAddress ipAddress = IPAddress.Parse(wsjtxIp);
            //IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, wsjtxPort);
            using (UdpClient udpClient = new UdpClient(wsjtxPort))
            {
                while (true)
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = udpClient.Receive(ref remoteEndPoint);

                    string message = Encoding.UTF8.GetString(receivedData);
                    MessageBox.Show($"Received message: {message}");
                }
            }
            return true;
        }


        [Key(0)]
        public int Id { get; set; }

        [Key(1)]
        public string Name { get; set; }
        private bool heartbeat()
        {
            Heartbeat hb = new Heartbeat();
            hb.hb = 0;
            hb.Id = "1";
            hb.Max = 3;
            hb.version = "1";
            hb.revision = "1";
            return true;
        }

        [MessagePackObject]
        public struct Heartbeat
        {
            /*Message       Direction Value                  Type
            * ------------- --------- ---------------------- -----------
            * Heartbeat     Out/In    0                      quint32
            *                         Id (unique key)        utf8
            *                         Maximum schema number  quint32
            *                         version                utf8
            *                         revision               utf8*/
            [Key(0)]
            public int hb { get; set; }
            [Key(1)]
            public string Id { get; set; }
            [Key(2)]
            public int Max { get; set; }
            [Key(3)]
            public string version { get; set; }
            [Key(4)]
            public string revision { get; set; }

        }

    }

}
