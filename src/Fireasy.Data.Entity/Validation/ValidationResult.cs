#if NET35
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Validation
{
    public class ValidationResult
    {
        private string _errorMessage;
        private IEnumerable<string> _memberNames;
        public static readonly ValidationResult Success;

        protected ValidationResult(ValidationResult validationResult)
        {
            if (validationResult == null)
            {
                throw new ArgumentNullException("validationResult");
            }
            this._errorMessage = validationResult._errorMessage;
            this._memberNames = validationResult._memberNames;
        }

        public ValidationResult(string errorMessage)
            : this(errorMessage, null)
        {
        }

        public ValidationResult(string errorMessage, IEnumerable<string> memberNames)
        {
            this._errorMessage = errorMessage;
            this._memberNames = memberNames ?? new string[0];
        }

        public override string ToString()
        {
            return (this.ErrorMessage ?? base.ToString());
        }

        public string ErrorMessage
        {
            get
            {
                return this._errorMessage;
            }
            set
            {
                this._errorMessage = value;
            }
        }

        public IEnumerable<string> MemberNames
        {
            get
            {
                return this._memberNames;
            }
        }
    }
}
#endif