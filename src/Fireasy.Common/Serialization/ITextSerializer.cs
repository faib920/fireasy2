// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 提供对象的文本序列化与反序列化方法。
    /// </summary>
    public interface ITextSerializer
    {
        /// <summary>
        /// 将对象转换为使用文本表示。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>表示对象的文本。</returns>
        string Serialize<T>(T value);

        /// <summary>
        /// 从文本中解析出类型 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content">表示对象的文本。</param>
        /// <returns>解析后的对象。</returns>
        T Deserialize<T>(string content);

        /// <summary>
        /// 从文本中解析出类型 <typeparamref name="T"/> 的对象，<typeparamref name="T"/> 可以是匿名类型。
        /// </summary>
        /// <typeparam name="T">自定义匿名类型。</typeparam>
        /// <param name="content">表示对象的文本</param>
        /// <param name="anyObj">为构造 <typeparamref name="T"/> 类型而初始化的对象。</param>
        /// <returns>解析后的对象。</returns>
        T Deserialize<T>(string content, T anyObj);

        /// <summary>
        /// 从文本中解析出类型 <paramref name="type"/> 的对象。
        /// </summary>
        /// <param name="content">表示对象的文本。</param>
        /// <param name="type">可序列化的对象类型。</param>
        /// <returns>对象。</returns>
        object Deserialize(string content, Type type);
        
        /// <summary>
        /// 异步方式将对象转换为使用文本表示。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>表示对象的文本。</returns>
        Task<string> SerializeAsync<T>(T value);

        /// <summary>
        /// 异步方式从文本中解析出类型 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content">表示对象的文本。</param>
        /// <returns>解析后的对象。</returns>
        Task<T> DeserializeAsync<T>(string content);

        /// <summary>
        /// 异步方式从文本中解析出类型 <typeparamref name="T"/> 的对象，<typeparamref name="T"/> 可以是匿名类型。
        /// </summary>
        /// <typeparam name="T">自定义匿名类型。</typeparam>
        /// <param name="content">表示对象的文本</param>
        /// <param name="anyObj">为构造 <typeparamref name="T"/> 类型而初始化的对象。</param>
        /// <returns>解析后的对象。</returns>
        Task<T> DeserializeAsync<T>(string content, T anyObj);

        /// <summary>
        /// 异步方式从文本中解析出类型 <paramref name="type"/> 的对象。
        /// </summary>
        /// <param name="content">表示对象的文本。</param>
        /// <param name="type">可序列化的对象类型。</param>
        /// <returns>对象。</returns>
        Task<object> DeserializeAsync(string content, Type type);
    }

    /// <summary>
    /// 表示限定 <typeparamref name="TOption"/> 的文本序列化与反序列化方法。
    /// </summary>
    /// <typeparam name="TOption"></typeparam>
    public interface ITextSerializer<TOption> : ITextSerializer where TOption : SerializeOption
    {
    }
}
