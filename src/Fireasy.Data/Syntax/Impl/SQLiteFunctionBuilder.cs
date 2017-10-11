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
        private static List<string> cache = new List<string>();
        private static object locker = new object();
        private static DynamicAssemblyBuilder assemblyBuilder = new DynamicAssemblyBuilder("SQLiteFunctionAssembly");
        private static Type funcType = "System.Data.SQLite.SQLiteFunction,System.Data.SQLite".ParseType();
        private static Type funcAttrType = "System.Data.SQLite.SQLiteFunctionAttribute,System.Data.SQLite".ParseType();
        private static Type funcTypeType = "System.Data.SQLite.FunctionType,System.Data.SQLite".ParseType();
        private static MethodInfo registerMethod = funcType.GetMethod("RegisterFunction", BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// 注册一个新的自定义函数。
        /// </summary>
        /// <param name="functionName">函数的名称。</param>
        /// <param name="paramsCount">参数的个数。</param>
        /// <param name="actionIL">自定义函数的代码IL。</param>
        public void Register(string functionName, int paramsCount, Action<BuildContext> actionIL)
        {
            var key = string.Format("{0}_{1}", functionName, paramsCount);

            lock (locker)
            {
                if (!cache.Contains(key))
                {
                    InternalRegister(BuildFunctionType(functionName, paramsCount, actionIL));
                    cache.Add(key);
                }
            }
        }

        /// <summary>
        /// 注册正则表达式匹配的函数。
        /// </summary>
        public void RegisterRegexFunction()
        {
            var convertMethod = typeof(Convert).GetMethods().FirstOrDefault(s => s.Name == "ToString" && s.GetParameters()[0].ParameterType == typeof(object));
            var matchMethod = typeof(Regex).GetMethods().FirstOrDefault(s => s.Name == "IsMatch" && s.GetParameters().Length == 3);

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
            var scalar = funcTypeType.GetField("Scalar");
            var bindings = new[] {
                    Expression.Bind(funcAttrType.GetProperty("Name"), Expression.Constant(functionName)),
                    Expression.Bind(funcAttrType.GetProperty("Arguments"), Expression.Constant(paramsCount)),
                    Expression.Bind(funcAttrType.GetProperty("FuncType"), Expression.MakeMemberAccess(null, scalar))
                };

            var typeBuilder = assemblyBuilder.DefineType(string.Format("SQLiteFunction_{0}_{1}", functionName, paramsCount), baseType: funcType);
            typeBuilder.SetCustomAttribute(Expression.MemberInit(Expression.New(funcAttrType), bindings));
            typeBuilder.DefineMethod("Invoke", typeof(object), new Type[] { typeof(object[]) }, ilCoding: actionIL).DefineParameter("args");
            return typeBuilder.CreateType();
        }

        private void InternalRegister(Type functionType)
        {
            registerMethod.Invoke(null, new object[] { functionType });
        }
    }
}
