using System;
using System.Collections.Generic;
using System.Text;

namespace FileHandlingSystem
{
    public class FileRecord
    {
        public int JobId { get; set; }
        public string FilePath { get; set; }
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }
    }
}
