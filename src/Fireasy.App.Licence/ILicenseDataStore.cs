// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.App.Licence
{
    /// <summary>
    /// 提供授权数据的存储方法。
    /// </summary>
    public interface ILicenseDataStore
    {
        /// <summary>
        /// 加载授权数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appKey">应用标识。</param>
        /// <returns></returns>
        T Load<T>(string appKey) where T : LicenceData;

        /// <summary>
        /// 保存授权数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appKey">应用标识。</param>
        /// <param name="data">授权数据。</param>
        void Save<T>(string appKey, T data) where T : LicenceData;
    }
}
