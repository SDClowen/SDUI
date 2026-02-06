using System.Text.RegularExpressions;
using SDUI.Controls;

namespace SDUI.Validations;

public class RegexValidationRule : ValidationRule
{
    public string Pattern { get; set; }

    public override bool Validate(ElementBase element, out string errorMessage)
    {
        if (!Regex.IsMatch(element.Text, Pattern))
        {
            errorMessage = ErrorMessage ?? "Geçersiz format.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}