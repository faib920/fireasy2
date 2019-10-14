// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Fireasy.Common.Extensions;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// <see cref="Expression"/> 的 json 读取器。
    /// </summary>
    public class ExpressionJsonReader
    {
        private JsonSerializer serializer;
        private string json;

        /// <summary>
        /// 获取参数名称与类型的字典。
        /// </summary>
        protected readonly Dictionary<string, Type> TypeDictionary = new Dictionary<string,Type>();
        
        /// <summary>
        /// 获取参数类型与表达式的字典。
        /// </summary>
        protected readonly Dictionary<Type, ParameterExpression> ParameterDictionary = new Dictionary<Type, ParameterExpression>();

        /// <summary>
        /// 初始化 <see cref="ExpressionJsonReader"/> 类的新实例。
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="json"></param>
        public ExpressionJsonReader(JsonSerializer serializer, string json)
        {
            this.serializer = serializer;
            this.json = json;
        }

        /// <summary>
        /// 获取 <see cref="JsonReader"/> 对象。
        /// </summary>
        protected JsonReader JsonReader { get; private set; }

        /// <summary>
        /// 通过对 json 文件的解析，获取 <see cref="Expression"/> 对象。
        /// </summary>
        /// <returns></returns>
        public Expression GetExpression()
        {
            using (var sr = new StringReader(json))
            using (JsonReader = new JsonReader(sr))
            {
                return ReadSegment();
            }
        }

        /// <summary>
        /// 分段读取块。
        /// </summary>
        /// <returns></returns>
        private Expression ReadSegment()
        {
            JsonReader.SkipWhiteSpaces();
            if (JsonReader.IsNull())
            {
                return null;
            }

            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);

            JsonReader.SkipWhiteSpaces();
            var key = JsonReader.ReadKey();
            var nodeType = (ExpressionType)Enum.Parse(typeof(ExpressionType), key);

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();

            var expression = ReadExpression(nodeType);

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return expression;
        }

        /// <summary>
        /// 根据 <paramref name="nodeType"/> 分段读取对应的 <see cref="Expression"/>。
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        protected virtual Expression ReadExpression(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Lambda:
                    return ReadLambdaExpression();
                case ExpressionType.Parameter:
                    return ReadParameterExpression();
                case ExpressionType.Constant:
                    return ReadConstantExpression();
                case ExpressionType.MemberAccess:
                    return ReadMemberExpression();
                case ExpressionType.Call:
                    return ReadMethodCallExpression();
                case ExpressionType.New:
                    return ReadNewExpression();
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return ReadNewArrayExpression();
                case ExpressionType.MemberInit:
                    return ReadMemberInitExpression();
                case ExpressionType.Conditional:
                    return ReadConditionalExpression();
                case ExpressionType.ListInit:
                    return ReadListInitExpression();
                case ExpressionType.TypeIs:
                    return ReadTypeBinaryExpression(nodeType);
                case ExpressionType.Invoke:
                    return ReadInvocationExpression();
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    return ReadUnaryExpression(nodeType);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                    return ReadBinaryExpression(nodeType);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 读取 <see cref="LambdaExpression"/> 块。
        /// </summary>
        /// <returns></returns>
        protected virtual LambdaExpression ReadLambdaExpression()
        {
            var parameters = new List<ParameterExpression>();

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Parameters
            CheckKey("Parameters", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();

            JsonReader.LoopReadArray(r =>
                {
                    var parameter = ReadSegment();
                    if (parameter != null)
                    {
                        parameters.Add((ParameterExpression)parameter);
                    }
                });

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Body
            JsonReader.SkipWhiteSpaces();
            CheckKey("Body", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var body = ReadSegment();
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeLambdaExpression(body, parameters);
        }

        /// <summary>
        /// 构造 <see cref="LambdaExpression"/> 对象。
        /// </summary>
        /// <param name="body">表达式的声明部份。</param>
        /// <param name="parameters">参数表达式列表。</param>
        /// <returns></returns>
        protected virtual LambdaExpression MakeLambdaExpression(Expression body, List<ParameterExpression> parameters)
        {
            return Expression.Lambda(body, parameters.ToArray());
        }

        /// <summary>
        /// 读取 <see cref="ParameterExpression"/> 块。
        /// </summary>
        /// <returns></returns>
        protected virtual ParameterExpression ReadParameterExpression()
        {
            Type type = null;

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Name
            CheckKey("Name", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var name = JsonReader.ReadAsString();
            JsonReader.SkipWhiteSpaces();

            if (JsonReader.Peek() != JsonTokens.EndObjectLiteralCharacter)
            {
                //Type
                JsonReader.ReadKey();
                JsonReader.SkipWhiteSpaces();

                JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                JsonReader.SkipWhiteSpaces();
                type = ResolveType(JsonReader.ReadAsString());
                JsonReader.SkipWhiteSpaces();

                if (!TypeDictionary.ContainsKey(name))
                {
                    TypeDictionary.Add(name, type);
                }
            }
            else
            {
                type = TypeDictionary[name];
            }

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeParameterExpression(type, name);
        }

        /// <summary>
        /// 构造 <see cref="ParameterExpression"/> 对象。
        /// </summary>
        /// <param name="type">参数的类型。</param>
        /// <param name="name">参数的名称。</param>
        /// <returns></returns>
        protected virtual ParameterExpression MakeParameterExpression(Type type, string name)
        {
            ParameterExpression parameterExp;
            if (!ParameterDictionary.TryGetValue(type, out parameterExp))
            {
                parameterExp = Expression.Parameter(type, name);
                ParameterDictionary.Add(type, parameterExp);
            }

            return parameterExp;
        }

        /// <summary>
        /// 读取 <see cref="ConstantExpression"/> 块。
        /// </summary>
        /// <returns></returns>
        protected virtual ConstantExpression ReadConstantExpression()
        {
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Type
            CheckKey("Type", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var type = ResolveType(JsonReader.ReadAsString());
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Value
            CheckKey("Value", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var value = serializer.Deserialize(JsonReader.ReadRaw(), type);
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeConstantExpression(type, value);
        }

        /// <summary>
        /// 构造 <see cref="ConstantExpression"/> 对象。
        /// </summary>
        /// <param name="type">变量的类型。</param>
        /// <param name="value">变量的值。</param>
        /// <returns></returns>
        protected virtual ConstantExpression MakeConstantExpression(Type type, object value)
        {
            return Expression.Constant(value.ToType(type), type);
        }

        /// <summary>
        /// 读取 <see cref="MethodCallExpression"/> 块。
        /// </summary>
        /// <returns></returns>
        protected virtual MethodCallExpression ReadMethodCallExpression()
        {
            var arguments = new List<Expression>();
            var genericArgTypes = new List<Type>();
            var parameterTypes = new List<Type>();

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Type
            CheckKey("Type", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var type = ResolveType(JsonReader.ReadAsString());
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Method
            CheckKey("Method", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var name = JsonReader.ReadAsString();
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Object
            CheckKey("Object", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var instance = ReadSegment();
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Arguments
            CheckKey("Arguments", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();

            JsonReader.LoopReadArray(r =>
                {
                    var argument = ReadSegment();
                    if (argument != null)
                    {
                        arguments.Add(argument);
                    }
                });

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //GenericArgTypes
            CheckKey("GenericArgTypes", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();

            JsonReader.LoopReadArray(r =>
                {
                    var genericArgType = ResolveType(r.ReadAsString());
                    if (genericArgType != null)
                    {
                        genericArgTypes.Add(genericArgType);
                    }
                });

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //ParameterTypes
            CheckKey("ParameterTypes", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();

            JsonReader.LoopReadArray(r =>
                {
                    var parameterType = ResolveType(r.ReadAsString());
                    if (parameterType != null)
                    {
                        parameterTypes.Add(parameterType);
                    }
                });

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeMethodCallExpression(type, name, parameterTypes, genericArgTypes, instance, arguments);
        }

        /// <summary>
        /// 构造 <see cref="MethodCallExpression"/> 对象。
        /// </summary>
        /// <param name="declaringType">方法的声明类型。</param>
        /// <param name="name">方法的名称。</param>
        /// <param name="parameterTypes">方法的参数类型列表。</param>
        /// <param name="genericArgTypes">方法的泛型参数类型列表。</param>
        /// <param name="instance">方法所属对象的表达式。</param>
        /// <param name="arguments">方法的调用参数列表。</param>
        /// <returns></returns>
        protected virtual MethodCallExpression MakeMethodCallExpression(Type declaringType, string name, List<Type> parameterTypes, List<Type> genericArgTypes, Expression instance, List<Expression> arguments)
        {
            var method = ResolveMethod(declaringType, name, parameterTypes.ToArray(), genericArgTypes.ToArray());
            return Expression.Call(instance, method, arguments.ToArray());
        }

        /// <summary>
        /// 读取 <see cref="MemberExpression"/> 块。
        /// </summary>
        /// <returns></returns>
        protected virtual MemberExpression ReadMemberExpression()
        {
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Type
            CheckKey("Type", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var type = ResolveType(JsonReader.ReadAsString());
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Member
            CheckKey("Member", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var name = JsonReader.ReadAsString();
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Expression
            CheckKey("Expression", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var expression = ReadSegment();
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeMemberExpression(type, name, expression);
        }

        /// <summary>
        /// 构造 <see cref="MemberExpression"/> 对象。
        /// </summary>
        /// <param name="declaringType">成员的声明类型。</param>
        /// <param name="name">成员的名称。</param>
        /// <param name="expression">成员所属对象的表达式。</param>
        /// <returns></returns>
        protected virtual MemberExpression MakeMemberExpression(Type declaringType, string name, Expression expression)
        {
            var member = declaringType.GetMember(name, BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
            return Expression.MakeMemberAccess(expression, member);
        }

        /// <summary>
        /// 读取 <see cref="BinaryExpression"/> 块。
        /// </summary>
        /// <returns></returns>
        protected virtual BinaryExpression ReadBinaryExpression(ExpressionType nodeType)
        {
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Left
            CheckKey("Left", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var left = ReadSegment();
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Right
            CheckKey("Right", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var right = ReadSegment();
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeBinaryExpression(nodeType, left, right);
        }

        /// <summary>
        /// 构造 <see cref="BinaryExpression"/> 对象。
        /// </summary>
        /// <param name="nodeType">节点类别。</param>
        /// <param name="left">左操作表达式。</param>
        /// <param name="right">右操作表达式。</param>
        /// <returns></returns>
        protected virtual BinaryExpression MakeBinaryExpression(ExpressionType nodeType, Expression left, Expression right)
        {
            return Expression.MakeBinary(nodeType, left, right);
        }

        /// <summary>
        /// 读取 <see cref="UnaryExpression"/> 块。
        /// </summary>
        /// <returns></returns>
        protected virtual UnaryExpression ReadUnaryExpression(ExpressionType nodeType)
        {
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();
            
            //Type
            CheckKey("Type", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var type = ResolveType(JsonReader.ReadAsString());
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Operand
            CheckKey("Operand", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var operand = ReadSegment();
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeUnaryExpression(nodeType, operand, type);
        }

        /// <summary>
        /// 构造 <see cref="UnaryExpression"/> 对象。
        /// </summary>
        /// <param name="nodeType">节点类别。</param>
        /// <param name="operand">操作数表达式。</param>
        /// <param name="type">目标类型。</param>
        /// <returns></returns>
        protected virtual UnaryExpression MakeUnaryExpression(ExpressionType nodeType, Expression operand, Type type)
        {
            return Expression.MakeUnary(nodeType, operand, type);
        }

        protected virtual NewExpression ReadNewExpression()
        {
            return null;
        }

        protected virtual NewArrayExpression ReadNewArrayExpression()
        {
            return null;
        }

        protected virtual MemberInitExpression ReadMemberInitExpression()
        {
            return null;
        }

        /// <summary>
        /// 读取 <see cref="ConditionalExpression"/> 对象。
        /// </summary>
        /// <returns></returns>
        protected virtual ConditionalExpression ReadConditionalExpression()
        {
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Type
            CheckKey("Type", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var type = ResolveType(JsonReader.ReadAsString());
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //True
            CheckKey("True", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var ifTrue = ReadSegment();
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //False
            CheckKey("False", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var ifFalse = ReadSegment();
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Test
            CheckKey("Test", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var test = ReadSegment();
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeConditionalExpression(test, ifTrue, ifFalse, type);
        }

        /// <summary>
        /// 构造 <see cref="ConditionalExpression"/> 对象。
        /// </summary>
        /// <param name="test">测试表达式。</param>
        /// <param name="ifTrue">为真的表达式。</param>
        /// <param name="ifFalse">为假的表达式。</param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual ConditionalExpression MakeConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse, Type type)
        {
            return Expression.Condition(test, ifTrue, ifFalse, type);
        }

        /// <summary>
        /// 读取 <see cref="ListInitExpression"/> 对象。
        /// </summary>
        /// <returns></returns>
        protected virtual ListInitExpression ReadListInitExpression()
        {
            return null;
        }

        /// <summary>
        /// 读取 <see cref="TypeBinaryExpression"/> 对象。
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        protected virtual TypeBinaryExpression ReadTypeBinaryExpression(ExpressionType nodeType)
        {
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Type
            CheckKey("Type", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var type = ResolveType(JsonReader.ReadAsString());
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();

            //Expression
            CheckKey("Expression", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var expression = ReadSegment();
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeTypeBinaryExpression(nodeType, expression, type);
        }

        /// <summary>
        /// 构造 <see cref="TypeBinaryExpression"/> 对象。
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="expression"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual TypeBinaryExpression MakeTypeBinaryExpression(ExpressionType nodeType, Expression expression, Type type)
        {
            if (nodeType == ExpressionType.TypeIs)
            {
                return Expression.TypeIs(expression, type);
            }

            return Expression.TypeEqual(expression, type);
        }

        /// <summary>
        /// 读取 <see cref="InvocationExpression"/> 对象。
        /// </summary>
        /// <returns></returns>
        protected virtual InvocationExpression ReadInvocationExpression()
        {
            var arguments = new List<Expression>();

            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            JsonReader.SkipWhiteSpaces();

            //Expression
            CheckKey("Expression", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();
            var expression = ReadSegment();
            JsonReader.SkipWhiteSpaces();
            JsonReader.AssertAndConsume(JsonTokens.ElementSeparator);
            JsonReader.SkipWhiteSpaces();
            
            //Arguments
            CheckKey("Arguments", JsonReader.ReadKey());
            JsonReader.SkipWhiteSpaces();

            JsonReader.AssertAndConsume(JsonTokens.PairSeparator);
            JsonReader.SkipWhiteSpaces();

            JsonReader.LoopReadArray(r =>
                {
                    var argument = ReadSegment();
                    if (argument != null)
                    {
                        arguments.Add(argument);
                    }
                });

            JsonReader.AssertAndConsume(JsonTokens.EndObjectLiteralCharacter);

            return MakeInvocationExpression(expression, arguments);
        }

        /// <summary>
        /// 构造 <see cref="InvocationExpression"/> 对象。
        /// </summary>
        /// <param name="expression">调用的表达式。</param>
        /// <param name="arguments">调用的参数列表。</param>
        /// <returns></returns>
        protected virtual InvocationExpression MakeInvocationExpression(Expression expression, List<Expression> arguments)
        {
            return Expression.Invoke(expression, arguments);
        }

        /// <summary>
        /// 根据字符串解析出类型。
        /// </summary>
        /// <param name="typeName">表示类型的字符串。</param>
        /// <returns></returns>
        protected virtual Type ResolveType(string typeName)
        {
            return Type.GetType(typeName);
        }

        /// <summary>
        /// 检查值是否为所期望的值。
        /// </summary>
        /// <param name="expected">期望的值。</param>
        /// <param name="actual">实际的值。</param>
        /// <returns></returns>
        private string CheckKey(string expected, string actual)
        {
            if (expected != actual)
            {
                throw new ArgumentOutOfRangeException(expected);
            }

            return actual;
        }

        /// <summary>
        /// 解析方法。
        /// </summary>
        /// <param name="declaringType">方法的声明类型。</param>
        /// <param name="name">方法的名称。</param>
        /// <param name="parameterTypes">方法的参数类型列表。</param>
        /// <param name="genArgTypes">方法的泛型参数类型列表。</param>
        /// <returns></returns>
        protected virtual MethodInfo ResolveMethod(Type declaringType, string name, Type[] parameterTypes, Type[] genArgTypes)
        {
            var methods = from mi in declaringType.GetMethods() where mi.Name == name select mi;
            foreach (var method in methods)
            {
                try
                {
                    var realMethod = method;
                    if (method.IsGenericMethod)
                    {
                        realMethod = method.MakeGenericMethod(genArgTypes);
                    }

                    var methodParameterTypes = realMethod.GetParameters().Select(p => p.ParameterType);
                    if (MatchPiecewise(parameterTypes, methodParameterTypes))
                    {
                        return realMethod;
                    }
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }

            return null;
        }

        private bool MatchPiecewise(IEnumerable<Type> first, IEnumerable<Type> second)
        {
            var firstArray = first.ToArray();
            var secondArray = second.ToArray();

            if (firstArray.Length != secondArray.Length)
            {
                return false;
            }

            for (var i = 0; i < firstArray.Length; i++)
            {
                if (secondArray[i].IsAssignableFrom(firstArray[i]) ||
                    firstArray[i].Equals(secondArray[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
