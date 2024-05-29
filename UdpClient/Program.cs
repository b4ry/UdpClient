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
            UdpClient client = new UdpClient();

            try
            {
                string message = "";

                (IPAddress serverIp, int serverPort) = (IPAddress.Parse(args[1].Split('=')[1]), int.Parse(args[0].Split('=')[1]));
                IPEndPoint serverIPEndpoint = new IPEndPoint(serverIp, serverPort);

                client.Connect(serverIPEndpoint);

                IPEndPoint listenerEndpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] sendBytes;

                while (message != "q")
                {
                    Console.Write(":");
                    message = Console.ReadLine();
                    sendBytes = Encoding.ASCII.GetBytes(message);

                    client.Send(sendBytes, sendBytes.Length);
                    Console.WriteLine($"Sent: {message}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                client.Close();
            }
        }
    }
}
