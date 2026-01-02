// Services/SchemaValidator.cs
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using XlsxMerger.Models;

namespace XlsxMerger.Services
{
    public class SchemaValidator
    {
        /// <summary>
        /// Validates that all input files have identical structure (worksheets and headers).
        /// </summary>
        /// <returns>AppValidationResult with success status or error details.</returns>
        public AppValidationResult ValidateSchema(IList<InputFile> files)
        {
            if (files == null || files.Count < 2)
            {
                return AppValidationResult.Success();
            }

            try
            {
                var referenceFile = files[0];
                var referenceSchema = ExtractSchema(referenceFile.FilePath);

                for (int i = 1; i < files.Count; i++)
                {
                    var currentFile = files[i];
                    var currentSchema = ExtractSchema(currentFile.FilePath);

                    if (!AreWorksheetNamesEqual(referenceSchema, currentSchema))
                    {
                        return AppValidationResult.Error(
                            $"File '{currentFile.Filename}' has different worksheets than '{referenceFile.Filename}'.\n" +
                            "All files must have the same worksheets in the same order.");
                    }

                    foreach (var sheetName in referenceSchema.Keys)
                    {
                        var refHeaders = referenceSchema[sheetName];
                        var curHeaders = currentSchema[sheetName];

                        if (!AreHeadersEqual(refHeaders, curHeaders))
                        {
                            return AppValidationResult.Error(
                                $"File '{currentFile.Filename}', Sheet '{sheetName}': Headers do not match '{referenceFile.Filename}'.\n" +
                                $"Expected: {string.Join(", ", refHeaders.Take(3))}...\n" +
                                $"Found: {string.Join(", ", curHeaders.Take(3))}...");
                        }
                    }
                }

                return AppValidationResult.Success();
            }
            catch (Exception ex)
            {
                return AppValidationResult.Error($"Validation failed due to file error: {ex.Message}");
            }
        }

        private Dictionary<string, List<string>> ExtractSchema(string filePath)
        {
            var schema = new Dictionary<string, List<string>>();

            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    List<string> headers = new List<string>();

                    if (!worksheet.IsEmpty())
                    {
                        var firstRow = worksheet.Row(1);
                        foreach (var cell in firstRow.CellsUsed())
                        {
                            headers.Add(cell.GetString().Trim());
                        }
                    }
                    schema.Add(worksheet.Name, headers);
                }
            }
            return schema;
        }

        private bool AreWorksheetNamesEqual(Dictionary<string, List<string>> schema1, Dictionary<string, List<string>> schema2)
        {
            if (schema1.Count != schema2.Count) return false;
            return schema1.Keys.SequenceEqual(schema2.Keys);
        }

        private bool AreHeadersEqual(List<string> headers1, List<string> headers2)
        {
            if (headers1.Count != headers2.Count) return false;
            return headers1.SequenceEqual(headers2);
        }
    }
}