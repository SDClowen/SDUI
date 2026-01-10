using SDUI.Controls;

namespace SDUI.Validations;

public class RequiredFieldValidationRule : ValidationRule
{
    public override bool Validate(UIElementBase element, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(element.Text))
        {
            errorMessage = ErrorMessage ?? "Bu alan boş bırakılamaz.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}