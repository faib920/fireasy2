// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Fireasy.Data.Entity.Query
{
    /// <summary>
    /// Lambda 表达式的 Hash 生成器。
    /// </summary>
    public class ExpressionHashGenerator : IExecuteCacheKeyGenerator, IQueryCacheKeyGenerator
    {
        public readonly static ExpressionHashGenerator Instance = new ExpressionHashGenerator();

        public string Generate(Expression expression, params string[] prefix)
        {
            var sb = new StringBuilder();
            foreach (var p in prefix)
            {
                if (!string.IsNullOrEmpty(p))
                {
                    sb.AppendFormat("{0}:", p);
                }
            }

            var evalExp = PartialEvaluator.Eval(expression, TranslateProviderBase.EvaluatedLocallyFunc);

            sb.Append(GetHashCode(evalExp));

            return sb.ToString();
        }

        private static int GetHashCode(Expression obj)
        {
            if (obj == null)
            {
                return 0;
            }

            var nodeType = (int)obj.NodeType;
            nodeType += (nodeType * 397) ^ obj.Type.GetHashCode();
            switch (obj.NodeType)
            {
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    {
                        var unaryExpression = (UnaryExpression)obj;
                        if (unaryExpression.Method != null)
                        {
                            nodeType += (nodeType * 397) ^ unaryExpression.Method.GetHashCode();
                        }
                        nodeType += (nodeType * 397) ^ GetHashCode(unaryExpression.Operand);
                        break;
                    }
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    {
                        var binaryExpression = (BinaryExpression)obj;
                        nodeType += (nodeType * 397) ^ GetHashCode(binaryExpression.Left);
                        nodeType += (nodeType * 397) ^ GetHashCode(binaryExpression.Right);
                        break;
                    }
                case ExpressionType.TypeIs:
                    {
                        var typeBinaryExpression = (TypeBinaryExpression)obj;
                        nodeType += (nodeType * 397) ^ GetHashCode(typeBinaryExpression.Expression);
                        nodeType += (nodeType * 397) ^ typeBinaryExpression.TypeOperand.GetHashCode();
                        break;
                    }
                case ExpressionType.Constant:
                    {
                        var constantExpression = (ConstantExpression)obj;
                        if (constantExpression.Value != null)
                        {
                            if (constantExpression.Value is IQueryable queryable)
                            {
                                nodeType += (nodeType * 397) ^ GetHashCode(queryable.Expression);
                            }
                            else
                            {
                                nodeType += (nodeType * 397) ^ constantExpression.Value.GetHashCode();
                            }
                        }
                        break;
                    }
                case ExpressionType.Parameter:
                    {
                        var parameterExpression = (ParameterExpression)obj;
                        nodeType += nodeType * 397;
                        if (parameterExpression.Name != null)
                        {
                            nodeType ^= parameterExpression.Name.GetHashCode();
                        }
                        break;
                    }
                case ExpressionType.MemberAccess:
                    {
                        var memberExpression = (MemberExpression)obj;
                        nodeType += (nodeType * 397) ^ memberExpression.Member.GetHashCode();
                        nodeType += (nodeType * 397) ^ GetHashCode(memberExpression.Expression);
                        break;
                    }
                case ExpressionType.Call:
                    {
                        var methodCallExpression = (MethodCallExpression)obj;
                        nodeType += (nodeType * 397) ^ methodCallExpression.Method.GetHashCode();
                        nodeType += (nodeType * 397) ^ GetHashCode(methodCallExpression.Object);
                        nodeType += (nodeType * 397) ^ GetHashCode(methodCallExpression.Arguments);
                        break;
                    }
                case ExpressionType.Lambda:
                    {
                        var lambdaExpression = (LambdaExpression)obj;
                        nodeType += (nodeType * 397) ^ lambdaExpression.ReturnType.GetHashCode();
                        nodeType += (nodeType * 397) ^ GetHashCode(lambdaExpression.Body);
                        nodeType += (nodeType * 397) ^ GetHashCode(lambdaExpression.Parameters);
                        break;
                    }
                case ExpressionType.New:
                    {
                        var newExpression = (NewExpression)obj;
                        nodeType += (nodeType * 397) ^ (newExpression.Constructor?.GetHashCode() ?? 0);
                        if (newExpression.Members != null)
                        {
                            for (int l = 0; l < newExpression.Members.Count; l++)
                            {
                                nodeType += (nodeType * 397) ^ newExpression.Members[l].GetHashCode();
                            }
                        }
                        nodeType += (nodeType * 397) ^ GetHashCode(newExpression.Arguments);
                        break;
                    }
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    {
                        var newArrayExpression = (NewArrayExpression)obj;
                        nodeType += (nodeType * 397) ^ GetHashCode(newArrayExpression.Expressions);
                        break;
                    }
                case ExpressionType.Invoke:
                    {
                        var invocationExpression = (InvocationExpression)obj;
                        nodeType += (nodeType * 397) ^ GetHashCode(invocationExpression.Expression);
                        nodeType += (nodeType * 397) ^ GetHashCode(invocationExpression.Arguments);
                        break;
                    }
                case ExpressionType.MemberInit:
                    {
                        var memberInitExpression = (MemberInitExpression)obj;
                        nodeType += (nodeType * 397) ^ GetHashCode(memberInitExpression.NewExpression);
                        for (var j = 0; j < memberInitExpression.Bindings.Count; j++)
                        {
                            var memberBinding = memberInitExpression.Bindings[j];
                            nodeType += (nodeType * 397) ^ memberBinding.Member.GetHashCode();
                            nodeType += (nodeType * 397) ^ (int)memberBinding.BindingType;
                            switch (memberBinding.BindingType)
                            {
                                case MemberBindingType.Assignment:
                                    {
                                        var memberAssignment = (MemberAssignment)memberBinding;
                                        nodeType += (nodeType * 397) ^ GetHashCode(memberAssignment.Expression);
                                        break;
                                    }
                                case MemberBindingType.ListBinding:
                                    {
                                        var memberListBinding = (MemberListBinding)memberBinding;
                                        for (int k = 0; k < memberListBinding.Initializers.Count; k++)
                                        {
                                            nodeType += (nodeType * 397) ^ GetHashCode(memberListBinding.Initializers[k].Arguments);
                                        }
                                        break;
                                    }
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    }
                case ExpressionType.ListInit:
                    {
                        var listInitExpression = (ListInitExpression)obj;
                        nodeType += (nodeType * 397) ^ GetHashCode(listInitExpression.NewExpression);
                        for (int i = 0; i < listInitExpression.Initializers.Count; i++)
                        {
                            nodeType += (nodeType * 397) ^ GetHashCode(listInitExpression.Initializers[i].Arguments);
                        }
                        break;
                    }
                case ExpressionType.Conditional:
                    {
                        var conditionalExpression = (ConditionalExpression)obj;
                        nodeType += (nodeType * 397) ^ GetHashCode(conditionalExpression.Test);
                        nodeType += (nodeType * 397) ^ GetHashCode(conditionalExpression.IfTrue);
                        nodeType += (nodeType * 397) ^ GetHashCode(conditionalExpression.IfFalse);
                        break;
                    }
                case ExpressionType.Default:
                    nodeType += (nodeType * 397) ^ obj.Type.GetHashCode();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return nodeType;
        }

        private static int GetHashCode<T>(IList<T> expressions) where T : Expression
        {
            var num = 0;
            for (int i = 0, n = expressions.Count; i < n; i++)
            {
                num += (num * 397) ^ GetHashCode(expressions[i]);
            }
            return num;
        }
    }
}