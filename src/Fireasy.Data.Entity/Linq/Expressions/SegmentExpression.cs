// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示对数据进行分段查询的表达式。
    /// </summary>
    public class SegmentExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="SegmentExpression"/> 类的新实例。
        /// </summary>
        /// <param name="segment">数据分段对象。</param>
        public SegmentExpression(IDataSegment segment)
            : base(DbExpressionType.Segment)
        {
            Segment = segment;
        }

        /// <summary>
        /// 获取数据分段对象。
        /// </summary>
        public IDataSegment Segment { get; private set; }
    }
}
