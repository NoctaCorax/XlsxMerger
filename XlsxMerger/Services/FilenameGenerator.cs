// Services/FilenameGenerator.cs
using System;
using System.Globalization; // Required for week number
using System.Text.RegularExpressions;

namespace XlsxMerger.Services
{
    /// <summary>
    /// Service responsible for generating the final output filename based on a template.
    /// Supports placeholders like %yyyy, %mo, %d, %user, %machine, etc.
    /// </summary>
    public class FilenameGenerator
    {
        // Updated method signature to include optional 'fileCount'
        public string GenerateFilename(string template, string prefix, string suffix, bool replaceSpaces, int fileCount = 0)
        {
            // Start with the current time
            DateTime now = DateTime.Now;
            string filename = template;

            // --- GROUP 0: Standard Date/Time ---
            filename = filename.Replace("%yyyy", now.ToString("yyyy")); // 2024
            filename = filename.Replace("%yy", now.ToString("yy"));     // 24
            filename = filename.Replace("%mo", now.ToString("MM"));     // 05
            filename = filename.Replace("%d", now.ToString("dd"));      // 15
            filename = filename.Replace("%h", now.ToString("HH"));      // 14
            filename = filename.Replace("%mi", now.ToString("mm"));     // 30
            filename = filename.Replace("%s", now.ToString("ss"));      // 45

            // --- GROUP 1: Extended Date/Time (NEW) ---
            // %ms - Milliseconds
            filename = filename.Replace("%ms", now.ToString("fff"));

            // %day - Day name (e.g., Monday)
            filename = filename.Replace("%day", now.ToString("dddd"));

            // %q - Quarter (1-4)
            int quarter = (now.Month + 2) / 3;
            filename = filename.Replace("%q", quarter.ToString());

            // %wk - Week number (1-52)
            // Use current system culture calendar
            var culture = CultureInfo.CurrentCulture;
            int weekNum = culture.Calendar.GetWeekOfYear(now, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
            filename = filename.Replace("%wk", weekNum.ToString("00"));

            // --- GROUP 2: File Context (NEW) ---
            // %count - Number of files being merged
            filename = filename.Replace("%count", fileCount.ToString());

            // --- GROUP 3: System Info (NEW) ---
            // %user - Windows username
            filename = filename.Replace("%user", Environment.UserName);

            // %machine - Computer name
            filename = filename.Replace("%machine", Environment.MachineName);


            // 2. Add Prefix and Suffix
            filename = $"{prefix}{filename}{suffix}";

            // 3. Replace spaces if requested
            if (replaceSpaces)
            {
                filename = filename.Replace(" ", "_");
            }

            // 4. Basic cleanup (remove double underscores)
            filename = Regex.Replace(filename, @"_{2,}", "_");

            // 5. Ensure extension exists
            if (!filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".xlsx";
            }

            return filename;
        }
    }
}