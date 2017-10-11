#if NET35
using System;

namespace Fireasy.Data.Entity.Validation
{
    public abstract class ValidationAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        private bool _hasBaseIsValid = false;
 
        protected ValidationAttribute(string errorMessage)
            : base(errorMessage)
        {
        }

        public ValidationResult GetValidationResult(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException("validationContext");
            }
            ValidationResult result = this.IsValid(value, validationContext);
            if ((result != null) && !((result != null) ? !string.IsNullOrEmpty(result.ErrorMessage) : false))
            {
                result = new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName), result.MemberNames);
            }
            return result;
        }

        protected virtual ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (this._hasBaseIsValid)
            {
                throw new NotImplementedException();
            }
            ValidationResult success = ValidationResult.Success;
            if (!this.IsValid(value))
            {
                string[] memberNames = (validationContext.MemberName != null) ? new string[] { validationContext.MemberName } : null;
                success = new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName), memberNames);
            }
            return success;
        }

        public override bool IsValid(object value)
        {
            if (!this._hasBaseIsValid)
            {
                this._hasBaseIsValid = true;
            }
            return (this.IsValid(value, null) == null);
        }
    }
}
#endif