using Fireasy.Common;
using Fireasy.Data.Syntax;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
#if !NET35
using System.ComponentModel.DataAnnotations;
#endif

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 表示属性在库里应该保证唯一。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class UniqueCodeAttribute : ValidationAttribute, IPropertyMarker
    {
        /// <summary>
        /// 初始化 <see cref="UniqueCodeAttribute"/> 类的新实例。
        /// </summary>
        public UniqueCodeAttribute()
            : base (SR.GetString(SRKind.ValidationUniqueCode))
        {
        }

        /// <summary>
        /// 获取或设置是否允许置空。
        /// </summary>
        public bool AllowNull { get; set; }

        /// <summary>
        /// 获取或设置保证唯一性的属性。
        /// </summary>
        public IProperty Property { get; set; }

        /// <summary>
        /// 检查指定的值对于当前的验证特性是否有效。
        /// </summary>
        /// <param name="value">属性值。</param>
        /// <param name="validationContext">验证上下文对象。</param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Guard.ArgumentNull(validationContext, "validationContext");

            var entityType = validationContext.ObjectType;

            if (ValidateNullable(value) == ValidationResult.Success)
            {
                return ValidationResult.Success;
            }

            IDatabase database;
            try
            {
                database = DatabaseFactory.GetDatabaseFromScope();
            }
            catch (UnableGetDatabaseScopeException exp)
            {
                return new ValidationResult(exp.Message);
            }

            var paramaters = new ParameterCollection();
            var entity = validationContext.ObjectInstance as IEntity;
            var syntax = database.Provider.GetService<ISyntaxProvider>();

            //var eqb = new EntityQueryBuilder(syntax, null, entityType, paramaters);
            //eqb = eqb.Select().Count().From().Where(Property, value);

            //if (entity.EntityState != EntityState.Attached)
            //{
            //    foreach (var pk in PropertyUnity.GetPrimaryProperties(entityType))
            //    {
            //        eqb = eqb.And(pk, entity.GetValue(pk.Name), QueryOperator.UnEquals);
            //    }
            //}

            //var result = database.ExecuteScalar<int>(eqb.ToSqlCommand(), paramaters);
            //return result == 0 ? ValidationResult.Success : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

            return ValidationResult.Success;
        }

        private ValidationResult ValidateNullable(object value)
        {
            if (AllowNull)
            {
                if (value == null)
                {
                    return ValidationResult.Success;
                }

                if (value is PropertyValue pvalue && PropertyValue.IsEmpty(pvalue))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(string.Empty);
        }
    }
}
