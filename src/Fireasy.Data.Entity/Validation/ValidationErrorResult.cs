// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

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
