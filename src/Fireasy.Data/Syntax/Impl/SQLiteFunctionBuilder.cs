// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Emit;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Fireasy.Data.Syntax
{
    /// <summary>
    /// SQLite 自定义函数构造器。
    /// </summary>
    public class SQLiteFunctionBuilder
    {
        private static readonly List<string> _cache = new List<string>();
        private static readonly object _locker = new object();
        private static readonly DynamicAssemblyBuilder _assemblyBuilder = new DynamicAssemblyBuilder("SQLiteFunctionAssembly");
        private static readonly Type _funcType = "System.Data.SQLite.SQLiteFunction,System.Data.SQLite".ParseType();
        private static readonly Type _funcAttrType = "System.Data.SQLite.SQLiteFunctionAttribute,System.Data.SQLite".ParseType();
        private static readonly Type _funcTypeType = "System.Data.SQLite.FunctionType,System.Data.SQLite".ParseType();
        private static readonly MethodInfo _mthRegister = _funcType.GetMethod("RegisterFunction", BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// 注册一个新的自定义函数。
        /// </summary>
        /// <param name="functionName">函数的名称。</param>
        /// <param name="paramsCount">参数的个数。</param>
        /// <param name="actionIL">自定义函数的代码IL。</param>
        public void Register(string functionName, int paramsCount, Action<BuildContext> actionIL)
        {
            var key = $"{functionName}_{paramsCount}";

            lock (_locker)
            {
                if (!_cache.Contains(key))
                {
                    InternalRegister(BuildFunctionType(functionName, paramsCount, actionIL));
                    _cache.Add(key);
                }
            }
        }

        /// <summary>
        /// 注册正则表达式匹配的函数。
        /// </summary>
        public void RegisterRegexFunction()
        {
            var convertMethod = typeof(Convert).GetMethods().FirstOrDefault(s => s.Name == nameof(Convert.ToString) && s.GetParameters()[0].ParameterType == typeof(object));
            var matchMethod = typeof(Regex).GetMethods().FirstOrDefault(s => s.Name == nameof(Regex.IsMatch) && s.GetParameters().Length == 3);

            Register("REGEXP", 2, c =>
                {
                    c.Emitter
                        .ldarg_1
                        .ldc_i4_0
                        .ldelem_ref
                        .call(convertMethod)
                        .ldarg_1
                        .ldc_i4_1
                        .ldelem_ref
                        .call(convertMethod)
                        .ldc_i4_1
                        .call(matchMethod)
                        .box(typeof(bool))
                        .ret();
                });
        }

        /// <summary>
        /// 创建 SQLiteFunction 子类。
        /// </summary>
        /// <param name="functionName">函数的名称。</param>
        /// <param name="paramsCount">参数的个数。</param>
        /// <param name="actionIL">自定义函数的代码IL。</param>
        /// <returns></returns>
        protected virtual Type BuildFunctionType(string functionName, int paramsCount, Action<BuildContext> actionIL)
        {
            var scalar = _funcTypeType.GetField("Scalar");
            var bindings = new[] {
                    Expression.Bind(_funcAttrType.GetProperty("Name"), Expression.Constant(functionName)),
                    Expression.Bind(_funcAttrType.GetProperty("Arguments"), Expression.Constant(paramsCount)),
                    Expression.Bind(_funcAttrType.GetProperty("FuncType"), Expression.MakeMemberAccess(null, scalar))
                };

            var typeBuilder = _assemblyBuilder.DefineType(string.Format("SQLiteFunction_{0}_{1}", functionName, paramsCount), baseType: _funcType);
            typeBuilder.SetCustomAttribute(Expression.MemberInit(Expression.New(_funcAttrType), bindings));
            typeBuilder.DefineMethod("Invoke", typeof(object), new Type[] { typeof(object[]) }, ilCoding: actionIL).DefineParameter("args");
            return typeBuilder.CreateType();
        }

        private void InternalRegister(Type functionType)
        {
            _mthRegister.FastInvoke(null, new object[] { functionType });
        }
    }
}
