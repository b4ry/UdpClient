using System.Text;

namespace UdpClient.MessageProcessors
{
    internal static class SendMessageProcessor
    {
        internal static void SendMessage(System.Net.Sockets.UdpClient client, string message)
        {
            byte[] sendBytes = Encoding.ASCII.GetBytes(message);
            client.Send(sendBytes, sendBytes.Length);
        }
    }
}
