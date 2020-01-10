// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// ELinq 表达式的翻译选项。无法继承此类。
    /// </summary>
    public sealed class TranslateOptions
    {
        /// <summary>
        /// 缺省的选项。
        /// </summary>
        public readonly static TranslateOptions Default = new TranslateOptions();

        /// <summary>
        /// 获取或设置是否隐藏列的别名。
        /// </summary>
        public bool HideColumnAliases { get; set; }

        /// <summary>
        /// 获取或设置是否隐藏表的别名。
        /// </summary>
        public bool HideTableAliases { get; set; }

        /// <summary>
        /// 获取或设置是否返回查询参数。默认为 false。
        /// </summary>
        public bool AttachParameter { get; set; }

        /// <summary>
        /// 获取或设置是否仅仅生成条件。
        /// </summary>
        public bool WhereOnly { get; set; }

        /// <summary>
        /// 获取或设置是否开启解析缓存。默认为 true。
        /// </summary>
        public bool CacheParsing { get; set; } = true;

        /// <summary>
        /// 获取或设置解析缓存过期时间(秒)。默认为600。
        /// </summary>
        public int CacheParsingTimes { get; set; } = 600;

        /// <summary>
        /// 获取或设置是否开启数据缓存。默认为 false。
        /// </summary>
        public bool CacheExecution { get; set; }

        /// <summary>
        /// 获取或设置数据缓存过期时间(秒)。默认为300。
        /// </summary>
        public int CacheExecutionTimes { get; set; } = 300;

        /// <summary>
        /// 获取或设置是否跟踪返回的实体的状态。默认为 true。
        /// </summary>
        public bool TraceEntityState { get; set; } = true;

        /// <summary>
        /// 克隆一份选项。
        /// </summary>
        /// <returns></returns>
        public TranslateOptions Clone()
        {
            return new TranslateOptions
                {
                    AttachParameter = AttachParameter,
                    HideTableAliases = HideTableAliases,
                    HideColumnAliases = HideColumnAliases,
                    CacheParsing = CacheParsing,
                    CacheParsingTimes = CacheParsingTimes,
                    CacheExecution = CacheExecution,
                    CacheExecutionTimes = CacheExecutionTimes,
                    WhereOnly = WhereOnly,
                    TraceEntityState = TraceEntityState
                };
        }
    }
}
