using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO.Ports;
using System.IO;
using System.Net;

namespace SurrogateRobot_receiveData
{
    class ReceiveData
    {
        Thread receiveThread, receiveThread2;
        UdpClient udpClient;
        //SerialPort _serialPort = new SerialPort("COM3", 115200);
        string TCP_SERVER_IP = "192.168.1.195";
        int TCP_port = 8051;
        int UDP_port = 8052;

        string udpMessage = "0,0,0";
        string tcpMessage = "0";

        bool isReceiveUdpMessage;
        bool isReceiveTcpMessage;

        string path;

        static void Main(string[] args)
        {
            ReceiveData receiver = new ReceiveData();
            receiver.init();
            //receiver._serialPort.Open();
            while (true)
            {
                if (receiver.isReceiveUdpMessage == true && receiver.isReceiveTcpMessage == false)
                {
                    string messageSerial = '(' + receiver.udpMessage + ",0)/";
                    Console.WriteLine(messageSerial);
                    receiver.isReceiveUdpMessage = false;
                }
                else if (receiver.isReceiveUdpMessage == false && receiver.isReceiveTcpMessage == true)
                {
                    string messageSerial = '(' + receiver.udpMessage + ',' + receiver.tcpMessage + ")/";
                    Console.WriteLine(messageSerial);
                    receiver.isReceiveTcpMessage = false;
                }

            }        
        }

        public void init()
        {
            receiveThread = new Thread(new ThreadStart(TCPReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            receiveThread2 = new Thread(new ThreadStart(UDPReceiveData));
            receiveThread2.IsBackground = true;
            receiveThread2.Start();
        }

        private void UDPReceiveData()
        {
            udpClient = new UdpClient(UDP_port);
            while (true)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpClient.Receive(ref anyIP);
                    udpMessage = Encoding.ASCII.GetString(data);
                    isReceiveUdpMessage = true;

                }
                catch (Exception err)
                {
                    Console.WriteLine("Robot Error :" + err.ToString());
                    udpClient.Close();
                }
            }

            udpClient.Close();
        }

        private void TCPReceiveData()
        {
            TcpClient socketConnection = null;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

            try
            {
                //_serialPort.Open();
                socketConnection = new TcpClient(TCP_SERVER_IP, TCP_port);
                Console.WriteLine("TCP client is connected with the server");
                Byte[] bytes = new Byte[1024];
                while (true)
                {
                    using (NetworkStream stream = socketConnection.GetStream())
                    {
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);

                            // serverMessage is a message from user side's computer.
                            tcpMessage = Encoding.ASCII.GetString(incommingData);
                            isReceiveTcpMessage = true;
                        }
                    }
                    
                }
                socketConnection.Close();
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("Socket exception: " + socketException);
                //socketConnection.Close();
            }
        }
    }
}
