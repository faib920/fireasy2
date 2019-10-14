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
    }
}
