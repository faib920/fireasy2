// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data
{
    /// <summary>
    /// 在使用 <see cref="DataPager"/> 对象前对数据量进行评估，以确定数据的分页方式。
    /// </summary>
    public interface IDataPageEvaluatable
    {
        /// <summary>
        /// 获取或设置评估器。
        /// </summary>
        IDataPageEvaluator Evaluator { get; set; }
    }
}
