using SDUI.Controls;

namespace SDUI.Validations;

public abstract class ValidationRule
{
    public string ErrorMessage { get; set; }
    public abstract bool Validate(UIElementBase element, out string errorMessage);
}
