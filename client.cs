using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPClient
{
    static void Main()
    {
        string serverName = "localhost";
        int serverPort = 1200;

        for (int i = 0; i < 5; i++)
        {
            using (UdpClient clientS = new UdpClient())
            {
                while (true)
                {
                    Console.Write("Enter a message to send to the server (or 'exit' to quit): ");
                    string message = Console.ReadLine();

                    if (message.ToLower() == "exit")
                        break;

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    clientS.Send(data, data.Length, serverName, serverPort);

                    IPEndPoint serverAddress = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = clientS.Receive(ref serverAddress);
                    string modifiedMessage = Encoding.UTF8.GetString(receivedData);

                    Console.WriteLine("Response from the server: " + modifiedMessage);
                }
            }
        }
    }
}

