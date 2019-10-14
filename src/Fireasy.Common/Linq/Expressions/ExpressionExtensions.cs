// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Linq.Expressions
{
    internal static class ExpressionExtensions
    {
        internal static InvocationExpression Update(this InvocationExpression invocationExp, Expression expression, IEnumerable<Expression> arguments)
        {
            if (arguments != invocationExp.Arguments || expression != invocationExp.Expression)
            {
                return Expression.Invoke(expression, arguments);
            }
            return invocationExp;
        }

        internal static NewArrayExpression Update(this NewArrayExpression newArrayExp, Type arrayType, IEnumerable<Expression> expressions)
        {
            if (expressions != newArrayExp.Expressions || newArrayExp.Type != arrayType)
            {
                return newArrayExp.NodeType == ExpressionType.NewArrayInit ? Expression.NewArrayInit(arrayType.GetElementType(), expressions) : Expression.NewArrayBounds(arrayType.GetElementType(), expressions);
            }
            return newArrayExp;
        }

        internal static ListInitExpression Update(this ListInitExpression listInitExp, NewExpression newExp, IEnumerable<ElementInit> initializers)
        {
            if (newExp != listInitExp.NewExpression || initializers != listInitExp.Initializers)
            {
                return Expression.ListInit(newExp, initializers);
            }
            return listInitExp;
        }

        internal static MemberInitExpression Update(this MemberInitExpression memberInitExp, NewExpression newExp, IEnumerable<MemberBinding> bindings)
        {
            if (newExp != memberInitExp.NewExpression || bindings != memberInitExp.Bindings)
            {
                return Expression.MemberInit(newExp, bindings);
            }
            return memberInitExp;
        }

        internal static NewExpression Update(this NewExpression newExp, ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members)
        {
            if (arguments != newExp.Arguments || constructor != newExp.Constructor || members != newExp.Members)
            {
                return newExp.Members != null ? Expression.New(constructor, arguments, members) : Expression.New(constructor, arguments);
            }
            return newExp;
        }

        internal static LambdaExpression Update(this LambdaExpression lambdaExp, Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
        {
            if (body != lambdaExp.Body || parameters != lambdaExp.Parameters || delegateType != lambdaExp.Type)
            {
                return Expression.Lambda(delegateType, body, parameters);
            }
            return lambdaExp;
        }

        internal static MemberListBinding Update(this MemberListBinding binding, MemberInfo member, IEnumerable<ElementInit> initializers)
        {
            if (initializers != binding.Initializers || member != binding.Member)
            {
                return Expression.ListBind(member, initializers);
            }
            return binding;
        }

        internal static MemberMemberBinding Update(this MemberMemberBinding binding, MemberInfo member, IEnumerable<MemberBinding> bindings)
        {
            if (bindings != binding.Bindings || member != binding.Member)
            {
                return Expression.MemberBind(member, bindings);
            }
            return binding;
        }

        internal static MemberAssignment Update(this MemberAssignment assignment, MemberInfo member, Expression expression)
        {
            if (expression != assignment.Expression || member != assignment.Member)
            {
                return Expression.Bind(member, expression);
            }
            return assignment;
        }

        internal static MethodCallExpression Update(this MethodCallExpression methodCallExp, Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            if (instance != methodCallExp.Object || method != methodCallExp.Method || arguments != methodCallExp.Arguments)
            {
                try
                {
                    return Expression.Call(instance, method, arguments);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return methodCallExp;
        }

        internal static MemberExpression Update(this MemberExpression memberExp, Expression expression, MemberInfo member)
        {
            if (expression != memberExp.Expression || member != memberExp.Member)
            {
                return Expression.MakeMemberAccess(expression, member);
            }
            return memberExp;
        }

        internal static ConditionalExpression Update(this ConditionalExpression conditionExp, Expression testExp, Expression ifTrueExp, Expression ifFalseExp)
        {
            if (testExp != conditionExp.Test || ifTrueExp != conditionExp.IfTrue || ifFalseExp != conditionExp.IfFalse)
            {
                return Expression.Condition(testExp, ifTrueExp, ifFalseExp);
            }
            return conditionExp;
        }

        internal static TypeBinaryExpression Update(this TypeBinaryExpression typeBinaryExp, Expression expression, Type typeOperand)
        {
            if (expression != typeBinaryExp.Expression || typeOperand != typeBinaryExp.TypeOperand)
            {
                return Expression.TypeIs(expression, typeOperand);
            }
            return typeBinaryExp;
        }

        internal static BinaryExpression Update(this BinaryExpression binaryExp, Expression leftExp, Expression rightExp, Expression converExp, bool isLiftedToNull, MethodInfo method)
        {
            if (leftExp != binaryExp.Left || rightExp != binaryExp.Right || converExp != binaryExp.Conversion || method != binaryExp.Method || isLiftedToNull != binaryExp.IsLiftedToNull)
            {
                return binaryExp.NodeType == ExpressionType.Coalesce && binaryExp.Conversion != null
                           ? Expression.Coalesce(leftExp, rightExp, converExp as LambdaExpression)
                           : Expression.MakeBinary(binaryExp.NodeType, leftExp, rightExp, isLiftedToNull, method);
            }
            return binaryExp;
        }

        internal static UnaryExpression Update(this UnaryExpression unaryExp, Expression operand, Type resultType, MethodInfo method)
        {
            if (unaryExp.Operand != operand || unaryExp.Type != resultType || unaryExp.Method != method)
            {
                return Expression.MakeUnary(unaryExp.NodeType, operand, resultType, method);
            }
            return unaryExp;
        }

    }
}
