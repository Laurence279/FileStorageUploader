namespace FileStorageUploader.Core.Services
{
    public class UserInteractionService : IUserInteractionService
    {
        public bool Confirm(string prompt)
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

        public string GetInput(string prompt)
        {
            Console.WriteLine("{0}", prompt);
            var result = Console.ReadLine();
            return result ?? string.Empty;
        }

        public void PrintLine(string message)
        {
            Console.WriteLine(message);
        }

        public void Print(string message)
        {
            Console.Write(message);
        }

        public void WaitForKey()
        {
            Console.ReadKey();
        }
    }
}
