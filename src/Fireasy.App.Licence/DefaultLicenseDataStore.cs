// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Security;
using Fireasy.Common.Serialization;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace Fireasy.App.Licence
{
    /// <summary>
    /// 缺省的授权数据存储方法。
    /// </summary>
    public class DefaultLicenseDataStore : ILicenseDataStore
    {
        /// <summary>
        /// 加载授权数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appKey">应用标识。</param>
        /// <returns></returns>
        public T Load<T>(string appKey) where T : LicenceData
        {
            var enKey = EncryptKey(appKey);
            var bin = CreateSerializer();

            try
            {
                var fileName = enKey + ".dat";
                using (var iso = IsolatedStorageFile.GetMachineStoreForDomain())
                {
                    if (!iso.FileExists(fileName))
                    {
                        return default(T);
                    }

                    return Deserialize<T>(iso, fileName, bin);
                }
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 保存授权数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appKey">应用标识。</param>
        /// <param name="data">授权数据。</param>
        public void Save<T>(string appKey, T data) where T : LicenceData
        {
            var enKey = EncryptKey(appKey);
            var bin = CreateSerializer();

            try
            {
                var fileName = enKey + ".dat";
                using (var iso = IsolatedStorageFile.GetMachineStoreForDomain())
                {
                    Serialize(iso, fileName, bin, data);
                }
            }
            catch (Exception exp)
            {
                throw new AppLicenseException("无法保存授权文件。", exp);
            }
        }

        private void Serialize(IsolatedStorageFile file, string fileName, IBinarySerializer bin, LicenceData data)
        {
            using (var stream = file.CreateFile(fileName))
            {
                var local = new LocalData 
                { 
                    Temp = new byte[new Random().Next(677, 2322)], 
                    License = bin.Serialize(data)
                };

                new Random().NextBytes(local.Temp);
                var bytes = bin.Serialize(local);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private T Deserialize<T>(IsolatedStorageFile file, string fileName, IBinarySerializer bin)
        {
            using (var stream = file.OpenFile(fileName, FileMode.Open))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                var data = bin.Deserialize<LocalData>(buffer);
                if (data != null)
                {
                    return bin.Deserialize<T>(data.License);
                }
            }

            return default(T);
        }

        private IBinarySerializer CreateSerializer()
        {
            var bin = new BinaryCryptoSerializer(CreateCryptoProvider());
            bin.Token = new SerializeToken { Data = Encoding.Default.GetBytes("~@@~") };
            return bin;
        }

        private ICryptoProvider CreateCryptoProvider()
        {
            var keybuffer = new byte[] { 45, 66, 124, 187, 22, 56, 90, 242 };
            var crypt = CryptographyFactory.Create(CryptoAlgorithm.DES);
            (crypt as SymmetricCrypto).CryptKey = keybuffer;
            return crypt;
        }

        private static string EncryptKey(string key)
        {
            var crypt = CryptographyFactory.Create(CryptoAlgorithm.SHA1);
            return crypt.Encrypt(key, Encoding.UTF8).ToHex();
        }

        [Serializable]
        private class LocalData
        {
            public byte[] Temp { get; set; }

            public byte[] License { get; set; }
        }
    }
}
