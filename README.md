# XlsxMerger

> A robust and user-friendly Windows desktop application for merging multiple Excel (.xlsx) files into a single consolidated report.

## Overview

**XlsxMerger** was designed to automate the tedious process of combining data from multiple Excel files. Unlike simple copy-paste scripts, this application includes an intelligent **Schema Validator** that ensures all input files share the same structure before merging, preventing data corruption.

It preserves cell formatting, handles large datasets asynchronously, and offers flexible filename customization.

## Key Features

* **Smart Merging**: Combines multiple `.xlsx` files into one. Headers are taken from the first file, and data is appended from subsequent files.
* **Schema Validation**: Automatically checks if all input files have identical worksheet names and column headers. Blocks the merge if a mismatch is detected.
* **Safety First**: Built-in protection against accidental file overwriting.
    * **Auto-rename**: Automatically appends `_1`, `_2` if a file exists.
    * **Explicit Overwrite**: User must check "Allow overwrite" and confirm via dialog to replace files.
* **Custom Filenames**: Powerful templating system (e.g., `Report_%yyyy-%mo`) with live preview.
* **Settings Persistence**: Remembers your last used filename template.
* **Visual Feedback**: Real-time progress bar and status updates.
* **Diagnostic Logging**: Detailed `debug.log` file for troubleshooting.

## Tech Stack

* **Language**: C#
* **Framework**: .NET 8.0 (WPF)
* **IDE**: Visual Studio 2022
* **Dependencies**:
    * [ClosedXML](https://github.com/ClosedXML/ClosedXML) - For Excel file manipulation without requiring Microsoft Office installed.

## Getting Started

### Prerequisites
* Windows 10 or 11.
* [.NET Desktop Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

### Building from Source
1.  Clone the repository:
    ```bash
    git clone [https://github.com/yourusername/XlsxMerger.git](https://github.com/yourusername/XlsxMerger.git)
    ```
2.  Open `XlsxMerger.sln` in **Visual Studio 2022**.
3.  Restore NuGet packages (Right-click Solution -> Restore NuGet Packages).
4.  Build and Run (Press `F5`).

## Usage Guide

1.  **Select Files**: Click "Select Files..." to choose the Excel spreadsheets you want to merge.
2.  **Choose Output**: Click "Browse..." to select where the result should be saved.
3.  **Configure Name**: Set your desired filename pattern (see below).
4.  **Merge**: Click "MERGE FILES".
    * The app will first **validate** the structure of all files.
    * If validation passes, it will **merge** the data.
    * A progress bar will show the current status.

### Filename Configuration

You can use the following placeholders in the "Template" field:

| Group | Placeholder | Description | Output |
| :--- | :--- | :--- | :--- |
| **Basic** | `%yyyy` | 4-digit Year | `2024` |
| | `%yy` | 2-digit Year | `24` |
| | `%mo` | Month | `05` |
| | `%d` | Day | `15` |
| | `%h` | Hour | `14` |
| | `%mi` | Minute | `30` |
| | `%s` | Second | `45` |
| **Extended** | `%ms` | Milliseconds | `123` |
| | `%q` | Quarter (1-4) | `2` |
| | `%wk` | Week of Year | `22` |
| | `%day` | Day Name | `Friday` |
| **System** | `%user` | Windows Username | `Admin` |
| | `%machine` | Computer Name | `DESKTOP-1` |
| **Context** | `%count` | Number of files | `5` |

**Example:**
Template: `Report_%user_Q%q_%yyyy`
Result: `Report_Admin_Q2_2024.xlsx`


## License

This project is licensed under the MIT License
