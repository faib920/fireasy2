// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 对 MSIL 的指令进行包装，使其支持链式语法。
    /// </summary>
    [SuppressMessage("Style", "IDE1006")]
    public sealed class EmitHelper
    {
        private readonly MethodBuilder _methodBuilder;

        /// <summary>
        /// 初始化 <see cref="EmitHelper"/> 类的新实例。
        /// </summary>
        public EmitHelper()
        {
        }

        /// <summary>
        /// 使用一个指令器初始化 <see cref="EmitHelper"/> 类的新实例。
        /// </summary>
        /// <param name="generator">MSIL 指令器。</param>
        public EmitHelper(ILGenerator generator)
            : this(generator, null)
        {
        }

        /// <summary>
        /// 使用指令器和方法初始化 <see cref="EmitHelper"/> 类的新实例。
        /// </summary>
        /// <param name="generator">MSIL 指令器。</param>
        /// <param name="methodBuilder">编写指令的方法。</param>
        public EmitHelper(ILGenerator generator, MethodBuilder methodBuilder)
        {
            ILGenerator = generator;
            _methodBuilder = methodBuilder;
        }

        /// <summary>
        /// 将 <see cref="EmitHelper"/> 隐式转换为 <see cref="ILGenerator"/>。
        /// </summary>
        /// <param name="emitHelper"></param>
        /// <returns></returns>
        public static implicit operator ILGenerator(EmitHelper emitHelper)
        {
            return emitHelper.ILGenerator;
        }

        /// <summary>
        /// 获取 MSIL 指令生成器。
        /// </summary>
        public ILGenerator ILGenerator { get; private set; }

        #region ILGenerator Methods

        /// <summary>
        /// 开始 Catch 块。
        /// </summary>
        /// <param name="exceptionType">表示异常的 System.Type 对象。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper BeginCatchBlock(Type exceptionType)
        {
            ILGenerator.BeginCatchBlock(exceptionType);
            return this;
        }

        /// <summary>
        /// 开始已筛选异常的异常块。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper BeginExceptFilterBlock()
        {
            ILGenerator.BeginExceptFilterBlock();
            return this;
        }

        /// <summary>
        /// 开始非筛选异常的异常块。
        /// </summary>
        /// <returns>块结尾的标签。这将使您停在正确的位置执行 Finally 块或完成 Try 块。</returns>
        public Label BeginExceptionBlock()
        {
            return ILGenerator.BeginExceptionBlock();
        }

        /// <summary>
        /// 在 Microsoft 中间语言 (MSIL) 流中开始一个异常错误块。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper BeginFaultBlock()
        {
            ILGenerator.BeginFaultBlock();
            return this;
        }

        /// <summary>
        /// 在 Microsoft 中间语言 (MSIL) 指令流中开始一个 Finally 块。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper BeginFinallyBlock()
        {
            ILGenerator.BeginFinallyBlock();
            return this;
        }

        /// <summary>
        /// 开始词法范围。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper BeginScope()
        {
            ILGenerator.BeginScope();
            return this;
        }

        /// <summary>
        /// 声明指定类型的局部变量。
        /// </summary>
        /// <param name="localType">一个 System.Type 对象，表示局部变量的类型。</param>
        /// <returns>已声明的局部变量。</returns>
        public LocalBuilder DeclareLocal(Type localType)
        {
            return ILGenerator.DeclareLocal(localType);
        }

        /// <summary>
        /// 声明指定类型的局部变量。
        /// </summary>
        /// <param name="builder">一个 <see cref="DynamicTypeBuilder"/>。</param>
        /// <returns>已声明的局部变量。</returns>
        public LocalBuilder DeclareLocal(DynamicTypeBuilder builder)
        {
            return ILGenerator.DeclareLocal(builder.TypeBuilder);
        }

        /// <summary>
        /// 声明指定类型的局部变量，还可以选择固定该变量所引用的对象。
        /// </summary>
        /// <param name="localType">一个 System.Type 对象，表示局部变量的类型。</param>
        /// <param name="pinned">如果要将对象固定在内存中，则为 true；否则为 false。</param>
        /// <returns>已声明的局部变量。</returns>
        public LocalBuilder DeclareLocal(Type localType, bool pinned)
        {
            return ILGenerator.DeclareLocal(localType, pinned);
        }

        /// <summary>
        /// 声明新标签。
        /// </summary>
        /// <returns>返回可用作分支标记的新标签。</returns>
        public Label DefineLabel()
        {
            return ILGenerator.DefineLabel();
        }

        /// <summary>
        /// 结束异常块。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper EndExceptionBlock()
        {
            ILGenerator.EndExceptionBlock();
            return this;
        }

        /// <summary>
        /// 结束词法范围。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper EndScope()
        {
            ILGenerator.EndScope();
            return this;
        }

        /// <summary>
        /// 用给定标签标记 Microsoft 中间语言 (MSIL) 流的当前位置。
        /// </summary>
        /// <param name="loc">为其设置索引的标签。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper MarkLabel(Label loc)
        {
            ILGenerator.MarkLabel(loc);
            return this;
        }

#if !NETSTANDARD
        /// <summary>
        /// 在 Microsoft 中间语言 (MSIL) 流中标记序列点。
        /// </summary>
        /// <param name="document">为其定义序列点的文档。</param>
        /// <param name="startLine">序列点开始的行。</param>
        /// <param name="startColumn">序列点开始的行中的列。</param>
        /// <param name="endLine">序列点结束的行。</param>
        /// <param name="endColumn">序列点结束的行中的列。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper MarkSequencePoint(
            ISymbolDocumentWriter document,
            int startLine,
            int startColumn,
            int endLine,
            int endColumn)
        {
            ILGenerator.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
            return this;
        }
