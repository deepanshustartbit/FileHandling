using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FileHandlingSystem
{
    public class FileHelper
    {
        public static string ComputeHash(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha = SHA256.Create();

            var hashBytes = sha.ComputeHash(stream);
            return Convert.ToHexString(hashBytes);
        }
    }
}
