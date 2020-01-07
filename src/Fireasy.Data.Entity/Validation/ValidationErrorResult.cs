using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Fireasy.Data.Entity.Validation
{
    public class ValidationErrorResult
    {
        public ValidationErrorResult(ValidationAttribute attribute, string errorMessage)
        {
            Attribute = attribute;
            ErrorMessage = errorMessage;
        }

        public ValidationErrorResult(ValidationAttribute attribute, string displayName, string errorMessage)
        {
            Attribute = attribute;
            DisplayName = displayName;
            ErrorMessage = errorMessage;
        }

        public ValidationAttribute Attribute { get; private set; }

        public string ErrorMessage { get; private set; }

        public string DisplayName { get; private set; }
    }
}
