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
    private static IPEndPoint clientWithFullAccess = null;

    static void Main()
    {
        Task.Run(async () => await StartServerAsync());

        // Keep the server running until the user presses Enter
        Console.ReadLine();
    }

    private static bool VerifyClientAccess(IPEndPoint clientAddress, string authMessage)
    {
        if (authMessage.StartsWith("CONNECT:ADMIN"))
        {
            string[] adminCredentials = authMessage.Substring(13).Split(':');

            // Ensure that the array has the expected length to avoid potential index out of range errors
            if (adminCredentials.Length == 2)
            {
                string adminUsername = adminCredentials[0];
                string adminPassword = adminCredentials[1];

                // Check if the provided username and password match the expected admin credentials
                if (adminUsername == "admin" && adminPassword == "admin123")
                {
                    return true; // Valid admin credentials
                }
            }
        }

        return false; // Invalid credentials
    }

    static async Task StartServerAsync()
    {
        string serverName = "";
        int serverPort = 1200;

        IPAddress ipv4Address = IPAddress.Parse("192.168.0.23");
        UdpClient serverS = new UdpClient(new IPEndPoint(ipv4Address, serverPort));
        Console.WriteLine($"Serveri eshte startuar ne IP adresen: {ipv4Address}, portin: {serverPort}");


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
                bool hasFullAccess = VerifyClientAccess(clientAddress, message);

                if (hasFullAccess)
                {
                    Console.WriteLine($"Client {clientAddress.Address} has full access.");
                    clientWithFullAccess = clientAddress;

                    // Send a message to the client indicating full access
                    byte[] fullAccessMessage = Encoding.UTF8.GetBytes("FULL_ACCESS");
                    await serverS.SendAsync(fullAccessMessage, fullAccessMessage.Length, clientAddress);
                }
                else
                {
                    Console.WriteLine($"Client {clientAddress.Address} does not have full access.");

                    // Send a message to the client indicating restricted access
                    byte[] restrictedAccessMessage = Encoding.UTF8.GetBytes("RESTRICTED_ACCESS");
                    await serverS.SendAsync(restrictedAccessMessage, restrictedAccessMessage.Length, clientAddress);

                    // Handle the case where the client doesn't have full access
                    // For example, you can send a message back to the client or take other actions.
                    continue;
                }

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
                    }
                    else
                    {
                        Console.WriteLine($"Invalid ADMIN credentials from {clientAddress.Address} on port {clientAddress.Port}");

                        // Send a message to the client indicating invalid credentials but granting regular access
                        byte[] invalidCredentialsMsg = Encoding.UTF8.GetBytes("Invalid credentials, accessing as a regular client");
                        await serverS.SendAsync(invalidCredentialsMsg, invalidCredentialsMsg.Length, clientAddress);

                        // Continue with regular client handling
                        // For example, you can remove the 'else' statement to handle the invalid credentials as a regular client
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
                        string filePath = Path.Combine(@"C:\Users\ZoneTech\Desktop\Detyra2\", fileName);

                        // Check if the file exists before attempting to open it
                        if (File.Exists(filePath))
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
                            Console.WriteLine($"File '{fileName}' not found in the server folder.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error opening file: {ex.Message}");
                    }
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
