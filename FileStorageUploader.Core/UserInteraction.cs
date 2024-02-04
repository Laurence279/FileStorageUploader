using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageUploader.Core
{
    public static class UserInteraction
    {
        public static bool Confirm(string prompt)
        {
            do
            {
                Console.WriteLine("{0} (Y/N)", prompt);
                var key = char.ToUpper(Console.ReadKey(true).KeyChar);
                if (key == 'N')
                {
                    return false;
                }
                else if (key == 'Y')
                {
                    break;
                }
            } while (true);
            return true;
        }

        public static string GetInput(string prompt)
        {
            Console.WriteLine("{0}", prompt);
            var result = Console.ReadLine();
            return result ?? string.Empty;
        }

        public static void Print(string message)
        {
            Console.WriteLine(message);
        }

        public static void WaitForKey()
        {
            Console.ReadKey();
        }
    }
}
