using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UdpClient.Helpers;

namespace UdpClient
{
    internal class Program
    {
        private const string _userInputPattern = @"^-\S+";

        static void Main(string[] args)
        {
            System.Net.Sockets.UdpClient client = new System.Net.Sockets.UdpClient();

            (IPAddress serverIp, int serverPort) = (IPAddress.Parse(args[0].Split('=')[1]), int.Parse(args[1].Split('=')[1]));
            IPEndPoint serverIPEndpoint = new IPEndPoint(serverIp, serverPort);

            CancellationTokenSource cts = new CancellationTokenSource();

            Task receivingTask = new Task(() =>
            { // UDP does not ensure that the data will be received straight away (or ever), so we need to query it in a separate thread
                while (true)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (client.Available > 0)
                    {
                        var receivedData = client.Receive(ref serverIPEndpoint);
                        var decodedReceivedData = Encoding.ASCII.GetString(receivedData);

                        ConsoleDisplayHelper.DisplayMessageInColor($"{decodedReceivedData}", ConsoleColor.DarkYellow);
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
                        ConsoleDisplayHelper.DisplayMessageInColor("Provide your nick: ", ConsoleColor.White);

                        nickName = Console.ReadLine();
                    }

                    sendBytes = Encoding.ASCII.GetBytes("-ru " + nickName);
                    client.Send(sendBytes, sendBytes.Length);

                    while (client.Available == 0) // waiting for registration feedback
                    {
                        continue;
                    }

                    var receivedData = client.Receive(ref serverIPEndpoint);
                    var decodedReceivedData = Encoding.ASCII.GetString(receivedData);

                    if (decodedReceivedData.StartsWith("RegistrationFailed:"))
                    {
                        ConsoleDisplayHelper.DisplayMessageInColor(decodedReceivedData, ConsoleColor.Red);

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
                    Console.ForegroundColor = ConsoleColor.White;
                    message = Console.ReadLine();
                    Match match = Regex.Match(message, _userInputPattern);

                    if (match.Success)
                    {
                        switch (match.Value)
                        {
                            case "-lu":
                            case "-dm":
                                sendBytes = Encoding.ASCII.GetBytes(message);
                                client.Send(sendBytes, sendBytes.Length);

                                ConsoleDisplayHelper.DisplayMessageInColor($"Sent: {message}", ConsoleColor.DarkGreen);

                                break;
                            case "-o":
                                OptionsMenuHelper.DisplayOptionsMenu();

                                break;
                            case "-q":
                                break;
                            default:
                                ConsoleDisplayHelper.DisplayMessageInColor($"Unrecognized option: {match.Value}!", ConsoleColor.DarkMagenta);

                                break;
                        }
                    }
                    else
                    {
                        ConsoleDisplayHelper.DisplayMessageInColor($"Message has to start with `-` followed by one of the options: {message}!",
                            ConsoleColor.DarkMagenta);
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleDisplayHelper.DisplayMessageInColor(e.ToString(), ConsoleColor.Red);
            }
            finally
            {
                cts.Cancel();
                client.Close();
            }

            ConsoleDisplayHelper.DisplayMessageInColor("Client stopped.", ConsoleColor.White);

            cts.Dispose();

            Console.ReadKey();
        }
    }
}
