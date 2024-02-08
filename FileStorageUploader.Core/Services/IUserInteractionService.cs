namespace FileStorageUploader.Core.Services
{
    public interface IUserInteractionService
    {
        public bool Confirm(string prompt);

        public string GetInput(string prompt);

        public void PrintLine(string message);

        public void Print(string message);

        public void WaitForKey();
    }
}
