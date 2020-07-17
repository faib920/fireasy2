// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Caching;
using Fireasy.Common.Emit;
using Fireasy.Data.RecordWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Fireasy.Data
{
    /// <summary>
    /// 根据提供的 <see cref="IDataReader"/> 对象的结构来生成一个动态类型。
    /// </summary>
    /// <remarks><see cref="IDatabase"/> 类的非泛型方法 ExecuteEnumerable 会使用该类来创建动态类型。</remarks>
    public sealed class DataReaderTypeBuilder
    {

        private readonly IDataReader _dataReader;

        /// <summary>
        /// 初始化 <see cref="DataReaderTypeBuilder"/> 类的新实例。
        /// </summary>
        /// <param name="dataReader">一个 <see cref="IDataReader"/> 对象。</param>
        public DataReaderTypeBuilder(IDataReader dataReader)
        {
            _dataReader = dataReader;
            ImplInterfaceTypes = new List<Type>();
        }

        /// <summary>
        /// 获取所生成的类型需要实现的接口类型列表。
        /// </summary>
        public List<Type> ImplInterfaceTypes { get; private set; }

        /// <summary>
        /// 根据一个 <see cref="IDataReader"/> 的结构生成一个动态类。
        /// </summary>
        /// <returns>由 <see cref="IDataReader"/> 中数据列所组成属性的动态类型。</returns>
        public Type CreateType()
        {
            var cacheMgr = MemoryCacheManager.Instance;
            var typeName = BuildTypeName(_dataReader);
            return cacheMgr.TryGet(typeName, () => InternalCreateType(_dataReader, ImplInterfaceTypes));
        }

        private static Type InternalCreateType(IDataReader reader, IEnumerable<Type> implInterfaceTypes)
        {
            var guid = Guid.NewGuid().ToString("N");
            var typeBuilder = new DynamicAssemblyBuilder("_Dynamic_" + guid).DefineType("$<>" + guid);
            foreach (var type in implInterfaceTypes)
            {
                typeBuilder.ImplementInterface(type);
            }

            var length = reader.FieldCount;
            var fields = new DynamicFieldBuilder[length];
            for (var i = 0; i < length; i++)
            {
                var name = GetFieldName(reader.GetName(i));
                var type = reader.GetFieldType(i);

                var fieldBuilder = typeBuilder.DefineField("<>__" + name, type);
                typeBuilder.DefineProperty(name, type).DefineGetSetMethods(fieldBuilder);
                fields[i] = fieldBuilder;
            }

            var constructorBuilder = typeBuilder.DefineConstructor(new[] { typeof(IDataReader), typeof(IRecordWrapper) }, ilCoding: bc =>
                bc.Emitter
                    .For(0, length, (e, i) =>
                        e.ldarg_0
                        .ldarg_2
                        .ldarg_1
                        .ldc_i4(i)
                        .callvirt(RecordWrapHelper.GetGetValueMethod(reader.GetFieldType(i)))
                        .stfld(fields[i].FieldBuilder))
                    .ret());
            constructorBuilder.DefineParameter("reader");
            constructorBuilder.DefineParameter("wrapper");

            return typeBuilder.CreateType();
        }

        /// <summary>
        /// 根据 <see cref="IDataReader"/> 生成类型的名称。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>类型的名称。</returns>
        /// <remarks>名称(Name)为字符串，年龄(Age)为整型，则返回的类型的名称为 Name:18$Age:9。</remarks>
        private static string BuildTypeName(IDataReader reader)
        {
            var sb = new StringBuilder();
            var length = reader.FieldCount;
            var assert = new AssertFlag();
            for (var i = 0; i < length; i++)
            {
                if (!assert.AssertTrue())
                {
                    sb.Append("$");
                }

                sb.Append($"{reader.GetName(i)}:{Type.GetTypeCode(reader.GetFieldType(i)).ToString("D")}");
            }

            return sb.ToString();
        }

        private static string GetFieldName(string fieldName)
        {
            var index = fieldName.IndexOf('.');
            if (index != -1)
            {
                return fieldName.Substring(index + 1);
            }

            return fieldName;
        }
    }
}