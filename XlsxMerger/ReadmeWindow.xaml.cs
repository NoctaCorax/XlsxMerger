// ReadmeWindow.xaml.cs
using System;
using System.Collections.Generic; // For standard lists
using System.Windows;
using System.Windows.Documents; // For FlowDocument elements (Paragraph, Run, etc.)
using System.Windows.Media;

namespace XlsxMerger
{
    public partial class ReadmeWindow : Window
    {
        public ReadmeWindow(string content)
        {
            InitializeComponent();
            RenderMarkdown(content);
        }

        /// <summary>
        /// A simple internal parser to convert Markdown text into WPF FlowDocument blocks.
        /// Handles Headers (#), Lists (*, -), Blockquotes (>), and basic Bolding (**).
        /// </summary>
        private void RenderMarkdown(string markdown)
        {
            // Clear existing content (if any)
            MainDocument.Blocks.Clear();

            string[] lines = markdown.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // FIX: Added '?' to make the type nullable, resolving "Converting null literal..." warning.
            // Use full type name to avoid conflict between System.Collections.Generic.List and System.Windows.Documents.List
            System.Windows.Documents.List? currentList = null;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                // 1. Handle Lists (* or -)
                if (trimmed.StartsWith("* ") || trimmed.StartsWith("- "))
                {
                    if (currentList == null)
                    {
                        currentList = new System.Windows.Documents.List();
                        currentList.MarkerStyle = TextMarkerStyle.Disc;
                        MainDocument.Blocks.Add(currentList);
                    }

                    // Remove the bullet char and parse the rest
                    string itemText = trimmed.Substring(2);
                    var listItem = new ListItem(CreateParagraphWithFormatting(itemText));
                    currentList.ListItems.Add(listItem);
                    continue;
                }
                else
                {
                    // Reset list buffer if line is not a list item
                    currentList = null;
                }

                // 2. Handle Headers (#)
                if (trimmed.StartsWith("# "))
                {
                    var p = CreateParagraphWithFormatting(trimmed.Substring(2));
                    p.FontSize = 24;
                    p.FontWeight = FontWeights.Bold;
                    p.Foreground = Brushes.DarkBlue;
                    p.Margin = new Thickness(0, 20, 0, 10);
                    MainDocument.Blocks.Add(p);
                }
                else if (trimmed.StartsWith("## "))
                {
                    var p = CreateParagraphWithFormatting(trimmed.Substring(3));
                    p.FontSize = 18;
                    p.FontWeight = FontWeights.Bold;
                    p.Foreground = Brushes.Teal;
                    p.Margin = new Thickness(0, 15, 0, 5);
                    MainDocument.Blocks.Add(p);
                }
                // 3. Handle Blockquotes (>)
                else if (trimmed.StartsWith("> "))
                {
                    var p = CreateParagraphWithFormatting(trimmed.Substring(2));
                    p.FontStyle = FontStyles.Italic;
                    p.Foreground = Brushes.Gray;
                    p.BorderThickness = new Thickness(4, 0, 0, 0);
                    p.BorderBrush = Brushes.LightGray;
                    p.Padding = new Thickness(10, 0, 0, 0);
                    MainDocument.Blocks.Add(p);
                }
                // 4. Handle Tables/Code (Basic detection for | or `)
                else if (trimmed.StartsWith("|") || trimmed.Contains("`") || trimmed.StartsWith("Use"))
                {
                    // For tables or code, we use Monospace font to keep alignment
                    var p = new Paragraph(new Run(line));
                    p.FontFamily = new FontFamily("Consolas, Courier New");
                    p.Background = Brushes.WhiteSmoke;
                    MainDocument.Blocks.Add(p);
                }
                // 5. Standard Paragraph
                else if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    var p = CreateParagraphWithFormatting(line);
                    p.Margin = new Thickness(0, 0, 0, 10);
                    MainDocument.Blocks.Add(p);
                }
            }
        }

        /// <summary>
        /// Parses a single line for bold text (**text**) and returns a Paragraph.
        /// </summary>
        private Paragraph CreateParagraphWithFormatting(string text)
        {
            Paragraph p = new Paragraph();

            // Split by double asterisks
            string[] parts = text.Split(new[] { "**" }, StringSplitOptions.None);

            for (int i = 0; i < parts.Length; i++)
            {
                Run run = new Run(parts[i]);

                // Odd index means it was inside ** **, so make it Bold
                if (i % 2 == 1)
                {
                    run.FontWeight = FontWeights.Bold;
                }

                p.Inlines.Add(run);
            }

            return p;
        }
    }
}