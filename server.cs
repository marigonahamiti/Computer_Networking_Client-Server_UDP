using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class UDPServer
{
    static void Main()
    {
        Task.Run(async () => await StartServerAsync());

        // Keep the server running until the user presses Enter
        Console.ReadLine();
    }
 static IPEndPoint clientWithFullAccess = null;
    static async Task StartServerAsync()
    {
        string serverName = "";
        int serverPort = 1200;

        UdpClient serverS = new UdpClient(serverPort);
        Console.WriteLine($"Serveri eshte startuar ne localhost ne portin: {serverPort}");

        List<IPEndPoint> clients = new List<IPEndPoint>();
        int maxClients = 5;

        while (clients.Count < maxClients)
        {
            UdpReceiveResult receiveResult = await serverS.ReceiveAsync();
            IPEndPoint clientAddress = receiveResult.RemoteEndPoint;
            byte[] data = receiveResult.Buffer;

            if (!clients.Contains(clientAddress))
            {
                clients.Add(clientAddress);
                Console.WriteLine($"Klienti {clients.Count} u lidh me {clientAddress.Address} ne portin {clientAddress.Port}");
            }

            string message = Encoding.UTF8.GetString(data);
            if (message.StartsWith("CONNECT:"))
            {
                if (message.Equals("CONNECT:CLIENT"))
                {
                    Console.WriteLine($"Client connected from {clientAddress.Address} on port {clientAddress.Port}");
                }
                else if (message.StartsWith("CONNECT:ADMIN"))
                {
                    string[] adminCredentials = message.Substring(13).Split(':');
                    string adminUsername = adminCredentials[0];
                    string adminPassword = adminCredentials[1];

                    if (adminUsername == "admin" && adminPassword == "admin123")
                    {
                        Console.WriteLine($"Admin connected from {clientAddress.Address} on port {clientAddress.Port}");
                        clientWithFullAccess = clientAddress;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid ADMIN credentials from {clientAddress.Address} on port {clientAddress.Port}");
                        byte[] invalidCredentialsMsg = Encoding.UTF8.GetBytes("Invalid ADMIN credentials");
                        await serverS.SendAsync(invalidCredentialsMsg, invalidCredentialsMsg.Length, clientAddress);
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid connection request from {clientAddress.Address} on port {clientAddress.Port}");
                    byte[] invalidConnectionMsg = Encoding.UTF8.GetBytes("Invalid connection request");
                    await serverS.SendAsync(invalidConnectionMsg, invalidConnectionMsg.Length, clientAddress);
                    continue;
                }
            }

            if (message.StartsWith("FILE:"))
            {
                string fileName = message.Substring(5); // Remove the "FILE:" prefix
                Console.WriteLine($"Received file content from client {clients.Count} for file: {fileName}");

            }
            else
            {
                Console.WriteLine($"Kerkesa nga klienti {clients.Count}: {message}");
                if (message.StartsWith("WRITE:"))
                {
                    string fileName = message.Substring(6); // Remove the "WRITE:" prefix
                    Console.WriteLine($"Received a request to write content to file: {fileName}");

                    // Handle the request to write content to the file
                    // For example, you can prompt the server to receive the content from the client.
                    // Note: This example doesn't implement the content receiving part; you can customize it based on your needs.
                }
                // ... (existing code)

                else if (message.StartsWith("OPEN:"))
                {
                    string fileName = message.Substring(5); // Remove the "OPEN:" prefix
                    Console.WriteLine($"Received a request to open file: {fileName}");

                    try
                    {
                        // Combine the file path with the server folder
                        string filePath = Path.Combine(@"C:\Users\ZoneTech\Desktop\Projekti2_Rrjeta_Kompjuterike-main\test.txt", fileName);

                        // Check if the file exists before attempting to open it
                  if (File.Exists(filePath))
    {
        if (clientWithFullAccess == null || clientAddress.Equals(clientWithFullAccess))
        {
            // Open the file using the default program associated with its type
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
            Console.WriteLine($"File '{fileName}' opened successfully.");
        }
        else
        {
            Console.WriteLine($"Access denied. Client {clientAddress.Address} does not have full access.");
        }
    }
    else
    {
        Console.WriteLine($"File '{fileName}' not found in the server folder.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error opening file: {ex.Message}");
}
if (message.StartsWith("GRANT_FULL_ACCESS:"))
{
    string clientIpAddress = message.Substring(18); // Remove the "GRANT_FULL_ACCESS:" prefix
    IPEndPoint grantedClientAddress = new IPEndPoint(IPAddress.Parse(clientIpAddress), serverPort);

    // Grant full access to the specified client
    clientWithFullAccess = grantedClientAddress;
    Console.WriteLine($"Full access granted to client {clientWithFullAccess.Address} on port {clientWithFullAccess.Port}");
}
else if (message.StartsWith("EXECUTE:"))
{
    string command = message.Substring(8); // Remove the "EXECUTE:" prefix
    Console.WriteLine($"Received a request to execute command: {command}");

    try
    {
        // ... (existing code)
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error executing command: {ex.Message}");
    }
}      
else if (message.StartsWith("GRANT_FULL_ACCESS:"))
{
    string clientIpAddress = message.Substring(18); // Remove the "GRANT_FULL_ACCESS:" prefix
    IPEndPoint grantedClientAddress = new IPEndPoint(IPAddress.Parse(clientIpAddress), serverPort);

    // Grant full access to the specified client
    clientWithFullAccess = grantedClientAddress;
    Console.WriteLine($"Full access granted to client {clientWithFullAccess.Address} on port {clientWithFullAccess.Port}");
}

                else if (message.StartsWith("EXECUTE:"))
                {
                string command = message.Substring(8); // Remove the "EXECUTE:" prefix
                Console.WriteLine($"Received a request to execute command: {command}");

                try
                {
                    string output = "";

                    // Handle specific commands
                    if (command.StartsWith("mkdr"))
                    {
                        // Extract the directory name from the command
                        string dirName = command.Substring(5).Trim();

                        // Create the directory
                        Directory.CreateDirectory(dirName);

                        output = $"Directory '{dirName}' created successfully.";
                    }
                    else if (command.StartsWith("ls"))
                    {
                        // Get the list of files in the server folder
string[] files = Directory.GetFiles(@"C:\Users\ZoneTech\Desktop");
                        output = string.Join(Environment.NewLine, files);
                    }
                    else
                    {
                        // Execute other commands using a process
                        ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (Process process = new Process() { StartInfo = psi })
                        {
                            process.Start();

                            // Read the output and error streams
                            output = process.StandardOutput.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();

                            // Append error to the output if there is any
                            if (!string.IsNullOrEmpty(error))
                                output += $"{Environment.NewLine}Error:{Environment.NewLine}{error}";
                        }
                    }

                    // Send the output back to the client
                    string response = $"Output:{Environment.NewLine}{output}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await serverS.SendAsync(responseData, responseData.Length, clientAddress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing command: {ex.Message}");
                }
                }


                else
                {
                    string messageK = message.ToUpper();
                    Console.WriteLine($"Pergjigja nga serveri: {messageK}");

                    byte[] responseData = Encoding.UTF8.GetBytes(messageK);
                    await serverS.SendAsync(responseData, responseData.Length, clientAddress);
                }
            }
        }

        Console.WriteLine("Lista e klientave eshte mbushur!");

        // Inform clients that the list is full
        foreach (var client in clients)
        {
            string fullMessage = "Server: Lista e klientave eshte mbushur!";
            byte[] fullMessageData = Encoding.UTF8.GetBytes(fullMessage);
            await serverS.SendAsync(fullMessageData, fullMessageData.Length, client);
        }

    }

    }   
}
