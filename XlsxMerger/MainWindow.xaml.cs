// MainWindow.xaml.cs
using Microsoft.Win32;
using System;
using System.Collections.Generic; // For List<>
using System.Collections.ObjectModel;
using System.IO;
using System.Linq; // For Select()
using System.Threading.Tasks; // REQUIRED FOR ASYNC
using System.Windows;
using System.Windows.Controls;
using XlsxMerger.Models;
using XlsxMerger.Services;

// FIX: Update alias to point to the correct model class (AppValidationResult)
using ValidationResult = XlsxMerger.Models.AppValidationResult;

namespace XlsxMerger
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<InputFile> Files { get; set; }

        private FilenameGenerator _filenameGenerator;
        private MergeEngine _mergeEngine;
        private SchemaValidator _schemaValidator;
        private DebugLogger _logger;

        private string _selectedOutputDirectory;

        // Constants for Settings
        private const string DEFAULT_TEMPLATE = "merged_%d-%mo-%yyyy";
        private const string CONFIG_FILE = "user_settings.txt";
        private const string README_FILE = "README.md";

        public MainWindow()
        {
            InitializeComponent();

            _logger = new DebugLogger();
            _logger.Log(LogLevel.INFO, "=== Application Started ===");

            Files = new ObservableCollection<InputFile>();
            DgFiles.ItemsSource = Files;
            // Add listener for collection changes to update file count preview
            Files.CollectionChanged += Files_CollectionChanged;

            _filenameGenerator = new FilenameGenerator();
            _mergeEngine = new MergeEngine();
            _schemaValidator = new SchemaValidator();

            _selectedOutputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            TxtOutputDir.Text = _selectedOutputDirectory;

            // Load saved settings
            LoadSettings();

            UpdateFilenamePreview();
        }

        // --- Event handler for collection changes ---
        private void Files_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFilenamePreview();
        }

        // ---------------------------------------------------------
        // SECTION: SETTINGS MANAGEMENT
        // ---------------------------------------------------------

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string[] lines = File.ReadAllLines(CONFIG_FILE);
                    if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
                    {
                        TxtTemplate.Text = lines[0];
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.WARN, $"Failed to load settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                File.WriteAllText(CONFIG_FILE, TxtTemplate.Text);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.WARN, $"Failed to save settings: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Auto-save settings when closing app
            SaveSettings();
        }

        private void BtnRestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            // Restore default values
            TxtTemplate.Text = DEFAULT_TEMPLATE;
            TxtPrefix.Text = string.Empty;
            TxtSuffix.Text = string.Empty;
            ChkReplaceSpaces.IsChecked = true;
            ChkAllowOverwrite.IsChecked = false;

            UpdateFilenamePreview();
            _logger.Log(LogLevel.INFO, "User restored default settings.");
        }

        // --- ReadMe Button Logic ---
        private void BtnReadme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string readmePath = Path.Combine(baseDir, README_FILE);

                if (File.Exists(readmePath))
                {
                    string content = File.ReadAllText(readmePath);
                    ReadmeWindow readmeWindow = new ReadmeWindow(content);
                    readmeWindow.Owner = this;
                    readmeWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show(
                        $"Documentation file ({README_FILE}) not found.\nExpected path: {readmePath}\n\nMake sure 'README.md' is set to 'Copy to Output Directory' in Visual Studio.",
                        "Documentation Missing",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open documentation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------------------------------------------------------
        // SECTION 1: INPUT FILES EVENTS
        // ---------------------------------------------------------

        private void BtnSelectFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Select Excel Files to Merge"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _logger.Log(LogLevel.INFO, $"User selected {openFileDialog.FileNames.Length} files.");
                foreach (string filename in openFileDialog.FileNames)
                {
                    Files.Add(new InputFile(filename));
                }
            }
        }

        private void BtnClearList_Click(object sender, RoutedEventArgs e)
        {
            _logger.Log(LogLevel.INFO, "User cleared file list.");
            Files.Clear();
        }

        // ---------------------------------------------------------
        // SECTION 2: OUTPUT SETTINGS EVENTS
        // ---------------------------------------------------------

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder",
                Title = "Select Output Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                string? folderPath = Path.GetDirectoryName(dialog.FileName);

                if (!string.IsNullOrEmpty(folderPath))
                {
                    _selectedOutputDirectory = folderPath;
                    TxtOutputDir.Text = folderPath;
                    _logger.Log(LogLevel.INFO, $"Output directory changed to: {folderPath}");
                }
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filenameGenerator == null) return;
            UpdateFilenamePreview();
        }

        private void OnCheckChanged(object sender, RoutedEventArgs e)
        {
            if (_filenameGenerator == null) return;
            UpdateFilenamePreview();
        }

        private void UpdateFilenamePreview()
        {
            if (TxtTemplate == null || TxtPrefix == null || TxtSuffix == null || ChkReplaceSpaces == null) return;

            string template = TxtTemplate.Text;
            string prefix = TxtPrefix.Text;
            string suffix = TxtSuffix.Text;
            bool replaceSpaces = ChkReplaceSpaces.IsChecked == true;

            // Pass the current file count to the generator
            int count = Files != null ? Files.Count : 0;
            string filename = _filenameGenerator.GenerateFilename(template, prefix, suffix, replaceSpaces, count);

            TxtPreview.Text = filename;
        }

        // ---------------------------------------------------------
        // SECTION 3: MAIN OPERATION (MERGE)
        // ---------------------------------------------------------

        private async void BtnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (Files.Count == 0)
            {
                MessageBox.Show("Please select files first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveSettings();

            string filename = TxtPreview.Text;
            if (string.IsNullOrEmpty(filename)) filename = "merged.xlsx";
            string fullOutputPath = Path.Combine(_selectedOutputDirectory, filename);

            if (File.Exists(fullOutputPath))
            {
                if (ChkAllowOverwrite.IsChecked == true)
                {
                    var result = MessageBox.Show(
                        $"File '{filename}' already exists.\n\n" +
                        "YES: Overwrite it.\n" +
                        "NO: Auto-rename (create copy).\n" +
                        "CANCEL: Stop operation.",
                        "File Exists",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Cancel) return;

                    if (result == MessageBoxResult.No)
                    {
                        fullOutputPath = GetAutoRenamedPath(fullOutputPath);
                        MessageBox.Show($"File will be saved as:\n{Path.GetFileName(fullOutputPath)}", "Renaming", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    fullOutputPath = GetAutoRenamedPath(fullOutputPath);
                    MessageBox.Show(
                        $"File with this name already exists.\n\nOverwrite is disabled, so file will be saved as:\n{Path.GetFileName(fullOutputPath)}",
                        "File Renamed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }

            // --- START PROCESSING ---
            BtnMerge.IsEnabled = false;
            PbStatus.Value = 0;
            TxtStatus.Text = "Validating schemas...";
            _logger.Log(LogLevel.INFO, "Starting Validation Phase...");

            ValidationResult? validationResult = null;

            try
            {
                await Task.Run(() =>
                {
                    validationResult = _schemaValidator.ValidateSchema(Files);
                });
            }
            catch (Exception ex)
            {
                HandleError($"Validation crash: {ex.Message}");
                return;
            }

            if (validationResult != null && !validationResult.IsValid)
            {
                _logger.Log(LogLevel.WARN, $"Validation Failed: {validationResult.Message}");
                MessageBox.Show(validationResult.Message, "Schema Mismatch", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetUI();
                return;
            }

            List<string> inputPaths = Files.Select(f => f.FilePath).ToList();

            var progressIndicator = new Progress<int>(percent =>
            {
                PbStatus.Value = percent;
                TxtStatus.Text = $"Merging... {percent}%";
            });

            TxtStatus.Text = "Merging started...";
            _logger.Log(LogLevel.INFO, $"Starting Merge to: {fullOutputPath}");

            try
            {
                await Task.Run(() =>
                {
                    _mergeEngine.MergeFiles(inputPaths, fullOutputPath, progressIndicator);
                });

                _logger.Log(LogLevel.INFO, "Merge Completed.");
                PbStatus.Value = 100;
                TxtStatus.Text = "Done!";

                MessageBox.Show($"Success!\nFile saved to:\n{fullOutputPath}",
                                "Merge Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                HandleError($"Merge Failed: {ex.Message}");
            }
            finally
            {
                ResetUI();
            }
        }

        private string GetAutoRenamedPath(string originalPath)
        {
            string directory = Path.GetDirectoryName(originalPath) ?? string.Empty;
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
            string extension = Path.GetExtension(originalPath);

            int counter = 1;
            string newPath = originalPath;

            while (File.Exists(newPath))
            {
                string newName = $"{fileNameWithoutExt}_{counter}{extension}";
                newPath = Path.Combine(directory, newName);
                counter++;
            }

            return newPath;
        }

        private void HandleError(string message)
        {
            _logger.Log(LogLevel.ERROR, message);
            TxtStatus.Text = "Error occurred.";
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ResetUI();
        }

        private void ResetUI()
        {
            BtnMerge.IsEnabled = true;
            BtnMerge.Content = "MERGE FILES";
        }
    }
}