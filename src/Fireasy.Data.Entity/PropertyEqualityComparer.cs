// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 定义方法以支持属性的对等比较。
    /// </summary>
    public class PropertyEqualityComparer : IEqualityComparer<IProperty>
    {
        /// <summary>
        /// 
        /// </summary>
        public static PropertyEqualityComparer Default = new PropertyEqualityComparer();

        /// <summary>
        /// 确定两个属性对象是否相等。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public virtual bool Equals(IProperty x, IProperty y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            if (x.GetType() != y.GetType())
            {
                return false;
            }
            return x.Name.Equals(y.Name);
        }

        /// <summary>
        /// 获取指定属性对象的哈希码。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual int GetHashCode(IProperty obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
