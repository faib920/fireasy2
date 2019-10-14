// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Converts user arguments into named-value parameters
    /// </summary>
    public class Parameterizer : DbExpressionVisitor
    {
        private Dictionary<TypeAndValue, NamedValueExpression> map = new Dictionary<TypeAndValue, NamedValueExpression>();
        private Dictionary<HashedExpression, NamedValueExpression> pmap = new Dictionary<HashedExpression, NamedValueExpression>();

        public static Expression Parameterize(Expression expression)
        {
            return new Parameterizer().Visit(expression);
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            // don't parameterize the projector or aggregator!
            SelectExpression select = (SelectExpression)this.Visit(proj.Select);
            return proj.Update(select, proj.Projector, proj.Aggregator);
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            if (u.NodeType == ExpressionType.Convert && u.Operand.NodeType == ExpressionType.ArrayIndex)
            {
                var b = (BinaryExpression)u.Operand;
                if (IsConstantOrParameter(b.Left) && IsConstantOrParameter(b.Right))
                {
                    return this.GetNamedValue(u);
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
            Expression left = this.Visit(b.Left);
            Expression right = this.Visit(b.Right);
            if (left.NodeType == (ExpressionType)DbExpressionType.NamedValue
             && right.NodeType == (ExpressionType)DbExpressionType.Column)
            {
                NamedValueExpression nv = (NamedValueExpression)left;
                ColumnExpression c = (ColumnExpression)right;
                left = QueryUtility.GetNamedValueExpression(nv.Name, nv.Value, (DbType)c.MapInfo.DataType);
            }
            else if (b.Right.NodeType == (ExpressionType)DbExpressionType.NamedValue
             && b.Left.NodeType == (ExpressionType)DbExpressionType.Column)
            {
                NamedValueExpression nv = (NamedValueExpression)right;
                ColumnExpression c = (ColumnExpression)left;
                right = QueryUtility.GetNamedValueExpression(nv.Name, nv.Value, (DbType)c.MapInfo.DataType);
            }

            return b.Update(left, b.Conversion, right);
        }

        protected override ColumnAssignment VisitColumnAssignment(ColumnAssignment ca)
        {
            ca = base.VisitColumnAssignment(ca);
            Expression expression = ca.Expression;
            if (expression is NamedValueExpression nv)
            {
                expression = QueryUtility.GetNamedValueExpression(nv.Name, nv.Value, (DbType)ca.Column.MapInfo.DataType);
            }

            return this.UpdateColumnAssignment(ca, ca.Column, expression);
        }

        int iParam = 0;
        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value != null && !IsNumeric(c.Value.GetType()))
            {
                TypeAndValue tv = new TypeAndValue(c.Type, c.Value);
                if (!this.map.TryGetValue(tv, out NamedValueExpression nv))
                { // re-use same name-value if same type & value
                    string name = "p" + (iParam++);
                    nv = new NamedValueExpression(name, c);
                    this.map.Add(tv, nv);
                }
                return nv;
            }
            return c;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            return this.GetNamedValue(p);
        }


        protected override Expression VisitMethodCall(MethodCallExpression methodCallExp)
        {
            if (methodCallExp.Method.DeclaringType == typeof(ExecutionBuilder))
            {
                return GetNamedValue(methodCallExp);
            }

            var arguments = Visit(methodCallExp.Arguments);
            var obj = Visit(methodCallExp.Object);
            return methodCallExp.Update(obj, arguments);
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            memberExp = (MemberExpression)base.VisitMember(memberExp);
            NamedValueExpression nv = memberExp.Expression as NamedValueExpression;
            if (nv != null)
            {
                Expression x = Expression.MakeMemberAccess(nv.Value, memberExp.Member);
                return GetNamedValue(x);
            }
            return memberExp;
        }

        private Expression GetNamedValue(Expression e)
        {
            NamedValueExpression nv;
            HashedExpression he = new HashedExpression(e);
            if (!this.pmap.TryGetValue(he, out nv))
            {
                string name = "p" + (iParam++);
                nv = new NamedValueExpression(name, e);
                this.pmap.Add(he, nv);
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

        struct TypeAndValue : IEquatable<TypeAndValue>
        {
            Type type;
            object value;
            int hash;

            public TypeAndValue(Type type, object value)
            {
                this.type = type;
                this.value = value;
                this.hash = type.GetHashCode() + (value != null ? value.GetHashCode() : 0);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TypeAndValue))
                    return false;
                return this.Equals((TypeAndValue)obj);
            }

            public bool Equals(TypeAndValue vt)
            {
                return vt.type == this.type && object.Equals(vt.value, this.value);
            }

            public override int GetHashCode()
            {
                return this.hash;
            }
        }

        struct HashedExpression : IEquatable<HashedExpression>
        {
            Expression expression;
            int hashCode;

            public HashedExpression(Expression expression)
            {
                this.expression = expression;
                this.hashCode = Hasher.ComputeHash(expression);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is HashedExpression))
                    return false;
                return this.Equals((HashedExpression)obj);
            }

            public bool Equals(HashedExpression other)
            {
                return this.hashCode == other.hashCode &&
                    DbExpressionComparer.AreEqual(this.expression, other.expression);
            }

            public override int GetHashCode()
            {
                return this.hashCode;
            }

            class Hasher : DbExpressionVisitor
            {
                int hc;

                internal static int ComputeHash(Expression expression)
                {
                    var hasher = new Hasher();
                    hasher.Visit(expression);
                    return hasher.hc;
                }

                protected override Expression VisitConstant(ConstantExpression c)
                {
                    hc = hc + ((c.Value != null) ? c.Value.GetHashCode() : 0);
                    return c;
                }
            }
        }
    }
}