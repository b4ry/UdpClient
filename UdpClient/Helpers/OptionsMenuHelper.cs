using System;

namespace UdpClient.Helpers
{
    internal static class OptionsMenuHelper
    {
        internal static void DisplayOptionsMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("------------------ OPTIONS ------------------");
            Console.WriteLine("-o                     - display options menu,");
            Console.WriteLine("-lu                    - get a list of actively registered users,");
            Console.WriteLine("-dm message -NICK_NAME - send a direct message to a particular user, eg. Hello! -John");
            Console.WriteLine("-q                     - close the application.");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("---------------------------------------------");
        }
    }
}
