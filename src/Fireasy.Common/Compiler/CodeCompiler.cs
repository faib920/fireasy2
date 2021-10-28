// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
#else
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Linq;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Fireasy.Common.Extensions;
using Microsoft.VisualBasic;

namespace Fireasy.Common.Compiler
{
    /// <summary>
    /// 代码编译器，提供对动态代码的编译。无法继承此类。
    /// </summary>
    public sealed class CodeCompiler
    {
        /// <summary>
        /// 初始化 <see cref="CodeCompiler"/> 类的新实例。
        /// </summary>
        public CodeCompiler()
        {
#if !NETSTANDARD
            CodeProvider = new CSharpCodeProvider();
#else
            SyntaxTreeParser = s => CSharpSyntaxTree.ParseText(s);
#endif
        }

#if !NETSTANDARD
        /// <summary>
        /// 获取或设置代码编译的提供者，默认为 <see cref="CSharpCodeProvider"/>。
        /// </summary>
        public CodeDomProvider CodeProvider { get; set; }
#else 
        /// <summary>
        /// 获取或设置语法解析器。
        /// </summary>
        public Func<string, SyntaxTree> SyntaxTreeParser { get; set; }
#endif

        /// <summary>
        /// 获取或设置输出的程序集。
        /// </summary>
        public string OutputAssembly { get; set; }

        /// <summary>
        /// 获取或设置编译选项。
        /// </summary>
        public string CompilerOptions { get; set; }

        /// <summary>
        /// 获取附加的程序集。
        /// </summary>
        public List<string> Assemblies { get; private set; } = new List<string>();

#if !NETSTANDARD
        /// <summary>
        /// 编译代码并返回指定方法的委托。如果未指定方法名称，则返回类的第一个方法。
        /// </summary>
        /// <typeparam name="TDelegate">委托类型。</typeparam>
        /// <param name="unit">代码模型容器。</param>
        /// <param name="methodName">方法的名称。</param>
        /// <returns>代码中对应方法的委托。</returns>
        public TDelegate CompileDelegate<TDelegate>(CodeCompileUnit unit, string methodName = null)
        {
            var compileType = CompileType(unit);
            return MakeDelegate<TDelegate>(compileType, methodName);
        }

        /// <summary>
        /// 使用一组源程序文件来生成一个程序集。
        /// </summary>
        /// <param name="unit">代码模型容器。</param>
        /// <returns>由代码编译成的程序集。</returns>
        public Assembly CompileAssembly(CodeCompileUnit unit)
        {
            var compileOption = GetCompilerParameters();

            var compileResult = CodeProvider.CompileAssemblyFromDom(compileOption, unit);
            if (compileResult.Errors.HasErrors)
            {
                ThrowCompileException(compileResult);
            }

            return compileResult.CompiledAssembly;
        }

        /// <summary>
        /// 使用 <see cref="CodeCompileUnit"/> 来生成一个新类型。
        /// </summary>
        /// <param name="unit">代码模型容器。</param>
        /// <param name="typeName">类的名称。</param>
        /// <returns>由代码编译成的动态类型。</returns>
        public Type CompileType(CodeCompileUnit unit, string typeName = null)
        {
            var assembly = CompileAssembly(unit);

            return GetTypeFromAssembly(assembly, typeName);
        }
#endif

        /// <summary>
        /// 编译代码并返回指定方法的委托。如果未指定方法名称，则返回类的第一个方法。
        /// </summary>
        /// <typeparam name="TDelegate">委托类型。</typeparam>
        /// <param name="source">程序源代码，代码中只允许包含一个类。</param>
        /// <param name="methodName">方法的名称。</param>
        /// <returns>代码中对应方法的委托。</returns>
        public TDelegate CompileDelegate<TDelegate>(string source, string methodName = null)
        {
            var compileType = CompileType(source);
            return MakeDelegate<TDelegate>(compileType, methodName);
        }

        /// <summary>
        /// 编译代码生成一个程序集。
        /// </summary>
        /// <param name="source">程序源代码。</param>
        /// <returns>由代码编译成的程序集。</returns>
        public Assembly CompileAssembly(string source)
        {
#if !NETSTANDARD
            var compileOption = GetCompilerParameters();

            var compileResult = CodeProvider.CompileAssemblyFromSource(compileOption, source);
            if (compileResult.Errors.HasErrors)
            {
                ThrowCompileException(compileResult);
            }

            return compileResult.CompiledAssembly;
#else
            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString())
                .AddSyntaxTrees(SyntaxTreeParser(source))
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddReferences(Assemblies.Select(s => MetadataReference.CreateFromFile(s)))
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

