// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Security
{
    /// <summary>
    /// 支持的加密算法。
    /// </summary>
    public enum CryptoAlgorithm
    {
        /// <summary>
        /// DES 对称加密算法。
        /// </summary>
        DES,

        /// <summary>
        /// AES 对称加密算法。
        /// </summary>
        AES,

        /// <summary>
        /// MD5 哈希加密算法。
        /// </summary>
        MD5,

        /// <summary>
        /// RC2 对称加密算法。
        /// </summary>
        RC2,

        /// <summary>
        /// RC4 对称加密算法。
        /// </summary>
        RC4,

        /// <summary>
        /// Rijndael 对称加密算法。
        /// </summary>
        Rijndael,

        /// <summary>
        /// SHA1 哈希加密算法。
        /// </summary>
        SHA1,

        /// <summary>
        /// SHA256 哈希加密算法。
        /// </summary>
        SHA256,

        /// <summary>
        /// SHA384 哈希加密算法。
        /// </summary>
        SHA384,

        /// <summary>
        /// SHA512 哈希加密算法。
        /// </summary>
        SHA512,

        /// <summary>
        /// RSA 非对称加密算法。
        /// </summary>
        RSA,

        /// <summary>
        /// DSA 非对称加密算法。
        /// </summary>
        DSA,

        /// <summary>
        /// 三重数据 DES 对称加密算法。
        /// </summary>
        TripleDES
    }
}
