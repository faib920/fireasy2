﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 一个动态程序集的构造器。
    /// </summary>
    public class DynamicAssemblyBuilder : DynamicBuilder
    {
        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;
        private static readonly SafetyDictionary<string, Assembly> _assemblies = new SafetyDictionary<string, Assembly>();
        private readonly List<ITypeCreator> _typeBuilders = new List<ITypeCreator>();
        private bool _isCreated = false;

        /// <summary>
        /// 初始化 <see cref="DynamicAssemblyBuilder"/> 类的新实例。
        /// </summary>
        /// <param name="assemblyName">程序集的名称。</param>
        /// <param name="output">程序集输出的文件名。</param>
        public DynamicAssemblyBuilder(string assemblyName, string output = null)
        {
            AssemblyName = assemblyName;
            OutputAssembly = output;
            Context = new BuildContext { AssemblyBuilder = this };
        }

        /// <summary>
        /// 获取 <see cref="AssemblyBuilder"/> 对象。
        /// </summary>
        public AssemblyBuilder AssemblyBuilder
        {
            get
            {
                return _assemblyBuilder ?? (_assemblyBuilder = InitAssemblyBuilder());
            }
        }

        /// <summary>
        /// 获取 <see cref="AssemblyBuilder"/> 对象。
        /// </summary>
        internal ModuleBuilder ModuleBuilder
        {
            get
            {
                return _moduleBuilder ?? (_moduleBuilder = InitModuleBuilder());
            }
        }

        /// <summary>
        /// 获取 <see cref="AssemblyBuilder"/> 对象。
        /// </summary>
        /// <returns></returns>
        private AssemblyBuilder InitAssemblyBuilder()
        {
            if (_assemblyBuilder == null)
            {
                var an = new AssemblyName(AssemblyName);
                if (string.IsNullOrEmpty(OutputAssembly))
                {
#if !NETSTANDARD
                    _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
#else
                    _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
#endif
                }
                else
                {
#if !NETSTANDARD
                    var dir = Path.GetDirectoryName(OutputAssembly);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave, dir);
#else
                    _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
#endif
                }

                _assemblies.TryAdd(AssemblyName, _assemblyBuilder);

                //如果引用到当前程序集中的其他类型，则需要定义以下事件
                AppDomain.CurrentDomain.AssemblyResolve += (o, e) =>
                    {
                        var names = e.Name.Split(',');
                        if (_assemblies.TryGetValue(names[0].Trim(), out Assembly assembly))
                        {
                            return assembly;
                        }

                        return null;
                    };
            }

            return _assemblyBuilder;
        }

        /// <summary>
        /// 获取 <see cref="ModuleBuilder"/> 对象。
        /// </summary>
        /// <returns></returns>
        private ModuleBuilder InitModuleBuilder()
        {
            if (_moduleBuilder == null)
            {
                if (string.IsNullOrEmpty(OutputAssembly))
                {
                    _moduleBuilder = AssemblyBuilder.DefineDynamicModule("Main");
                }
                else
                {
                    var fileName = OutputAssembly.Substring(OutputAssembly.LastIndexOf("\\") + 1);
#if !NETSTANDARD
                    _moduleBuilder = AssemblyBuilder.DefineDynamicModule(fileName, fileName);
#else
                    _moduleBuilder = AssemblyBuilder.DefineDynamicModule(fileName);
#endif
                }
            }

            return _moduleBuilder;
        }

        /// <summary>
        /// 获取或设置程序集的名称。
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 获取或设置输出的文件名。
        /// </summary>
        public string OutputAssembly { get; set; }

        /// <summary>
        /// 使用当前的构造器定义一个动态类型。
        /// </summary>
        /// <param name="typeName">类型的名称。</param>
        /// <param name="visual">指定类的可见性。</param>
        /// <param name="calling">指定类的调用属性。</param>
        /// <param name="baseType">类型的父类。</param>
        /// <returns></returns>
        public DynamicTypeBuilder DefineType(string typeName, VisualDecoration visual = VisualDecoration.Public, CallingDecoration calling = CallingDecoration.Standard, Type baseType = null)
        {
            var typeBuilder = new DynamicTypeBuilder(Context, typeName, visual, calling, baseType);
            _typeBuilders.Add(typeBuilder);
            return typeBuilder;
        }

        /// <summary>
        /// 使用当前的构造器定义一个动态接口。
        /// </summary>
        /// <param name="typeName">类型的名称。</param>
        /// <param name="visual">指定类的可见性。</param>
        /// <returns></returns>
        public DynamicInterfaceBuilder DefineInterface(string typeName, VisualDecoration visual = VisualDecoration.Public)
        {
            var typeBuilder = new DynamicInterfaceBuilder(Context, typeName, visual);
            _typeBuilders.Add(typeBuilder);
            return typeBuilder;
        }

        /// <summary>
        /// 使用当前构造器定义一个枚举。
        /// </summary>
        /// <param name="enumName">枚举的名称。</param>
        /// <param name="underlyingType">枚举的类型。</param>
        /// <param name="visual">指定枚举的可见性。</param>
        /// <returns></returns>
        public DynamicEnumBuilder DefineEnum(string enumName, Type underlyingType = null, VisualDecoration visual = VisualDecoration.Public)
        {
            var enumBuilder = new DynamicEnumBuilder(Context, enumName, underlyingType ?? typeof(int), visual);
            _typeBuilders.Add(enumBuilder);
            return enumBuilder;
        }

        /// <summary>
        /// 创建程序集。
        /// </summary>
        /// <returns></returns>
        public Assembly Create()
        {
            if (!_isCreated)
            {
                foreach (var typeBuilder in _typeBuilders)
                {
                    typeBuilder.CreateType();
                }

                _isCreated = true;
            }

            return AssemblyBuilder;
        }

#if !NETSTANDARD
        /// <summary>
        /// 将所有的动态类型保存到程序集。
        /// </summary>
        public Assembly Save()
        {
            Create();

            if (!string.IsNullOrEmpty(OutputAssembly))
            {
                var fileName = OutputAssembly.Substring(OutputAssembly.LastIndexOf("\\") + 1);
                AssemblyBuilder.Save(fileName);
            }

            return AssemblyBuilder;
        }
#endif

        /// <summary>
        /// 设置一个 <see cref="CustomAttributeBuilder"/> 对象。
        /// </summary>
        /// <param name="customBuilder"></param>
        protected override void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            AssemblyBuilder.SetCustomAttribute(customBuilder);
        }
    }
}
