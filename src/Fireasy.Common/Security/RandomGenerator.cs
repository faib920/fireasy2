﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Security.Cryptography;

namespace Fireasy.Common.Security
{
    public static class RandomGenerator
    {
        private static readonly char[] _encodings = new char[] {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static string Create()
        {
            using var randgen = new RNGCryptoServiceProvider();
            var bytes = new byte[15];
            randgen.GetBytes(bytes);
            return Encode(bytes);
        }

        private static string Encode(byte[] buffer)
        {
            char[] chArray = new char[24];
            int num2 = 0;
            for (int i = 0; i < 15; i += 5)
            {
                int num4 = ((buffer[i] | (buffer[i + 1] << 8)) | (buffer[i + 2] << 16)) | (buffer[i + 3] << 24);
                int index = num4 & 61;
                chArray[num2++] = _encodings[index];
                index = (num4 >> 5) & 61;
                chArray[num2++] = _encodings[index];
                index = (num4 >> 10) & 61;
                chArray[num2++] = _encodings[index];
                index = (num4 >> 15) & 61;
                chArray[num2++] = _encodings[index];
                index = (num4 >> 20) & 61;
                chArray[num2++] = _encodings[index];
                index = (num4 >> 0x19) & 61;
                chArray[num2++] = _encodings[index];
                num4 = ((num4 >> 30) & 3) | (buffer[i + 4] << 2);
                index = num4 & 61;
                chArray[num2++] = _encodings[index];
                index = (num4 >> 5) & 61;
                chArray[num2++] = _encodings[index];
            }
            return new string(chArray);
        }
    }
}
