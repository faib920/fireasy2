using Fireasy.Data.Entity.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Fireasy.Data.Entity.Linq.Translators
{
    static class InternalExtensions
    {
        public static string GetAvailableColumnName(this IList<ColumnDeclaration> columns, string baseName)
        {
            string name = baseName;
            int n = 0;
            while (!IsUniqueName(columns, name))
            {
                name = baseName + (n++);
            }
            return name;
        }

        private static bool IsUniqueName(IList<ColumnDeclaration> columns, string name)
        {
            foreach (var col in columns)
            {
                if (col.Name == name)
                {
                    return false;
                }
            }
            return true;
        }

#if NET35
        public static MemberAssignment Update(this MemberAssignment assigment, Expression expression)
        {
            if (expression == assigment.Expression)
            {
                return assigment;
            }

            return System.Linq.Expressions.Expression.Bind(assigment.Member, expression);
        }

        public static MethodCallExpression Update(this MethodCallExpression callExp, Expression body, IEnumerable<Expression> arguments)
        {
            if (callExp.Object != body || arguments != callExp.Arguments)
            {
                return Expression.Call(body, callExp.Method, arguments);
            }

            return callExp;
        }

        public static BinaryExpression Update(this BinaryExpression binary, Expression left, LambdaExpression conversion, Expression right)
        {
            if (binary.Left == left && binary.Right == right && binary.Conversion == conversion)
            {
                return binary;
            }

            return Expression.MakeBinary(binary.NodeType, left, right, binary.IsLiftedToNull, binary.Method, conversion);
        }
#endif
    }
}
