// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Linq.Expressions;
using Fireasy.Data.Entity.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Converts user arguments into named-value parameters
    /// </summary>
    public class Parameterizer : DbExpressionVisitor
    {
        private readonly Dictionary<TypeAndValue, NamedValueExpression> _tmap = new Dictionary<TypeAndValue, NamedValueExpression>();
        private readonly Dictionary<HashedExpression, NamedValueExpression> _hmap = new Dictionary<HashedExpression, NamedValueExpression>();
        private int _param = 0;
        private bool _isLikeMethod = false;

        public static Expression Parameterize(Expression expression)
        {
            return new Parameterizer().Visit(expression);
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            // don't parameterize the projector or aggregator!
            SelectExpression select = (SelectExpression)Visit(proj.Select);
            return proj.Update(select, proj.Projector, proj.Aggregator);
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            if (u.NodeType == ExpressionType.Convert && u.Operand.NodeType == ExpressionType.ArrayIndex)
            {
                var b = (BinaryExpression)u.Operand;

                if (IsConstantOrParameter(b.Left) && IsConstantOrParameter(b.Right))
                {
                    return GetNamedValue(u);
                }
            }

            return base.VisitUnary(u);
        }

        private static bool IsConstantOrParameter(Expression e)
        {
            return e != null && e.NodeType == ExpressionType.Constant || e.NodeType == ExpressionType.Parameter;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            var left = Visit(b.Left);
            var right = Visit(b.Right);

            if (left is NamedValueExpression nv0 && right is ColumnExpression c0)
            {
                left = QueryUtility.GetNamedValueExpression(nv0.Name, nv0.Value, (DbType)c0.MapInfo.DataType);
            }
            else if (b.Right is NamedValueExpression nv1 && b.Left is ColumnExpression c1)
            {
                right = QueryUtility.GetNamedValueExpression(nv1.Name, nv1.Value, (DbType)c1.MapInfo.DataType);
            }

            return b.Update(left, b.Conversion, right);
        }

        protected override ColumnAssignment VisitColumnAssignment(ColumnAssignment ca)
        {
            ca = base.VisitColumnAssignment(ca);
            var expression = ca.Expression;

            if (expression is NamedValueExpression nv)
            {
                expression = QueryUtility.GetNamedValueExpression(nv.Name, nv.Value, (DbType)ca.Column.MapInfo.DataType);
            }

            return UpdateColumnAssignment(ca, ca.Column, expression);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value != null && !IsNumeric(c.Value.GetType()))
            {
                var tv = new TypeAndValue(c.Type, _isLikeMethod, c.Value);
                if (!_tmap.TryGetValue(tv, out NamedValueExpression nv))
                {
                    var useEscape = false;
                    //if like method then use escape '/'
                    if (_isLikeMethod && c.Value is string str && Regex.IsMatch(str, "(%|_)"))
                    {
                        str = Regex.Replace(str, "(%|_)", m => "/" + m.Value);
                        c = Expression.Constant(str);
                        useEscape = true;
                    }

                    // re-use same name-value if same type & value
                    var name = string.Intern("p" + _param++);
                    nv = new NamedValueExpression(name, c, useEscape);
                    _tmap.Add(tv, nv);
                }

                return nv;
            }

            return c;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            return GetNamedValue(p);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExp)
        {
            if (methodCallExp.Method.DeclaringType == typeof(ExecutionBuilder))
            {
                return GetNamedValue(methodCallExp);
            }

            _isLikeMethod = IsLikeMethod(methodCallExp);

            var arguments = Visit(methodCallExp.Arguments);

            _isLikeMethod = false;

            var obj = Visit(methodCallExp.Object);

            return methodCallExp.Update(obj, arguments);
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            memberExp = (MemberExpression)base.VisitMember(memberExp);
            if (memberExp.Expression is NamedValueExpression nv)
            {
                Expression x = Expression.MakeMemberAccess(nv.Value, memberExp.Member);
                return GetNamedValue(x);
            }

            return memberExp;
        }

        private Expression GetNamedValue(Expression e)
        {
            var he = new HashedExpression(e);
            if (!_hmap.TryGetValue(he, out NamedValueExpression nv))
            {
                var name = "p" + (_param++);
                nv = new NamedValueExpression(name, e);
                _hmap.Add(he, nv);
            }

            return nv;
        }

        private bool IsNumeric(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsLikeMethod(MethodCallExpression methodCallExp)
        {
            if (methodCallExp.Method.DeclaringType == typeof(string))
            {
                switch (methodCallExp.Method.Name)
                {
                    case nameof(string.StartsWith):
                    case nameof(string.EndsWith):
                    case nameof(string.Contains):
                        return true;
                }
            }

            return false;
        }

        private struct TypeAndValue : IEquatable<TypeAndValue>
        {
            private readonly Type _type;
            private readonly object _value;
            private readonly int _hash;
            private readonly bool _isLike;

            public TypeAndValue(Type type, bool isLike, object value)
            {
                _type = type;
                _value = value;
                _isLike = isLike;
                _hash = type.GetHashCode() + (value != null ? value.GetHashCode() : 0) + (_isLike ? 1 : 0);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TypeAndValue))
                {
                    return false;
                }

                return Equals((TypeAndValue)obj);
            }

            public bool Equals(TypeAndValue vt)
            {
                return vt._type == _type && Equals(vt._value, _value) && _isLike == vt._isLike;
            }

            public override int GetHashCode()
            {
                return _hash;
            }
        }

        private struct HashedExpression : IEquatable<HashedExpression>
        {
            private readonly Expression _expression;
            private readonly int _hashCode;

            public HashedExpression(Expression expression)
            {
                _expression = expression;
                _hashCode = Hasher.ComputeHash(expression);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is HashedExpression))
                {
                    return false;
                }

                return Equals((HashedExpression)obj);
            }

            public bool Equals(HashedExpression other)
            {
                return _hashCode == other._hashCode &&
                    DbExpressionComparer.AreEqual(_expression, other._expression);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            private class Hasher : DbExpressionVisitor
            {
                private int _hc;

                internal static int ComputeHash(Expression expression)
                {
                    var hasher = new Hasher();
                    hasher.Visit(expression);
                    return hasher._hc;
                }

                protected override Expression VisitConstant(ConstantExpression c)
                {
                    _hc += ((c.Value != null) ? c.Value.GetHashCode() : 0);
                    return c;
                }
            }
        }
    }
}