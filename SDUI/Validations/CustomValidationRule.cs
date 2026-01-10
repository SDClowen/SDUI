using System;
using SDUI.Controls;

namespace SDUI.Validations;

public class CustomValidationRule : ValidationRule
{
    private readonly Func<UIElementBase, (bool isValid, string errorMessage)> _validationFunc;

    public CustomValidationRule(Func<UIElementBase, (bool isValid, string errorMessage)> validationFunc)
    {
        _validationFunc = validationFunc;
    }

    public override bool Validate(UIElementBase element, out string errorMessage)
    {
        var result = _validationFunc(element);
        errorMessage = result.errorMessage;
        return result.isValid;
    }
}