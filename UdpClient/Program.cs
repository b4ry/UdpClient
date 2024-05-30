using Client.OptionsMenu;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        private const string _pattern = @"^-\S+";

        static void Main(string[] args)
        {
            UdpClient client = new UdpClient();

            (IPAddress serverIp, int serverPort) = (IPAddress.Parse(args[1].Split('=')[1]), int.Parse(args[0].Split('=')[1]));
            IPEndPoint serverIPEndpoint = new IPEndPoint(serverIp, serverPort);

            CancellationTokenSource cts = new CancellationTokenSource();

            Task receivingTask = new Task(() =>
            { // UDP does not ensure that the data will be received straight away (or ever), so we need to query it
                while (true)
                {
                    if(cts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (client.Available > 0)
                    {
                        var receivedData = client.Receive(ref serverIPEndpoint);
                        var decodedReceivedData = Encoding.ASCII.GetString(receivedData);
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"{decodedReceivedData}");
                    }

                    Task.Delay(2000);
                }
            }, cts.Token, TaskCreationOptions.LongRunning);

            bool isRegistered = false;

            try
            {
                client.Connect(serverIPEndpoint); // since it is UDP, the connect DOES NOT establish a session

                byte[] sendBytes;

                string nickName = string.Empty;
                string message = string.Empty;

                while (!isRegistered)
                {
                    while (string.IsNullOrWhiteSpace(nickName))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Provide your nick:");
                        nickName = Console.ReadLine();
                    }

                    sendBytes = Encoding.ASCII.GetBytes("-ru " + nickName);
                    client.Send(sendBytes, sendBytes.Length);

                    while (client.Available == 0)
                    {
                        continue;
                    }

                    var receivedData = client.Receive(ref serverIPEndpoint);
                    var decodedReceivedData = Encoding.ASCII.GetString(receivedData);

                    if (decodedReceivedData.StartsWith("RegistrationFailed:"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(decodedReceivedData);
                        nickName = string.Empty;
                    }
                    else
                    {
                        isRegistered = true;
                    }
                }

                Console.WriteLine($"Registered as: {nickName}, IP {client.Client.LocalEndPoint}");

                OptionsMenuHelper.DisplayOptionsMenu();

                receivingTask.Start();

                while (message != "-q")
                {
                    PrepareMessage(out message, out sendBytes);
                    Match match = Regex.Match(message, _pattern);

                    if (match.Success)
                    {
                        switch (match.Value)
                        {
                            case "-lu":
                            case "-dm":
                                client.Send(sendBytes, sendBytes.Length);
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine($"Sent: {message}");

                                break;
                            case "-o":
                                OptionsMenuHelper.DisplayOptionsMenu();

                                break;
                            case "-q":
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                Console.WriteLine($"Unrecognized option!");

                                break;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine($"Unrecognized option!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
            }
            finally
            {
                cts.Cancel();
                client.Close();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Client stopped.");

            cts.Dispose();

            Console.ReadKey();
        }

        private static void PrepareMessage(out string message, out byte[] sendBytes)
        {
            Console.ForegroundColor = ConsoleColor.White;
            message = Console.ReadLine();
            sendBytes = Encoding.ASCII.GetBytes(message);
        }
    }
}
