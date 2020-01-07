// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Security;
using System;
using System.Text;

namespace Fireasy.App.Licence
{
    /// <summary>
    /// 缺省的授权码管理器。
    /// </summary>
    public class DefaultLicenseManager : ILicenseManager
    {
        /// <summary>
        /// 获取本地请求码。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public string GetLocalKey(LocalKeyOption option)
        {
            var machineKey = MachineKeyHelper.GetMachineKey(option.MachineKeyKind);
            if (string.IsNullOrEmpty(machineKey))
            {
                throw new AppLicenseException($"未能获取 {option.MachineKeyKind} 的相关信息。", null);
            }

            Guard.ArgumentNull(option.AppKey, nameof(option.AppKey));

            var crypt = CryptographyFactory.Create(option.EncryptKind.ToString());
            var bytes = crypt.Encrypt($"_uY9bd{machineKey}-9b83X{option.AppKey}_5Y946Z", Encoding.ASCII);
            var data = new byte[bytes.Length + 2];
            bytes.CopyTo(data, 2);
            data[0] = (byte)option.MachineKeyKind;
            data[1] = (byte)option.EncryptKind;
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// 获取授权码。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public string GetLicenseKey(LicenceGenerateOption option)
        {
            Guard.ArgumentNull(option.PrivateKey, nameof(option.PrivateKey));
            Guard.ArgumentNull(option.LocalKey, nameof(option.LocalKey));

            var crypt = CryptographyFactory.Create(option.EncryptKind.ToString()) as AsymmetricCrypto;
            crypt.PrivateKey = option.PrivateKey;
            return Convert.ToBase64String(crypt.CreateSignature(Encoding.ASCII.GetBytes(option.LocalKey)));
        }

        /// <summary>
        /// 验证授权码是否正确。
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public bool VerifyLicenseKey(LicenceVerifyOption option)
        {
            Guard.ArgumentNull(option.PublicKey, nameof(option.PublicKey));
            Guard.ArgumentNull(option.LicenseKey, nameof(option.LicenseKey));

            if (option.VerifyLocalKey)
            {
                Guard.ArgumentNull(option.AppKey, nameof(option.AppKey));
                Guard.ArgumentNull(option.LocalKey, nameof(option.LocalKey));
            }

            var crypt = CryptographyFactory.Create(option.EncryptKind.ToString()) as AsymmetricCrypto;
            crypt.PublicKey = option.PublicKey;

            if (option.VerifyLocalKey)
            {
                var bytes = FromBase64String(option.LocalKey);
                if (bytes == null)
                {
                    return false;
                }

                var data = new byte[bytes.Length - 2];
                Array.Copy(bytes, 2, data, 0, data.Length);

                if (!Enum.IsDefined(typeof(MachineKeyKinds), (int)bytes[0]) ||
                    !Enum.IsDefined(typeof(LocalKeyEncryptKinds), (int)bytes[1]))
                {
                    return false;
                }

                var mkKind = (MachineKeyKinds)bytes[0];
                var mkEncryptKind = (LocalKeyEncryptKinds)bytes[1];
                var localKey = GetLocalKey(new LocalKeyOption
                {
                    AppKey = option.AppKey,
                    MachineKeyKind = mkKind,
                    EncryptKind = mkEncryptKind
                });

                if (localKey != option.LocalKey)
                {
                    return false;
                }
            }

            var licenseBytes = FromBase64String(option.LicenseKey);
            if (licenseBytes == null)
            {
                return false;
            }

            return crypt.VerifySignature(Encoding.ASCII.GetBytes(option.LocalKey), licenseBytes);
        }

        private byte[] FromBase64String(string key)
        {
            try
            {
                return Convert.FromBase64String(key);
            }
            catch
            {
                return null;
            }
        }
    }
}
