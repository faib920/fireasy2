// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Aop;
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// 提供对 <see cref="ITreeNode"/> 类型的 Json 转换。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeNodeJsonConverter<T> : JsonConverter where T : ITreeNode
    {
        private Dictionary<string, Func<T, object>> attrMappers = new Dictionary<string, Func<T, object>>();

        /// <summary>
        /// 初始化 <see cref="TreeNodeJsonConverter&lt;T&gt;"/> 类的新实例。
        /// </summary>
        public TreeNodeJsonConverter()
        {
        }

        /// <summary>
        /// 使用一组属性映射初始化 <see cref="TreeNodeJsonConverter&lt;T&gt;"/> 类的新实例。
        /// </summary>
        /// <param name="attrMappers"></param>
        public TreeNodeJsonConverter(Dictionary<string, Func<T, object>> attrMappers)
        {
            this.attrMappers = attrMappers;
        }

        /// <summary>
        /// 判断指定的类型是否可以转换。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool CanConvert(Type type)
        {
            return typeof(T).IsAssignableFrom(type) || typeof(T) == type;
        }

        /// <summary>
        /// 返回不支持读。
        /// </summary>
        public override bool CanRead { get; } = false;

        /// <summary>
        /// 将对象写为 Json 文本。
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override string WriteJson(JsonSerializer serializer, object obj)
        {
            var node = obj as ITreeNode;
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"id\": \"{0}\",", node.Id);

            foreach (var map in attrMappers)
            {
                sb.AppendFormat("\"{0}\": {1},", map.Key, serializer.Serialize(map.Value((T)obj)));
            }

            if (node.HasChildren || node.IsLoaded)
            {
                var state = WriteState(node);
                if (!string.IsNullOrEmpty(state))
                {
                    sb.Append(state + ",");
                }
            }

            var icon = WriteIconCls(node);
            if (!string.IsNullOrEmpty(icon))
            {
                sb.Append(icon + ",");
            }

            sb.AppendFormat("\"children\": {0}", serializer.Serialize(node.Children));
            sb.Append("}");

            return sb.ToString();
        }

        protected virtual string WriteState(ITreeNode node)
        {
            return string.Format("\"state\": \"{0}\"", node.IsLoaded ? "open" : "closed");
        }

        protected virtual string WriteIconCls(ITreeNode node)
        {
            return string.Empty;
        }
    }
}
