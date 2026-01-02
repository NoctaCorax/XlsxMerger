// Services/MergeEngine.cs
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XlsxMerger.Services
{
    public class MergeEngine
    {
        /// <summary>
        /// Merges multiple Excel files into a single output file.
        /// Reports progress via IProgress interface.
        /// </summary>
        /// <param name="inputPaths">List of full paths to input files.</param>
        /// <param name="outputPath">Full path where the result will be saved.</param>
        /// <param name="progress">Optional reporter for progress updates (0-100).</param>
        // FIX: Dodano znak zapytania '?' przy IProgress<int>, co mówi kompilatorowi:
        // "Ten parametr może być nullem i to jest celowe".
        public void MergeFiles(List<string> inputPaths, string outputPath, IProgress<int>? progress = null)
        {
            // 1. Prepare the output workbook
            using (var outputWorkbook = new XLWorkbook())
            {
                var firstFilePath = inputPaths[0];
                List<string> sheetNames;

                using (var firstWb = new XLWorkbook(firstFilePath))
                {
                    sheetNames = firstWb.Worksheets.Select(ws => ws.Name).ToList();
                }

                // Calculate total operations for progress bar (Sheets * Files)
                int totalOperations = sheetNames.Count * inputPaths.Count;
                int currentOperation = 0;

                // 2. Iterate through each worksheet name
                foreach (var sheetName in sheetNames)
                {
                    var outputSheet = outputWorkbook.Worksheets.Add(sheetName);
                    int currentOutputRow = 1;

                    // 3. Process each input file
                    for (int i = 0; i < inputPaths.Count; i++)
                    {
                        var filePath = inputPaths[i];

                        using (var inputWb = new XLWorkbook(filePath))
                        {
                            var inputSheet = inputWb.Worksheet(sheetName);
                            if (inputSheet != null)
                            {
                                int startRow = (i == 0) ? 1 : 2;
                                var lastRowUsed = inputSheet.LastRowUsed();

                                if (lastRowUsed != null)
                                {
                                    int inputRowCount = lastRowUsed.RowNumber();
                                    for (int row = startRow; row <= inputRowCount; row++)
                                    {
                                        var sourceRow = inputSheet.Row(row);
                                        var targetRow = outputSheet.Row(currentOutputRow);
                                        sourceRow.CopyTo(targetRow);
                                        currentOutputRow++;
                                    }
                                }
                            }
                        }

                        // Update Progress
                        currentOperation++;
                        if (progress != null)
                        {
                            int percentComplete = (int)((double)currentOperation / totalOperations * 100);
                            progress.Report(percentComplete);
                        }
                    }
                }

                // 4. Save the final result
                outputWorkbook.SaveAs(outputPath);

                // Ensure progress shows 100% at the end
                progress?.Report(100);
            }
        }
    }
}