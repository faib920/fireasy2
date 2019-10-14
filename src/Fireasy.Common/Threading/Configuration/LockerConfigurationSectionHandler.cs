// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD
using Fireasy.Common.Configuration;

namespace Fireasy.Common.Threading.Configuration
{
    /// <summary>
    /// 提供对锁的配置节的处理。
    /// </summary>
    public sealed class LockerConfigurationSectionHandler : ConfigurationSectionHandler<LockerConfigurationSection>
    {
    }
}
#endif