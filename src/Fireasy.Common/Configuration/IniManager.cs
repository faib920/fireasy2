// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Fireasy.Common.Configuration
{
    public class IniManager
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        private readonly string _fileName;

        public IniManager(string fileName)
        {
            _fileName = fileName;
        }

        public T Get<T>(string section, string key)
        {
            var buffer = new byte[65535];
            var bufLen = GetPrivateProfileString(section, key, string.Empty, buffer, buffer.GetUpperBound(0), _fileName);

            //必须设定0（系统默认的代码页）的编码方式，否则无法支持中文
            var str = Encoding.GetEncoding(0).GetString(buffer);
            str = str.Substring(0, bufLen);
            return str.Trim().To<T>();
        }

        public void Set<T>(string section, string key, T value)
        {
            if (!WritePrivateProfileString(section, key, value.ToStringSafely(), _fileName))
            {
                throw (new ApplicationException());
            }
        }
    }
}
