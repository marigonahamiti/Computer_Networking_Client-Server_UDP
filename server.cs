using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPServer
{
    static void Main()
    {
        string serverName = "";
        int serverPort = 1200;

        UdpClient serverS = new UdpClient(serverPort);
        Console.WriteLine($"Serveri eshte startuar ne localhost ne portin: {serverPort}");

        HashSet<IPEndPoint> clients = new HashSet<IPEndPoint>();

        while (clients.Count < 4)
        {
            IPEndPoint clientAddress = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = serverS.Receive(ref clientAddress);
            clients.Add(clientAddress);

            Console.WriteLine($"Klienti u lidh me {clientAddress.Address} ne portin {clientAddress.Port}");
            string message = Encoding.UTF8.GetString(data);
            Console.WriteLine($"Kerkesa nga klienti: {message}");

            string messageK = message.ToUpper();
            Console.WriteLine($"Pergjigja nga serveri: {messageK}");

            byte[] responseData = Encoding.UTF8.GetBytes(messageK);
            serverS.Send(responseData, responseData.Length, clientAddress);
        }

        Console.WriteLine("Lista e klientave eshte mbushur!");
        serverS.Close();
    }
}

