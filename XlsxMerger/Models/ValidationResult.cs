// Models/ValidationResult.cs
namespace XlsxMerger.Models
{
    /// <summary>
    /// Represents the result of a validation check.
    /// Renamed to AppValidationResult to avoid conflict with System.Windows.Controls.ValidationResult.
    /// </summary>
    public class AppValidationResult
    {
        public bool IsValid { get; set; }

        // FIX: Inicjalizujemy pustym ciągiem znaków, aby uniknąć ostrzeżenia CS8618
        public string Message { get; set; } = string.Empty;

        // Helper method to create a Success result quickly
        public static AppValidationResult Success()
        {
            return new AppValidationResult { IsValid = true, Message = "Validation Passed" };
        }

        // Helper method to create an Error result quickly
        public static AppValidationResult Error(string message)
        {
            return new AppValidationResult { IsValid = false, Message = message };
        }
    }
}