#endif

        /// <summary>
        /// 发出指令以引发异常。
        /// </summary>
        /// <param name="exceptionType">要引发的异常类型的类。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ThrowException(Type exceptionType)
        {
            ILGenerator.ThrowException(exceptionType);
            return this;
        }

        /// <summary>
        /// 指定用于计算当前活动词法范围的局部变量和监视值的命名空间。
        /// </summary>
        /// <param name="namespaceName">用于计算当前活动词法范围的局部变量和监视值的命名空间。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper UsingNamespace(string namespaceName)
        {
            ILGenerator.UsingNamespace(namespaceName);
            return this;
        }

        #endregion

        #region Emit Wrappers

        /// <summary>
        /// 将两个值相加并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper add
        {
            get
            {
                ILGenerator.Emit(OpCodes.Add);
                return this;
            }
        }

        /// <summary>
        /// 将两个整数相加，执行溢出检查，并且将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper add_ovf
        {
            get
            {
                ILGenerator.Emit(OpCodes.Add_Ovf);
                return this;
            }
        }

        /// <summary>
        /// 将两个无符号整数值相加，执行溢出检查，并且将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper add_ovf_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Add_Ovf_Un);
                return this;
            }
        }


        /// <summary>
        /// 计算两个值的按位“与”并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper and
        {
            get
            {
                ILGenerator.Emit(OpCodes.And);
                return this;
            }
        }

        /// <summary>
        /// 返回指向当前方法的参数列表的非托管指针。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper arglist
        {
            get
            {
                ILGenerator.Emit(OpCodes.Arglist);
                return this;
            }
        }

        /// <summary>
        /// 如果两个值相等，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper beq(Label label)
        {
            ILGenerator.Emit(OpCodes.Beq, label);
            return this;
        }

        /// <summary>
        /// 如果两个值相等，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper beq_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Beq_S, label);
            return this;
        }

        /// <summary>
        /// 如果第一个值大于或等于第二个值，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bge(Label label)
        {
            ILGenerator.Emit(OpCodes.Bge, label);
            return this;
        }

        /// <summary>
        /// 如果第一个值大于或等于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bge_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Bge_S, label);
            return this;
        }

        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bge_un(Label label)
        {
            ILGenerator.Emit(OpCodes.Bge_Un, label);
            return this;
        }

        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bge_un_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Bge_Un_S, label);
            return this;
        }

        /// <summary>
        /// 如果第一个值大于第二个值，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bgt(Label label)
        {
            ILGenerator.Emit(OpCodes.Bgt, label);
            return this;
        }

        /// <summary>
        /// 如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bgt_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Bgt_S, label);
            return this;
        }

        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bgt_un(Label label)
        {
            ILGenerator.Emit(OpCodes.Bgt_Un, label);
            return this;
        }

        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bgt_un_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Bgt_Un_S, label);
            return this;
        }

        /// <summary>
        /// 如果第一个值小于或等于第二个值，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ble(Label label)
        {
            ILGenerator.Emit(OpCodes.Ble, label);
            return this;
        }

        /// <summary>
        /// 如果第一个值小于或等于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ble_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Ble_S, label);
            return this;
        }

        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于或等于第二个值，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ble_un(Label label)
        {
            ILGenerator.Emit(OpCodes.Ble_Un, label);
            return this;
        }

        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点值时，如果第一个值小于或等于第二个值，则将控制权转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ble_un_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Ble_Un_S, label);
            return this;
        }

        /// <summary>
        /// 如果第一个值小于第二个值，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper blt(Label label)
        {
            ILGenerator.Emit(OpCodes.Blt, label);
            return this;
        }

        /// <summary>
        /// 如果第一个值小于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper blt_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Blt_S, label);
            return this;
        }

        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于第二个值，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper blt_un(Label label)
        {
            ILGenerator.Emit(OpCodes.Blt_Un, label);
            return this;
        }

        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper blt_un_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Blt_Un_S, label);
            return this;
        }

        /// <summary>
        /// 当两个无符号整数值或不可排序的浮点型值不相等时，将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bne_un(Label label)
        {
            ILGenerator.Emit(OpCodes.Bne_Un, label);
            return this;
        }

        /// <summary>
        /// 当两个无符号整数值或不可排序的浮点型值不相等时，将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper bne_un_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Bne_Un_S, label);
            return this;
        }

        /// <summary>
        /// 将值类转换为对象引用（O 类型）。
        /// </summary>
        /// <param name="type">A Type</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper box(Type type)
        {
            ILGenerator.Emit(OpCodes.Box, type);
            return this;
        }

        /// <summary>
        /// 如果类型为值类型，将值类转换为对象引用（O 类型），否则不做任何处理。
        /// </summary>
        /// <param name="type">A Type</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper boxIfValueType(Type type)
        {
            Guard.ArgumentNull(type, nameof(type));

            return type.IsValueType ? box(type) : this;
        }

        /// <summary>
        /// 无条件地将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper br(Label label)
        {
            ILGenerator.Emit(OpCodes.Br, label);
            return this;
        }

        /// <summary>
        /// 无条件地将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper br_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Br_S, label);
            return this;
        }

        /// <summary>
        /// 向 Common Language Infrastructure (CLI) 发出信号以通知调试器已撞上了一个断点。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper @break
        {
            get
            {
                ILGenerator.Emit(OpCodes.Break);
                return this;
            }
        }

        /// <summary>
        /// 如果 value 为 false、空引用（Visual Basic 中的 Nothing）或零，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper brfalse(Label label)
        {
            ILGenerator.Emit(OpCodes.Brfalse, label);
            return this;
        }

        /// <summary>
        /// 如果 value 为 false、空引用或零，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper brfalse_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Brfalse_S, label);
            return this;
        }

        /// <summary>
        /// 如果 value 为 true、非空或非零，则将控制转移到目标指令。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper brtrue(Label label)
        {
            ILGenerator.Emit(OpCodes.Brtrue, label);
            return this;
        }

        /// <summary>
        /// 如果 value 为 true、非空或非零，则将控制转移到目标指令（短格式）。
        /// </summary>
        /// <param name="label">The label to branch from this location.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper brtrue_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Brtrue_S, label);
            return this;
        }

        /// <summary>
        /// 调用由传递的方法说明符指示的方法。
        /// </summary>
        /// <param name="methodInfo">The method to be called.</param>
        public EmitHelper call(MethodInfo methodInfo)
        {
            ILGenerator.Emit(OpCodes.Call, methodInfo);
            return this;
        }

        /// <summary>
        /// 调用由传递的方法说明符指示的方法。
        /// </summary>
        /// <param name="builder">构造器。</param>
        public EmitHelper call(DynamicMethodBuilder builder)
        {
            ILGenerator.Emit(OpCodes.Call, builder.MethodBuilder);
            return this;
        }

        /// <summary>
        /// 调用由传递的构造函数说明符指示的构造函数。
        /// </summary>
        /// <param name="constructorInfo">The constructor to be called.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper call(ConstructorInfo constructorInfo)
        {
            ILGenerator.Emit(OpCodes.Call, constructorInfo);
            return this;
        }

        /// <summary>
        /// 调用由传递的方法说明符指示的方法。
        /// </summary>
        /// <param name="methodInfo">The method to be called.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper call(MethodInfo methodInfo, Type[] optionalParameterTypes)
        {
            ILGenerator.EmitCall(OpCodes.Call, methodInfo, optionalParameterTypes);
            return this;
        }

        /// <summary>
        /// 调用由传递的方法说明符指示的方法。
        /// </summary>
        /// <param name="builder">构造器。</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper call(DynamicMethodBuilder builder, Type[] optionalParameterTypes)
        {
            ILGenerator.EmitCall(OpCodes.Call, builder.MethodBuilder, optionalParameterTypes);
            return this;
        }

        /// <summary>
        /// 调用指定名称的方法。
        /// </summary>
        /// <param name="type">A Type</param>
        /// <param name="methodName">The name of the method to be called.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper call(Type type, string methodName, params Type[] optionalParameterTypes)
        {
            Guard.ArgumentNull(type, nameof(type));

            var methodInfo = type.GetMethod(methodName, optionalParameterTypes);

            Guard.NullReference(methodInfo);

            return call(methodInfo);
        }

        /// <summary>
        /// 调用指定名称的方法。
        /// </summary>
        /// <param name="type">A Type</param>
        /// <param name="methodName">The name of the method to be called.</param>
        /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> 
        /// that specify how the search is conducted.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper call(Type type, string methodName, BindingFlags flags, params Type[] optionalParameterTypes)
        {
            Guard.ArgumentNull(type, nameof(type));

            var methodInfo = type.GetMethod(methodName, flags, null, optionalParameterTypes, null);

            Guard.NullReference(methodInfo);

            return call(methodInfo);
        }

        /// <summary>
        /// 调用由传递的方法说明符指示的方法（作为指向入口点的指针）。
        /// </summary>
        /// <param name="methodInfo">The method to be called.</param>
        public EmitHelper calli(MethodInfo methodInfo)
        {
            ILGenerator.Emit(OpCodes.Calli, methodInfo);
            return this;
        }

        /// <summary>
        /// 调用由传递的构造函数说明符指示的构造函数（作为指向入口点的指针）。
        /// </summary>
        /// <param name="constructorInfo">The constructor to be called.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper calli(ConstructorInfo constructorInfo)
        {
            ILGenerator.Emit(OpCodes.Calli, constructorInfo);
            return this;
        }

        /// <summary>
        /// 调用由传递的方法说明符指示的方法（作为指向入口点的指针）。
        /// </summary>
        /// <param name="methodInfo">The method to be called.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper calli(MethodInfo methodInfo, Type[] optionalParameterTypes)
        {
            ILGenerator.EmitCall(OpCodes.Calli, methodInfo, optionalParameterTypes);
            return this;
        }

        /// <summary>
        /// 调用指定名称的方法（作为指向入口点的指针）。
        /// </summary>
        /// <param name="type">A Type</param>
        /// <param name="methodName">The name of the method to be called.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper calli(Type type, string methodName, params Type[] optionalParameterTypes)
        {
            Guard.ArgumentNull(type, nameof(type));

            var methodInfo = type.GetMethod(methodName, optionalParameterTypes);

            Guard.NullReference(methodInfo);

            return calli(methodInfo);
        }

        /// <summary>
        /// 调用指定名称的方法（作为指向入口点的指针）。
        /// </summary>
        /// <param name="type">A Type</param>
        /// <param name="methodName">The name of the method to be called.</param>
        /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> 
        /// that specify how the search is conducted.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper calli(Type type, string methodName, BindingFlags flags, params Type[] optionalParameterTypes)
        {
            Guard.ArgumentNull(type, nameof(type));

            var methodInfo = type.GetMethod(methodName, flags, null, optionalParameterTypes, null);

            Guard.NullReference(methodInfo);

            return calli(methodInfo);
        }

        /// <summary>
        /// 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
        /// </summary>
        /// <param name="methodInfo">The method to be called.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper callvirt(MethodInfo methodInfo)
        {
            ILGenerator.Emit(OpCodes.Callvirt, methodInfo);
            return this;
        }

        /// <summary>
        /// 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
        /// </summary>
        /// <param name="builder">构造器。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper callvirt(DynamicMethodBuilder builder)
        {
            ILGenerator.Emit(OpCodes.Callvirt, builder.MethodBuilder);
            return this;
        }

        /// <summary>
        /// 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
        /// </summary>
        /// <param name="methodInfo">The method to be called.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper callvirt(MethodInfo methodInfo, Type[] optionalParameterTypes)
        {
            ILGenerator.EmitCall(OpCodes.Callvirt, methodInfo, optionalParameterTypes);
            return this;
        }

        /// <summary>
        /// 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
        /// </summary>
        /// <param name="builder">构造器。</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper callvirt(DynamicMethodBuilder builder, Type[] optionalParameterTypes)
        {
            ILGenerator.EmitCall(OpCodes.Callvirt, builder.MethodBuilder, optionalParameterTypes);
            return this;
        }

        /// <summary>
        /// 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
        /// </summary>
        /// <param name="methodName">The method to be called.</param>
        /// <param name="type">The declaring type of the method.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper callvirt(Type type, string methodName, params Type[] optionalParameterTypes)
        {
            Guard.ArgumentNull(type, nameof(type));

            var methodInfo = type.GetMethod(methodName, optionalParameterTypes);

            Guard.NullReference(methodInfo);

            return callvirt(methodInfo);
        }

        /// <summary>
        /// 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
        /// </summary>
        /// <param name="methodName">The method to be called.</param>
        /// <param name="type">The declaring type of the method.</param>
        /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> 
        /// that specify how the search is conducted.</param>
        /// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method.</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper callvirt(Type type, string methodName, BindingFlags flags, params Type[] optionalParameterTypes)
        {
            var methodInfo =
                optionalParameterTypes == null ?
                    type.GetMethod(methodName, flags) :
                    type.GetMethod(methodName, flags, null, optionalParameterTypes, null);

            Guard.NullReference(methodInfo);

            return callvirt(methodInfo, null);
        }

        /// <summary>
        /// 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
        /// </summary>
        /// <param name="methodName">要调用的方法名称。</param>
        /// <param name="type">表示一个类型。</param>
        /// <param name="flags">绑定标识。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper callvirt(Type type, string methodName, BindingFlags flags)
        {
            return callvirt(type, methodName, flags, null);
        }

        /// <summary>
        /// 尝试将引用传递的对象转换为指定的类。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper castclass(Type type)
        {
            ILGenerator.Emit(OpCodes.Castclass, type);
            return this;
        }

        /// <summary>
        /// 尝试将对象转换为指定的类，如果对象是值类型，则进行拆箱。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper castType(Type type)
        {
            Guard.ArgumentNull(type, nameof(type));

            return type.IsValueType ? unbox_any(type) : castclass(type);
        }

        /// <summary>
        /// 比较两个值。如果这两个值相等，则将整数值 1 (int32) 推送到计算堆栈上；否则，将 0 (int32) 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ceq
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ceq);
                return this;
            }
        }

        /// <summary>
        /// 比较两个值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper cgt
        {
            get
            {
                ILGenerator.Emit(OpCodes.Cgt);
                return this;
            }
        }

        /// <summary>
        /// 比较两个无符号的或不可排序的值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper cgt_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Cgt_Un);
                return this;
            }
        }

        /// <summary>
        /// 约束要对其进行虚方法调用的类型。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper constrained(Type type)
        {
            ILGenerator.Emit(OpCodes.Constrained, type);
            return this;
        }

        /// <summary>
        /// 如果值不是有限数，则引发 <see cref="System.ArithmeticException"/>。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ckfinite
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ckfinite);
                return this;
            }
        }

        /// <summary>
        /// 比较两个值。如果第一个值小于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper clt
        {
            get
            {
                ILGenerator.Emit(OpCodes.Clt);
                return this;
            }
        }

        /// <summary>
        /// 比较无符号的或不可排序的值 value1 和 value2。如果 value1 小于 value2，则将整数值 1 (int32 ) 推送到计算堆栈上；反之，将 0 ( int32 ) 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper clt_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Clt_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 native int。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_i
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_I);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 int8，然后将其扩展（填充）为 int32。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_i1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_I1);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 int16，然后将其扩展（填充）为 int32。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_i2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_I2);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 int32。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_i4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_I4);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 int64。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_i8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_I8);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为相应的类型。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv(Type type)
        {
            Guard.ArgumentNull(type, nameof(type));

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                    conv_i1.end();
                    break;
                case TypeCode.Int16:
                    conv_i2.end();
                    break;
                case TypeCode.Int32:
                    conv_i4.end();
                    break;
                case TypeCode.Int64:
                    conv_i8.end();
                    break;
                case TypeCode.Byte:
                    conv_u1.end();
                    break;
                case TypeCode.Char:
                case TypeCode.UInt16:
                    conv_u2.end();
                    break;
                case TypeCode.UInt32:
                    conv_u4.end();
                    break;
                case TypeCode.UInt64:
                    conv_u8.end();
                    break;
                case TypeCode.Single:
                    conv_r4.end();
                    break;
                case TypeCode.Double:
                    conv_r8.end();
                    break;
                default:
                    {
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var ci = type.GetConstructor(type.GetGenericArguments());
                            if (ci != null)
                            {
                                newobj(ci);
                                break;
                            }
                        }

                        throw ThrowNotExpectedTypeException(type);
                    }
            }

            return this;
        }


        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 native int，并在溢出时引发 <see cref="System.OverflowException"/>。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 int8 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I1);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 native int，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 int8 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i1_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I1_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 int16 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I2);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 int16 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i2_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I2_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I2_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i4_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I4_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 int64，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I8);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 int64，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_i8_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_I8_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned native int，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned native int，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned int8 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U1);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned int8 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u1_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U1_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned int16 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U2);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned int16 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u2_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U2_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U4);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned int32，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u4_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U4_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned int64，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U8);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned int64，并在溢出时引发 System.OverflowException。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_ovf_u8_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_Ovf_U8_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 float32。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_r4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_R4);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 float64。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_r8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_R8);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的无符号整数值转换为 float32。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_r_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_R_Un);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned native int，然后将其扩展为 native int。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_u
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_U);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned int8，然后将其扩展为 int32。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_u1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_U1);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned int16，然后将其扩展为 int32。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_u2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_U2);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned int32，然后将其扩展为 int32。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_u4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_U4);
                return this;
            }
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned int64，然后将其扩展为 int64。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper conv_u8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Conv_U8);
                return this;
            }
        }

        /// <summary>
        /// 将指定数目的字节从源地址复制到目标地址。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper cpblk
        {
            get
            {
                ILGenerator.Emit(OpCodes.Cpblk);
                return this;
            }
        }

        /// <summary>
        /// 将位于对象（&amp;、* 或 native int 类型）地址的值类型复制到目标对象（&amp;、* 或 native int 类型）的地址。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper cpobj(Type type)
        {
            ILGenerator.Emit(OpCodes.Cpobj, type);
            return this;
        }

        /// <summary>
        /// 将两个值相除并将结果作为浮点（F 类型）或商（int32 类型）推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper div
        {
            get
            {
                ILGenerator.Emit(OpCodes.Div);
                return this;
            }
        }

        /// <summary>
        /// 两个无符号整数值相除并将结果 ( int32 ) 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper div_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Div_Un);
                return this;
            }
        }

        /// <summary>
        /// 复制计算堆栈上当前最顶端的值，然后将副本推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper dup
        {
            get
            {
                ILGenerator.Emit(OpCodes.Dup);
                return this;
            }
        }

        /// <summary>
        /// 将控制从异常的 filter 子句转移回 Common Language Infrastructure (CLI) 异常处理程序。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper endfilter
        {
            get
            {
                ILGenerator.Emit(OpCodes.Endfilter);
                return this;
            }
        }

        /// <summary>
        /// 将控制从异常块的 fault 或 finally 子句转移回 Common Language Infrastructure (CLI) 异常处理程序。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper endfinally
        {
            get
            {
                ILGenerator.Emit(OpCodes.Endfinally);
                return this;
            }
        }

        /// <summary>
        /// 将位于特定地址的内存的指定块初始化为给定大小和初始值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper initblk
        {
            get
            {
                ILGenerator.Emit(OpCodes.Initblk);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定地址的值类型的每个字段初始化为空引用或适当的基元类型的 0。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper initobj(Type type)
        {
            ILGenerator.Emit(OpCodes.Initobj, type);
            return this;
        }

        /// <summary>
        /// 测试对象引用（O 类型）是否为特定类的实例。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper isinst(Type type)
        {
            ILGenerator.Emit(OpCodes.Isinst, type);
            return this;
        }

        /// <summary>
        /// 退出当前方法并跳至指定方法。
        /// </summary>
        /// <param name="methodInfo">要跳转的方法。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper jmp(MethodInfo methodInfo)
        {
            ILGenerator.Emit(OpCodes.Jmp, methodInfo);
            return this;
        }

        /// <summary>
        /// 将参数（由指定索引值引用）加载到堆栈上。
        /// </summary>
        /// <param name="index">表示参数的索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarg(short index)
        {
            ILGenerator.Emit(OpCodes.Ldarg, index);
            return this;
        }

        /// <summary>
        /// 将参数（由指定索引值引用）加载到堆栈上。
        /// </summary>
        /// <param name="index">表示参数的索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarg(int index)
        {
            switch (index)
            {
                case 0:
                    ldarg_0.end();
                    break;
                case 1:
                    ldarg_1.end();
                    break;
                case 2:
                    ldarg_2.end();
                    break;
                case 3:
                    ldarg_3.end();
                    break;
                default:
                    if (index <= byte.MaxValue)
                    {
                        ldarg_s((byte)index);
                    }
                    else if (index <= short.MaxValue)
                    {
                        ldarg((short)index);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    break;
            }

            return this;
        }

        /// <summary>
        /// 将参数（由指定索引值引用）加载到堆栈上。
        /// </summary>
        /// <param name="index">表示参数的索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarga(short index)
        {
            ILGenerator.Emit(OpCodes.Ldarga, index);
            return this;
        }

        /// <summary>
        /// 将参数（由指定的短格式索引引用）加载到计算堆栈上。
        /// </summary>
        /// <param name="index">表示参数的索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarga_s(byte index)
        {
            ILGenerator.Emit(OpCodes.Ldarga_S, index);
            return this;
        }

        /// <summary>
        /// 将参数（由指定索引值引用）加载到堆栈上。
        /// </summary>
        /// <param name="index">表示参数的索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarga(int index)
        {
            if (index <= byte.MaxValue)
            {
                ldarga_s((byte)index);
            }
            else if (index <= short.MaxValue)
            {
                ldarga((short)index);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return this;
        }

        /// <summary>
        /// 将索引为 0 的参数加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarg_0
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldarg_0);
                return this;
            }
        }

        /// <summary>
        /// 将索引为 1 的参数加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarg_1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldarg_1);
                return this;
            }
        }

        /// <summary>
        /// 将索引为 2 的参数加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarg_2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldarg_2);
                return this;
            }
        }

        /// <summary>
        /// 将索引为 3 的参数加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarg_3
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldarg_3);
                return this;
            }
        }

        /// <summary>
        /// 将参数（由指定的短格式索引引用）加载到计算堆栈上。
        /// </summary>
        /// <param name="index">值的索引。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldarg_s(byte index)
        {
            ILGenerator.Emit(OpCodes.Ldarg_S, index);
            return this;
        }

        /// <summary>
        /// 将布尔值参数加载到计算堆栈上。
        /// </summary>
        /// <param name="b">要推送到栈上的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_bool(bool b)
        {
            ILGenerator.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            return this;
        }

        /// <summary>
        /// 将所提供的 int32 类型的值作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <param name="num">要推送到栈上的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4(int num)
        {
            ILGenerator.Emit(OpCodes.Ldc_I4, num);
            return this;
        }

        /// <summary>
        /// 将整数值 0 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_0
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_0);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 1 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_1);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 2 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_2);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 3 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_3
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_3);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 4 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_4);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 5 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_5
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_5);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 6 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_6
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_6);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 7 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_7
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_7);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 8 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_8);
                return this;
            }
        }

        /// <summary>
        /// 将整数值 -1 作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_m1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldc_I4_M1);
                return this;
            }
        }

        /// <summary>
        /// 将指定的整数作为 int32 推送到计算堆栈上。
        /// </summary>
        /// <param name="num">要推送到栈上的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i4_(int num)
        {
            switch (num)
            {
                case -1:
                    ldc_i4_m1.end();
                    break;
                case 0:
                    ldc_i4_0.end();
                    break;
                case 1:
                    ldc_i4_1.end();
                    break;
                case 2:
                    ldc_i4_2.end();
                    break;
                case 3:
                    ldc_i4_3.end();
                    break;
                case 4:
                    ldc_i4_4.end();
                    break;
                case 5:
                    ldc_i4_5.end();
                    break;
                case 6:
                    ldc_i4_6.end();
                    break;
                case 7:
                    ldc_i4_7.end();
                    break;
                case 8:
                    ldc_i4_8.end();
                    break;
                default:
                    if (num >= sbyte.MinValue && num <= sbyte.MaxValue)
                    {
                        ldc_i4_s((sbyte)num);
                    }
                    else
                    {
                        ldc_i4(num);
                    }

                    break;
            }

            return this;
        }

        /// <summary>
        /// 将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）。
        /// </summary>
        /// <param name="num">要推送到栈上的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        [CLSCompliant(false)]
        public EmitHelper ldc_i4_s(sbyte num)
        {
            ILGenerator.Emit(OpCodes.Ldc_I4_S, num);
            return this;
        }

        /// <summary>
        /// 将所提供的 int64 类型的值作为 int64 推送到计算堆栈上。
        /// </summary>
        /// <param name="num">要推送到栈上的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_i8(long num)
        {
            ILGenerator.Emit(OpCodes.Ldc_I8, num);
            return this;
        }

        /// <summary>
        /// 将所提供的 float32 类型的值作为 F (float) 类型推送到计算堆栈上。
        /// </summary>
        /// <param name="num">要推送到栈上的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_r4(float num)
        {
            ILGenerator.Emit(OpCodes.Ldc_R4, num);
            return this;
        }

        /// <summary>
        /// 将所提供的 float64 类型的值作为 F (float) 类型推送到计算堆栈上。
        /// </summary>
        /// <param name="num">要推送到栈上的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldc_r8(double num)
        {
            ILGenerator.Emit(OpCodes.Ldc_R8, num);
            return this;
        }

        /// <summary>
        /// 按照指令中指定的类型，将指定数组索引中的元素加载到计算堆栈的顶部。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem(Type type)
        {
            ILGenerator.Emit(OpCodes.Ldelem, type);
            return this;
        }

        /// <summary>
        /// 将位于指定数组索引处的 native int 类型的元素作为 native int 加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_i
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_I);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 int8 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_i1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_I1);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 int16 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_i2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_I2);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 int32 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_i4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_I4);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 int64 类型的元素作为 int64 加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_i8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_I8);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 float32 类型的元素作为 F 类型（浮点型）加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_r4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_R4);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 float64 类型的元素作为 F 类型（浮点型）加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_r8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_R8);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的包含对象引用的元素作为 O 类型（对象引用）加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_ref
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_Ref);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 unsigned int8 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_u1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_U1);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 unsigned int16 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_u2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_U2);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的 unsigned int32 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelem_u4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldelem_U4);
                return this;
            }
        }

        /// <summary>
        /// 将位于指定数组索引的数组元素的地址作为 &amp; 类型（托管指针）加载到计算堆栈的顶部。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldelema(Type type)
        {
            ILGenerator.Emit(OpCodes.Ldelema, type);
            return this;
        }

        /// <summary>
        /// 查找对象中其引用当前位于计算堆栈的字段的值。
        /// </summary>
        /// <param name="fieldInfo">表示字段的 <see cref="FieldInfo"/>。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldfld(FieldInfo fieldInfo)
        {
            ILGenerator.Emit(OpCodes.Ldfld, fieldInfo);
            return this;
        }

        /// <summary>
        /// 查找对象中其引用当前位于计算堆栈的字段的地址。
        /// </summary>
        /// <param name="fieldInfo">表示字段的 <see cref="FieldInfo"/>。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldflda(FieldInfo fieldInfo)
        {
            ILGenerator.Emit(OpCodes.Ldflda, fieldInfo);
            return this;
        }

        /// <summary>
        /// 查找对象中其引用当前位于计算堆栈的字段的值。
        /// </summary>
        /// <param name="builder">构造器。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldfld(DynamicFieldBuilder builder)
        {
            ILGenerator.Emit(OpCodes.Ldfld, builder.FieldBuilder);
            return this;
        }

        /// <summary>
        /// 查找对象中其引用当前位于计算堆栈的字段的地址。
        /// </summary>
        /// <param name="builder">构造器。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldflda(DynamicFieldBuilder builder)
        {
            ILGenerator.Emit(OpCodes.Ldflda, builder.FieldBuilder);
            return this;
        }

        /// <summary>
        /// 将指向实现特定方法的本机代码的非托管指针（native int 类型）推送到计算堆栈上。
        /// </summary>
        /// <param name="methodInfo">要调用的方法。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldftn(MethodInfo methodInfo)
        {
            ILGenerator.Emit(OpCodes.Ldftn, methodInfo);
            return this;
        }

        /// <summary>
        /// 将 native int 类型的值作为 native int 间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_i
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_I);
                return this;
            }
        }

        /// <summary>
        /// 将 int8 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_i1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_I1);
                return this;
            }
        }

        /// <summary>
        /// 将 int16 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_i2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_I2);
                return this;
            }
        }

        /// <summary>
        /// 将 int32 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_i4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_I4);
                return this;
            }
        }

        /// <summary>
        /// 将 int64 类型的值作为 int64 间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_i8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_I8);
                return this;
            }
        }

        /// <summary>
        /// 将 float32 类型的值作为 F (float) 类型间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_r4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_R4);
                return this;
            }
        }

        /// <summary>
        /// 将 float64 类型的值作为 F (float) 类型间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_r8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_R8);
                return this;
            }
        }

        /// <summary>
        /// 将对象引用作为 O（对象引用）类型间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_ref
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_Ref);
                return this;
            }
        }

        /// <summary>
        /// 将 unsigned int8 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_u1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_U1);
                return this;
            }
        }

        /// <summary>
        /// 将 unsigned int16 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_u2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_U2);
                return this;
            }
        }

        /// <summary>
        /// 将 unsigned int32 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind_u4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldind_U4);
                return this;
            }
        }

        /// <summary>
        /// 将指定类型的值间接加载到计算堆栈上。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldind(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    ldind_i1.end();
                    break;
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    ldind_i2.end();
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    ldind_i4.end();
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ldind_i8.end();
                    break;
                case TypeCode.Single:
                    ldind_r4.end();
                    break;
                case TypeCode.Double:
                    ldind_r8.end();
                    break;
                default:
                    if (type.IsClass)
                    {
                        ldind_ref.end();
                    }
                    else if (type.IsValueType)
                    {
                        stobj(type);
                    }
                    else
                    {
                        throw ThrowNotExpectedTypeException(type);
                    }

                    break;
            }

            return this;
        }

        /// <summary>
        /// 将从零开始的、一维数组的元素的数目推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldlen
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldlen);
                return this;
            }
        }

        /// <summary>
        /// 将指定索引处的局部变量加载到计算堆栈上。
        /// </summary>
        /// <param name="index">变量在栈上的索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloc(short index)
        {
            ILGenerator.Emit(OpCodes.Ldloc, index);
            return this;
        }

        /// <summary>
        /// 将指定的局部变量加载到计算堆栈上。
        /// </summary>
        /// <param name="localBuilder">表示一个局部变量。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloc(LocalBuilder localBuilder)
        {
            ILGenerator.Emit(OpCodes.Ldloc, localBuilder);
            return this;
        }

        /// <summary>
        /// 将位于特定索引处的局部变量的地址加载到计算堆栈上。
        /// </summary>
        /// <param name="index">表示索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloca(short index)
        {
            ILGenerator.Emit(OpCodes.Ldloca, index);
            return this;
        }

        /// <summary>
        /// 将指定的局部变量的地址加载到计算堆栈上。
        /// </summary>
        /// <param name="local">表示一个局部变量。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloca(LocalBuilder local)
        {
            ILGenerator.Emit(OpCodes.Ldloca, local);
            return this;
        }

        /// <summary>
        /// 将位于特定索引处的局部变量的地址加载到计算堆栈上（短格式）。
        /// </summary>
        /// <param name="index">变量的索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloca_s(byte index)
        {
            ILGenerator.Emit(OpCodes.Ldloca_S, index);
            return this;
        }

        /// <summary>
        /// 将索引 0 处的局部变量加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloc_0
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldloc_0);
                return this;
            }
        }

        /// <summary>
        /// 将索引 1 处的局部变量加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloc_1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldloc_1);
                return this;
            }
        }

        /// <summary>
        /// 将索引 2 处的局部变量加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloc_2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldloc_2);
                return this;
            }
        }

        /// <summary>
        /// 将索引 3 处的局部变量加载到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloc_3
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldloc_3);
                return this;
            }
        }

        /// <summary>
        /// 将特定索引处的局部变量加载到计算堆栈上（短格式）。
        /// </summary>
        /// <param name="index">变量的索引值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldloc_s(byte index)
        {
            ILGenerator.Emit(OpCodes.Ldloca_S, index);
            return this;
        }

        /// <summary>
        /// 将空引用（O 类型）推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldnull
        {
            get
            {
                ILGenerator.Emit(OpCodes.Ldnull);
                return this;
            }
        }

        /// <summary>
        /// 将地址指向的值类型对象复制到计算堆栈的顶部。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldobj(Type type)
        {
            ILGenerator.Emit(OpCodes.Ldobj, type);
            return this;
        }

        /// <summary>
        /// 将静态字段的值推送到计算堆栈上。
        /// </summary>
        /// <param name="fieldInfo">表示字段的 <see cref="FieldInfo"/>。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldsfld(FieldInfo fieldInfo)
        {
            ILGenerator.Emit(OpCodes.Ldsfld, fieldInfo);
            return this;
        }

        /// <summary>
        /// 将静态字段的地址推送到计算堆栈上。
        /// </summary>
        /// <param name="fieldInfo">表示字段的 <see cref="FieldInfo"/>。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldsflda(FieldInfo fieldInfo)
        {
            ILGenerator.Emit(OpCodes.Ldsflda, fieldInfo);
            return this;
        }

        /// <summary>
        /// 推送对元数据中存储的字符串的新对象引用。
        /// </summary>
        /// <param name="str">一个字符串。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldstr(string str)
        {
            ILGenerator.Emit(OpCodes.Ldstr, str);
            return this;
        }

        /// <summary>
        /// 将元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上。
        /// </summary>
        /// <param name="methodInfo">要调用的方法。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldtoken(MethodInfo methodInfo)
        {
            ILGenerator.Emit(OpCodes.Ldtoken, methodInfo);
            return this;
        }

        /// <summary>
        /// 将元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上。
        /// </summary>
        /// <param name="fieldInfo">表示字段的 <see cref="FieldInfo"/>。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldtoken(FieldInfo fieldInfo)
        {
            ILGenerator.Emit(OpCodes.Ldtoken, fieldInfo);
            return this;
        }

        /// <summary>
        /// 将元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldtoken(Type type)
        {
            ILGenerator.Emit(OpCodes.Ldtoken, type);
            return this;
        }

        /// <summary>
        /// 将指向实现与指定对象关联的特定虚方法的本机代码的非托管指针（native int 类型）推送到计算堆栈上。
        /// </summary>
        /// <param name="methodInfo">要调用的方法。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ldvirtftn(MethodInfo methodInfo)
        {
            ILGenerator.Emit(OpCodes.Ldvirtftn, methodInfo);
            return this;
        }

        /// <summary>
        /// 退出受保护的代码区域，无条件将控制转移到特定目标指令。
        /// </summary>
        /// <param name="label">表示一个分支标签。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper leave(Label label)
        {
            ILGenerator.Emit(OpCodes.Leave, label);
            return this;
        }

        /// <summary>
        /// 退出受保护的代码区域，无条件将控制转移到目标指令（缩写形式）。
        /// </summary>
        /// <param name="label">表示一个分支标签。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper leave_s(Label label)
        {
            ILGenerator.Emit(OpCodes.Leave_S, label);
            return this;
        }

        /// <summary>
        /// 从本地动态内存池分配特定数目的字节并将第一个分配的字节的地址（瞬态指针，* 类型）推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper localloc
        {
            get
            {
                ILGenerator.Emit(OpCodes.Localloc);
                return this;
            }
        }

        /// <summary>
        /// 将对特定类型实例的类型化引用推送到计算堆栈上。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper mkrefany(Type type)
        {
            ILGenerator.Emit(OpCodes.Mkrefany, type);
            return this;
        }

        /// <summary>
        /// 将两个值相乘并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper mul
        {
            get
            {
                ILGenerator.Emit(OpCodes.Mul);
                return this;
            }
        }

        /// <summary>
        /// 将两个整数值相乘，执行溢出检查，并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper mul_ovf
        {
            get
            {
                ILGenerator.Emit(OpCodes.Mul_Ovf);
                return this;
            }
        }

        /// <summary>
        /// 将两个无符号整数值相乘，执行溢出检查，并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper mul_ovf_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Mul_Ovf_Un);
                return this;
            }
        }

        /// <summary>
        /// 对一个值执行求反并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper neg
        {
            get
            {
                ILGenerator.Emit(OpCodes.Neg);
                return this;
            }
        }

        /// <summary>
        /// 将对新的从零开始的一维数组（其元素属于特定类型）的对象引用推送到计算堆栈上。
        /// </summary>
        /// <param name="type">表示元素的类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper newarr(Type type)
        {
            ILGenerator.Emit(OpCodes.Newarr, type);
            return this;
        }

        /// <summary>
        /// 创建一个值类型的新对象或新实例，并将对象引用（O 类型）推送到计算堆栈上。
        /// </summary>
        /// <param name="constructorInfo">一个构造函数。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper newobj(ConstructorInfo constructorInfo)
        {
            ILGenerator.Emit(OpCodes.Newobj, constructorInfo);
            return this;
        }

        /// <summary>
        /// 创建一个值类型的新对象或新实例，并将对象引用（O 类型）推送到计算堆栈上。
        /// </summary>
        /// <param name="builder">一个构造器。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper newobj(DynamicConstructorBuilder builder)
        {
            ILGenerator.Emit(OpCodes.Newobj, builder.ConstructorBuilder);
            return this;
        }

        /// <summary>
        /// 创建一个值类型的新对象或新实例，并将对象引用（O 类型）推送到计算堆栈上。
        /// </summary>
        /// <param name="type">表示对象的类型。</param>
        /// <param name="parameters">构造的参数类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper newobj(Type type, params Type[] parameters)
        {
            Guard.ArgumentNull(type, nameof(type));

            var ci = type.GetConstructor(parameters);
            return newobj(ci);
        }

        /// <summary>
        /// 如果修补操作码，则填充空间。尽管可能消耗处理周期，但未执行任何有意义的操作。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper nop
        {
            get
            {
                ILGenerator.Emit(OpCodes.Nop);
                return this;
            }
        }

        /// <summary>
        /// 计算堆栈顶部整数值的按位求补并将结果作为相同的类型推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper not
        {
            get
            {
                ILGenerator.Emit(OpCodes.Not);
                return this;
            }
        }

        /// <summary>
        /// 计算位于堆栈顶部的两个整数值的按位求补并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper or
        {
            get
            {
                ILGenerator.Emit(OpCodes.Or);
                return this;
            }
        }

        /// <summary>
        /// 移除当前位于计算堆栈顶部的值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper pop
        {
            get
            {
                ILGenerator.Emit(OpCodes.Pop);
                return this;
            }
        }

        /// <summary>
        /// 指定后面的数组地址操作在运行时不执行类型检查，并且返回可变性受限的托管指针。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper @readonly
        {
            get
            {
                ILGenerator.Emit(OpCodes.Readonly);
                return this;
            }
        }

        /// <summary>
        /// 检索嵌入在类型化引用内的类型标记。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper refanytype
        {
            get
            {
                ILGenerator.Emit(OpCodes.Refanytype);
                return this;
            }
        }

        /// <summary>
        /// 检索嵌入在类型化引用内的地址（&amp; 类型）。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper refanyval(Type type)
        {
            ILGenerator.Emit(OpCodes.Refanyval, type);
            return this;
        }

        /// <summary>
        /// 将两个值相除并将余数推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper rem
        {
            get
            {
                ILGenerator.Emit(OpCodes.Rem);
                return this;
            }
        }

        /// <summary>
        /// 将两个无符号值相除并将余数推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper rem_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Rem_Un);
                return this;
            }
        }

        /// <summary>
        /// 从当前方法返回，并将返回值（如果存在）从调用方的计算堆栈推送到被调用方的计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper ret()
        {
            ILGenerator.Emit(OpCodes.Ret);
            return this;
        }

        /// <summary>
        /// 再次引发当前异常。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper rethrow
        {
            get
            {
                ILGenerator.Emit(OpCodes.Rethrow);
                return this;
            }
        }

        /// <summary>
        /// 将整数值左移（用零填充）指定的位数，并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper shl
        {
            get
            {
                ILGenerator.Emit(OpCodes.Shl);
                return this;
            }
        }

        /// <summary>
        /// 将整数值右移（保留符号）指定的位数，并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper shr
        {
            get
            {
                ILGenerator.Emit(OpCodes.Shr);
                return this;
            }
        }

        /// <summary>
        /// 将无符号整数值右移（用零填充）指定的位数，并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper shr_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Shr_Un);
                return this;
            }
        }

        /// <summary>
        /// 将提供的值类型的大小（以字节为单位）推送到计算堆栈上。
        /// </summary>
        /// <param name="type">表示一个值类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper @sizeof(Type type)
        {
            ILGenerator.Emit(OpCodes.Sizeof, type);
            return this;
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值存储到位于指定索引的参数槽中。
        /// </summary>
        /// <param name="index">槽索引。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper starg(short index)
        {
            ILGenerator.Emit(OpCodes.Starg, index);
            return this;
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值存储在参数槽中的指定索引处（短格式）。
        /// </summary>
        /// <param name="index">槽索引。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper starg_s(byte index)
        {
            ILGenerator.Emit(OpCodes.Starg_S, index);
            return this;
        }

        /// <summary>
        /// 将位于计算堆栈顶部的值存储到位于指定索引的参数槽中。
        /// </summary>
        /// <param name="index">槽索引。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper starg(int index)
        {
            if (index < byte.MaxValue)
            {
                starg_s((byte)index);
            }
            else if (index < short.MaxValue)
            {
                starg((short)index);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return this;
        }

        /// <summary>
        /// 用计算堆栈中的值替换给定索引处的数组元素，其类型由 <paramref name="arrayType"/> 指定。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem(Type arrayType)
        {
            ILGenerator.Emit(OpCodes.Stelem, arrayType);
            return this;
        }

        /// <summary>
        /// 用计算堆栈上的 native int 值替换给定索引处的数组元素。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem_i
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stelem_I);
                return this;
            }
        }

        /// <summary>
        /// 用计算堆栈上的 int8 值替换给定索引处的数组元素。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem_i1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stelem_I1);
                return this;
            }
        }

        /// <summary>
        /// 用计算堆栈上的 int16 值替换给定索引处的数组元素。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem_i2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stelem_I2);
                return this;
            }
        }

        /// <summary>
        /// 用计算堆栈上的 int32 值替换给定索引处的数组元素。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem_i4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stelem_I4);
                return this;
            }
        }

        /// <summary>
        /// 用计算堆栈上的 int64 值替换给定索引处的数组元素。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem_i8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stelem_I8);
                return this;
            }
        }

        /// <summary>
        /// 用计算堆栈上的 float32 值替换给定索引处的数组元素。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem_r4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stelem_R4);
                return this;
            }
        }

        /// <summary>
        /// 用计算堆栈上的 float64 值替换给定索引处的数组元素。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem_r8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stelem_R8);
                return this;
            }
        }

        /// <summary>
        /// 用计算堆栈上的对象 ref 值（O 类型）替换给定索引处的数组元素。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stelem_ref
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stelem_Ref);
                return this;
            }
        }

        /// <summary>
        /// 用新值替换在对象引用或指针的字段中存储的值。
        /// </summary>
        /// <param name="fieldInfo">表示字段的 <see cref="FieldInfo"/>。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stfld(FieldInfo fieldInfo)
        {
            ILGenerator.Emit(OpCodes.Stfld, fieldInfo);
            return this;
        }

        /// <summary>
        /// 用新值替换在对象引用或指针的字段中存储的值。
        /// </summary>
        /// <param name="builder">构造器。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stfld(DynamicFieldBuilder builder)
        {
            ILGenerator.Emit(OpCodes.Stfld, builder.FieldBuilder);
            return this;
        }

        /// <summary>
        /// 在所提供的地址存储 native int 类型的值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind_i
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stind_I);
                return this;
            }
        }

        /// <summary>
        /// 在所提供的地址存储 int8 类型的值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind_i1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stind_I1);
                return this;
            }
        }

        /// <summary>
        /// 在所提供的地址存储 int16 类型的值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind_i2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stind_I2);
                return this;
            }
        }

        /// <summary>
        /// 在所提供的地址存储 int32 类型的值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind_i4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stind_I4);
                return this;
            }
        }

        /// <summary>
        /// 在所提供的地址存储 int64 类型的值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind_i8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stind_I8);
                return this;
            }
        }

        /// <summary>
        /// 在所提供的地址存储 float32 类型的值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind_r4
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stind_R4);
                return this;
            }
        }

        /// <summary>
        /// 在所提供的地址存储 float64 类型的值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind_r8
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stind_R8);
                return this;
            }
        }

        /// <summary>
        /// 存储所提供地址处的对象引用值。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind_ref
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stind_Ref);
                return this;
            }
        }

        /// <summary>
        /// 在所提供的地址存储指定类型的值。
        /// </summary>
        /// <param name="type">表示一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stind(Type type)
        {
            Guard.ArgumentNull(type, nameof(type));

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    stind_i1.end();
                    break;
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    stind_i2.end();
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    stind_i4.end();
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    stind_i8.end();
                    break;
                case TypeCode.Single:
                    stind_r4.end();
                    break;
                case TypeCode.Double:
                    stind_r8.end();
                    break;
                default:
                    if (type.IsClass)
                    {
                        stind_ref.end();
                    }
                    else if (type.IsValueType)
                    {
                        stobj(type);
                    }
                    else
                    {
                        throw ThrowNotExpectedTypeException(type);
                    }

                    break;
            }

            return this;
        }

        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到指定的局部变量中。
        /// </summary>
        /// <param name="local">一个局部变量。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stloc(LocalBuilder local)
        {
            ILGenerator.Emit(OpCodes.Stloc, local);
            return this;
        }

        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到指定索引处的局部变量列表中。
        /// </summary>
        /// <param name="index">表示索引的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stloc(short index)
        {
            if (index >= byte.MinValue && index <= byte.MaxValue)
            {
                return stloc_s((byte)index);
            }

            ILGenerator.Emit(OpCodes.Stloc, index);
            return this;
        }

        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到索引 0 处的局部变量列表中。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stloc_0
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stloc_0);
                return this;
            }
        }

        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到索引 1 处的局部变量列表中。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stloc_1
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stloc_1);
                return this;
            }
        }

        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到索引 2 处的局部变量列表中。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stloc_2
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stloc_2);
                return this;
            }
        }

        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到索引 3 处的局部变量列表中。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stloc_3
        {
            get
            {
                ILGenerator.Emit(OpCodes.Stloc_3);
                return this;
            }
        }

        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储在指定的局部变量（短格式）。
        /// </summary>
        /// <param name="local">一个局部变量。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stloc_s(LocalBuilder local)
        {
            ILGenerator.Emit(OpCodes.Stloc_S, local);
            return this;
        }

        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储在局部变量列表中的 index 处（短格式）。
        /// </summary>
        /// <param name="index">表示索引的值。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stloc_s(byte index)
        {
            switch (index)
            {
                case 0:
                    stloc_0.end();
                    break;
                case 1:
                    stloc_1.end();
                    break;
                case 2:
                    stloc_2.end();
                    break;
                case 3:
                    stloc_3.end();
                    break;
                default:
                    ILGenerator.Emit(OpCodes.Stloc_S, index);
                    break;
            }

            return this;
        }

        /// <summary>
        /// 将指定类型的值从计算堆栈复制到所提供的内存地址中。
        /// </summary>
        /// <param name="type">一个类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stobj(Type type)
        {
            ILGenerator.Emit(OpCodes.Stobj, type);
            return this;
        }

        /// <summary>
        /// 用来自计算堆栈的值替换静态字段的值。
        /// </summary>
        /// <param name="fieldInfo">一个表示字段的 <see cref="FieldInfo"/>。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper stsfld(FieldInfo fieldInfo)
        {
            ILGenerator.Emit(OpCodes.Stsfld, fieldInfo);
            return this;
        }

        /// <summary>
        /// 从其他值中减去一个值并将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper sub
        {
            get
            {
                ILGenerator.Emit(OpCodes.Sub);
                return this;
            }
        }

        /// <summary>
        /// 从另一值中减去一个整数值，执行溢出检查，并且将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper sub_ovf
        {
            get
            {
                ILGenerator.Emit(OpCodes.Sub_Ovf);
                return this;
            }
        }

        /// <summary>
        /// 从另一值中减去一个无符号整数值，执行溢出检查，并且将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper sub_ovf_un
        {
            get
            {
                ILGenerator.Emit(OpCodes.Sub_Ovf_Un);
                return this;
            }
        }

        /// <summary>
        /// 实现跳转表。
        /// </summary>
        /// <param name="labels">从此位置分支到的标签对象的数组。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper @switch(Label[] labels)
        {
            ILGenerator.Emit(OpCodes.Switch, labels);
            return this;
        }

        /// <summary>
        /// 执行后缀的方法调用指令，以便在执行实际调用指令前移除当前方法的堆栈帧。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper tailcall
        {
            get
            {
                ILGenerator.Emit(OpCodes.Tailcall);
                return this;
            }
        }

        /// <summary>
        /// 引发当前位于计算堆栈上的异常对象。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper @throw
        {
            get
            {
                ILGenerator.Emit(OpCodes.Throw);
                return this;
            }
        }

        /// <summary>
        /// 指示当前位于计算堆栈上的地址可能没有与紧接的 ldind、stind、ldfld、stfld、ldobj、stobj、initblk 或 cpblk 指令的自然大小对齐。
        /// </summary>
        /// <param name="label">分量的分支标签。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper unaligned(Label label)
        {
            ILGenerator.Emit(OpCodes.Unaligned, label);
            return this;
        }

        /// <summary>
        /// 指示当前位于计算堆栈上的地址可能没有与紧接的 ldind、stind、ldfld、stfld、ldobj、stobj、initblk 或 cpblk 指令的自然大小对齐。
        /// </summary>
        /// <param name="addr">表示一个栈上的地址。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper unaligned(long addr)
        {
            ILGenerator.Emit(OpCodes.Unaligned, addr);
            return this;
        }

        /// <summary>
        /// 将值类型的已装箱的表示形式转换为其未装箱的形式。
        /// </summary>
        /// <param name="type">拆箱后的类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper unbox(Type type)
        {
            ILGenerator.Emit(OpCodes.Unbox, type);
            return this;
        }

        /// <summary>
        /// 将指令中指定类型的已装箱的表示形式转换成未装箱形式。
        /// </summary>
        /// <param name="type">拆箱后的类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper unbox_any(Type type)
        {
            ILGenerator.Emit(OpCodes.Unbox_Any, type);
            return this;
        }

        /// <summary>
        /// 如果该类型是值类型，将指令中指定类型的已装箱的表示形式转换成未装箱形式，否则不做任何处理。
        /// </summary>
        /// <param name="type">拆箱后的类型。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper unboxIfValueType(Type type)
        {
            Guard.ArgumentNull(type, nameof(type));

            return type.IsValueType ? unbox_any(type) : this;
        }

        /// <summary>
        /// 指定当前位于计算堆栈顶部的地址可以是易失的，并且读取该位置的结果不能被缓存，或者对该地址的多个存储区不能被取消。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper @volatile
        {
            get
            {
                ILGenerator.Emit(OpCodes.Volatile);
                return this;
            }
        }

        /// <summary>
        /// 计算位于计算堆栈顶部的两个值的按位异或，并且将结果推送到计算堆栈上。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper xor
        {
            get
            {
                ILGenerator.Emit(OpCodes.Xor);
                return this;
            }
        }

        /// <summary>
        /// 终止符，无具体含义，用于属性后面。
        /// </summary>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper end()
        {
            return this;
        }

        /// <summary>
        /// 使用断言进行编码。
        /// </summary>
        /// <param name="predicate">用于测试条件的函数。</param>
        /// <param name="trueAction">如果条件为 true，则执行该方法。</param>
        /// <param name="falseAction">如果条件为 false，则执行该方法。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper Assert(bool predicate, Action<EmitHelper> trueAction, Action<EmitHelper> falseAction = null)
        {
            if (predicate)
            {
                trueAction?.Invoke(this);
            }
            else
            {
                falseAction?.Invoke(this);
            }

            return this;
        }

        /// <summary>
        /// 枚举序列中的所有元素，并应用指定的方法。
        /// </summary>
        /// <typeparam name="T">序列中的元素类型。</typeparam>
        /// <param name="source">一个序列。</param>
        /// <param name="action">应用 <see cref="EmitHelper"/> 实例的方法。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper Each<T>(IEnumerable<T> source, Action<EmitHelper, T, int> action)
        {
            if (source == null || action == null)
            {
                return this;
            }

            var index = 0;
            foreach (var item in source)
            {
                action(this, item, index++);
            }

            return this;
        }

        /// <summary>
        /// 循环从 <paramref name="from"/> 到 <paramref name="to"/> 之间的数字，并应用指定的方法。
        /// </summary>
        /// <param name="from">开始的数字。</param>
        /// <param name="to">结束的数字。</param>
        /// <param name="action">循环体方法。</param>
        /// <returns>当前 <see cref="EmitHelper"/> 的实例。</returns>
        public EmitHelper For(int from, int to, Action<EmitHelper, int> action)
        {
            if (action == null)
            {
                return this;
            }

            for (var i = from; i < to; i++)
            {
                action(this, i);
            }

            return this;
        }

        #endregion

        private static Exception ThrowNoMethodException(Type type, string methodName)
        {
            return new InvalidOperationException(SR.GetString(SRKind.MethodNotFound, methodName));
        }

        private static Exception ThrowNotExpectedTypeException(Type type)
        {
            return new ArgumentException(SR.GetString(SRKind.NotExpectedType, type.FullName));
        }
    }
}
