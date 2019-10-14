// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Schema.Linq
{
    /// <summary>
    /// 使用非法的限制查询表达式时抛出的异常。
    /// </summary>
    public class SchemaQueryTranslateException : Exception
    {
        /// <summary>
        /// 实例化类 <see cref="SchemaQueryTranslateException"/> 的新实例，表示指定的 <see cref="MemberInfo"/> 不支持限制查询。
        /// </summary>
        /// <param name="currMember">当前使用的 <see cref="MemberInfo"/> 对象。</param>
        /// <param name="mbrRestrs">提供限制查询的 <see cref="MemberInfo"/> 列表。</param>
        public SchemaQueryTranslateException(MemberInfo currMember, IEnumerable<MemberInfo> mbrRestrs)
            : base (SR.GetString(SRKind.RestrictionNotSupportedOfMember, currMember.Name, GetMemberNames(mbrRestrs)))
        {
        }

        /// <summary>
        /// 实例化类 <see cref="SchemaQueryTranslateException"/> 的新实例，表示此元数据不支持限制查询。
        /// </summary>
        /// <param name="metadataType"></param>
        public SchemaQueryTranslateException(Type metadataType)
            : base(SR.GetString(SRKind.RestrictionNotSupportedOfMetadata, metadataType.Name))
        {
        }

        static string GetMemberNames(IEnumerable<MemberInfo> mbrRestrs)
        {
            return string.Join(",", mbrRestrs.Select(s => s.Name));
        }
    }
}
