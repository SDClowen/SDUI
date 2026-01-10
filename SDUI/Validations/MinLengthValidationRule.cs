using SDUI.Controls;

namespace SDUI.Validations;

public class MinLengthValidationRule : ValidationRule
{
    public int MinLength { get; set; }

    public override bool Validate(UIElementBase element, out string errorMessage)
    {
        if (element.Text.Length < MinLength)
        {
            errorMessage = ErrorMessage ?? $"Bu alan en az {MinLength} karakter olmalıdır.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}