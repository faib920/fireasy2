// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 提供数据库实例配置的路由服务支持
    /// </summary>
    public interface IInstanceRouteService
    {
        /// <summary>
        /// 注册路由的匹配路径。
        /// </summary>
        /// <param name="match">匹配路径。</param>
        void RegisterRoute(string match);

        /// <summary>
        /// 获取路由的匹配路径。
        /// </summary>
        /// <returns>匹配路径。</returns>
        string GetRoute();

        /// <summary>
        /// 根据匹配路径从路由服务中获取实例配置。
        /// </summary>
        /// <returns></returns>
        IInstanceConfigurationSetting GetSetting();
    }
}
