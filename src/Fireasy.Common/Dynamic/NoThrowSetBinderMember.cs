// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Dynamic;

namespace Fireasy.Common.Dynamic
{
    /// <summary>
    /// 在动态设置成员操作时忽略异常。
    /// </summary>
    public class NoThrowSetBinderMember : SetMemberBinder
    {
        private readonly SetMemberBinder innerBinder;

        public NoThrowSetBinderMember(SetMemberBinder innerBinder)
            : base(innerBinder.Name, innerBinder.IgnoreCase)
        {
            this.innerBinder = innerBinder;
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            var retMetaObject = innerBinder.Bind(target, new DynamicMetaObject[] { value });

            var noThrowVisitor = new NoThrowExpressionVisitor();
            var resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

            var finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
            return finalMetaObject;
        }
    }
}
