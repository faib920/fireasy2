// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 为订阅管理器提供的通知方法。
    /// </summary>
    public interface ISubscribeNotification
    {
        /// <summary>
        /// 当发布主题失败时，通过该方法决定后续是否将主题进行持久化，再由定时器进行重新发布。
        /// </summary>
        /// <param name="context"></param>
        void OnPublishError(SubscribeNotificationContext context);

        /// <summary>
        /// 当主题消费失败时，通过该方法决定是否将主题重新推入到队列。
        /// </summary>
        /// <returns></returns>
        /// <param name="context"></param>
        void OnConsumeError(SubscribeNotificationContext context);
    }
}
