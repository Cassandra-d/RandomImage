using System;
using System.IO;
using System.Security.Cryptography;

namespace RandomImage
{
    public static class Aux
    {
        public static DateTime GetModificationDate(string filePath)
        {
            try
            {
                var retVal = new DateTime();
                if (File.Exists(filePath))
                    retVal = File.GetLastWriteTime(filePath);
                return retVal;
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException ||
                    ex is ArgumentException ||
                    ex is PathTooLongException ||
                    ex is NotSupportedException)
                    return new DateTime();
                throw new InvalidProgramException("Getting modification date", ex);
            }
        }

        public static string GetHashCode(string imagePath)
        {
            var retVal = string.Empty;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(imagePath))
                {
                    retVal = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", null).ToLower();
                }
            }
            return retVal;
        }

        public static long GetSize(string imagePath)
        {
            return !File.Exists(imagePath) ? 0 : (new FileInfo(imagePath)).Length;
        }
    }
}