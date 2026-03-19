using System;
using System.Collections.Generic;
using System.Text;

namespace FileHandlingSystem.Scanner
{
    public class FileScannerService
    {
        public IEnumerable<string> GetFiles(string path)
        {
            IEnumerable<string> files = Enumerable.Empty<string>();
            IEnumerable<string> directories = Enumerable.Empty<string>();

            try
            {
                files = Directory.EnumerateFiles(path);
                directories = Directory.EnumerateDirectories(path);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: {path}");
                yield break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {path} - {ex.Message}");
                yield break;
            }

            foreach (var file in files)
            {
                yield return file;
            }

            foreach (var dir in directories)
            {
                foreach (var file in GetFiles(dir))
                {
                    yield return file;
                }
            }
        }
    }
}
