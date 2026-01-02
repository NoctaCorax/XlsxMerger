// Models/InputFile.cs
using System;

namespace XlsxMerger.Models
{
    /// <summary>
    /// Represents a single Excel file selected by the user.
    /// This model will be displayed in the DataGrid.
    /// </summary>
    public class InputFile
    {
        // Name of the file with extension (e.g., "report.xlsx")
        public string Filename { get; set; }

        // Full path to the file on disk (e.g., "C:\Docs\report.xlsx")
        public string FilePath { get; set; }

        // Formatted file size (e.g., "15 KB")
        public string SizeFormatted { get; set; }

        // Current status of the file (e.g., "Ready", "Error")
        // We will use this later for validation icons
        public string Status { get; set; }

        /// <summary>
        /// Constructor to easily create a new InputFile
        /// </summary>
        public InputFile(string path)
        {
            FilePath = path;
            Filename = System.IO.Path.GetFileName(path);

            // Basic file info logic
            var info = new System.IO.FileInfo(path);
            long sizeInBytes = info.Length;
            SizeFormatted = $"{sizeInBytes / 1024} KB"; // Simple conversion to KB

            Status = "Ready"; // Default status
        }
    }
}