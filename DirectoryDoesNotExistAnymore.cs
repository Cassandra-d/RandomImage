using System;

namespace RandomImage
{
    public class DirectoryDoesNotExistAnymore : Exception
    {
        private const string Key = "dir";
        public DirectoryDoesNotExistAnymore(string directory)
        {
            Data[Key] = directory;
        }

        public string Directory()
        {
            return (Data[Key] ?? string.Empty).ToString();
        }

        public override string Message
        {
            get
            {
                return string.Concat("Directory ", Directory(), " does not exist");
            }
        }
    }
}
