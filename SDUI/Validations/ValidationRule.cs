using SDUI.Controls;

namespace SDUI.Validations;

public abstract class ValidationRule
{
    public string ErrorMessage { get; set; }
    public abstract bool Validate(ElementBase element, out string errorMessage);
}