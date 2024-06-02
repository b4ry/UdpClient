using System.Net;
using System.Text;

namespace UdpClient.MessageProcessors
{
    internal class ReceiveDataProcessor
    {
        internal static string ReceiveData(System.Net.Sockets.UdpClient client, ref IPEndPoint serverIPEndpoint)
        {
            var receivedData = client.Receive(ref serverIPEndpoint);
            var decodedReceivedData = Encoding.ASCII.GetString(receivedData);

            return decodedReceivedData;
        }
    }
}