            if (!string.IsNullOrEmpty(OutputAssembly))
            {
                var result = compilation.Emit(OutputAssembly);
                if (result.Success)
                {
                    return Assembly.Load(OutputAssembly);
                }
                else
                {
                    ThrowCompileException(result);
                    return null;
                }
            }
            else
            {
                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    return Assembly.Load(ms.ToArray());
                }
                else
                {
                    ThrowCompileException(result);
                    return null;
                }
            }
#endif
        }

        /// <summary>
        /// 编译代码生成一个新类型。
        /// </summary>
        /// <param name="source">程序源代码。</param>
        /// <param name="typeName">类的名称。</param>
        /// <returns>由代码编译成的动态类型。</returns>
        public Type CompileType(string source, string typeName = null)
        {
            var assembly = CompileAssembly(source);

            return GetTypeFromAssembly(assembly, typeName);
        }

#if !NETSTANDARD
        /// <summary>
        /// 编译代码并返回指定方法的委托。如果未指定方法名称，则返回类的第一个方法。
        /// </summary>
        /// <typeparam name="TDelegate">委托类型。</typeparam>
        /// <param name="fileNames">外部的一组源程序文件。</param>
        /// <param name="methodName">方法的名称。</param>
        /// <returns>代码中对应方法的委托。</returns>
        public TDelegate CompileDelegate<TDelegate>(string[] fileNames, string methodName = null)
        {
            var compileType = CompileType(fileNames);
            return MakeDelegate<TDelegate>(compileType, methodName);
        }

        /// <summary>
        /// 使用一组源程序文件来生成一个程序集。
        /// </summary>
        /// <param name="fileNames">外部的一组源程序文件。</param>
        /// <returns>由代码编译成的程序集。</returns>
        public Assembly CompileAssembly(string[] fileNames)
        {
            var compileOption = GetCompilerParameters();

            var compileResult = CodeProvider.CompileAssemblyFromFile(compileOption, fileNames);
            if (compileResult.Errors.HasErrors)
            {
                ThrowCompileException(compileResult);
            }

            return compileResult.CompiledAssembly;
        }

        /// <summary>
        /// 使用一组源程序文件来生成一个新类型。
        /// </summary>
        /// <param name="fileNames">外部的一组源程序文件。</param>
        /// <param name="typeName">类的名称。</param>
        /// <returns>由代码编译成的动态类型。</returns>
        public Type CompileType(string[] fileNames, string typeName = null)
        {
            var assembly = CompileAssembly(fileNames);

            return GetTypeFromAssembly(assembly, typeName);
        }
#endif

        private Type GetTypeFromAssembly(Assembly assembly, string typeName = null)
        {
            if (assembly != null)
            {
                if (!string.IsNullOrEmpty(typeName))
                {
                    return assembly.GetType(typeName);
                }

                var types = assembly.GetExportedTypes();
                if (types.Length > 0)
                {
                    return types[0];
                }
            }

            return null;
        }

#if !NETSTANDARD
        private CompilerParameters GetCompilerParameters()
        {
            var option = new CompilerParameters();
            if (!string.IsNullOrEmpty(OutputAssembly))
            {
                var exten = Path.GetExtension(OutputAssembly);
                if (exten != null)
                {
                    option.GenerateExecutable = exten.ToLower() == ".exe";
                    option.OutputAssembly = OutputAssembly;
                }
            }
            else
            {
                option.GenerateInMemory = true;
            }

            option.CompilerOptions = CompilerOptions;

            AddAssembly(option);

            return option;
        }

        private void AddAssembly(CompilerParameters option)
        {
            option.ReferencedAssemblies.Add("system.dll");
            foreach (var assembly in Assemblies)
            {
                if (!option.ReferencedAssemblies.Contains(assembly))
                {
                    option.ReferencedAssemblies.Add(assembly);
                }
            }
        }

        private void ThrowCompileException(CompilerResults result)
        {
            var errorBuilder = new StringBuilder();
            foreach (CompilerError error in result.Errors)
            {
                errorBuilder.AppendLine(string.Format("({0},{1}): {2}", error.Line, error.Column, error.ErrorText));
            }

            throw new CodeCompileException(errorBuilder.ToString());
        }
#else
        private void ThrowCompileException(EmitResult result)
        {
            var errorBuilder = new StringBuilder();

            foreach (var diagnostic in result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error))
            {
                errorBuilder.AppendFormat("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
            }

            throw new CodeCompileException(errorBuilder.ToString());
        }
#endif

        private TDelegate MakeDelegate<TDelegate>(Type compileType, string methodName)
        {
            if (compileType == null)
            {
                return default;
            }

            var theMethod = string.IsNullOrEmpty(methodName) ? compileType.GetMethods()[0] : compileType.GetMethod(methodName);
            if (theMethod == null)
            {
                throw new ArgumentException(SR.GetString(SRKind.MethodNotFound, methodName));
            }

            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), theMethod.IsStatic ? null : compileType.New(), theMethod);
        }
    }

    /// <summary>
    /// 编译所使用的语言。
    /// </summary>
    public enum Language
    {
        CSharp,
        VisualBasic
    }
}
