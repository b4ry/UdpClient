using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (UdpClient client = new UdpClient())
            {
                bool isRegistered = false;

                try
                {
                    string message = string.Empty;

                    (IPAddress serverIp, int serverPort) = (IPAddress.Parse(args[1].Split('=')[1]), int.Parse(args[0].Split('=')[1]));
                    IPEndPoint serverIPEndpoint = new IPEndPoint(serverIp, serverPort);

                    client.Connect(serverIPEndpoint); // since it is UDP, the connect DOES NOT establish a session

                    byte[] sendBytes;

                    string nickName = string.Empty;

                    while (!isRegistered)
                    {
                        while (string.IsNullOrWhiteSpace(nickName))
                        {
                            Console.Write("Provide your nick:");
                            nickName = Console.ReadLine();
                        }

                        sendBytes = Encoding.ASCII.GetBytes("UserRegistration:" + nickName);
                        client.Send(sendBytes, sendBytes.Length);

                        while (client.Available == 0)
                        {
                            continue;
                        }

                        var receivedData = client.Receive(ref serverIPEndpoint);
                        var decodedReceivedData = Encoding.ASCII.GetString(receivedData);

                        if (decodedReceivedData.StartsWith("RegistrationFailed:"))
                        {
                            Console.WriteLine(decodedReceivedData);
                            nickName = string.Empty;
                        }
                        else
                        {
                            isRegistered = true;
                        }
                    }

                    Console.WriteLine($"Registered as: {nickName}, IP {client.Client.LocalEndPoint}");

                    while (message != "q")
                    {
                        PrepareMessage(out message, out sendBytes);

                        client.Send(sendBytes, sendBytes.Length);
                        Console.WriteLine($"Sent: {message}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private static void PrepareMessage(out string message, out byte[] sendBytes)
        {
            Console.Write(":");
            message = Console.ReadLine();
            sendBytes = Encoding.ASCII.GetBytes(message);
        }
    }
}
