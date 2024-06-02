using System;

namespace UdpClient.Helpers
{
    internal static class ConsoleDisplayHelper
    {
        internal static void DisplayMessageInColor(string message, ConsoleColor consoleColor)
        {
            // not a perfect solution; not atomic.
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
        }
    }
}
