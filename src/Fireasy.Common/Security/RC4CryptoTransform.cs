// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Security.Cryptography;

namespace Fireasy.Common.Security
{
    internal class RC4CryptoTransform : ICryptoTransform
    {
        private readonly byte[] _key;
        private readonly int _length;
        private readonly byte[] _permutation;
        private byte _index1;
        private byte _index2;

        public RC4CryptoTransform(byte[] key)
        {
            _key = (byte[])key.Clone();
            _length = key.Length;
            _permutation = new byte[256];
            Init();
        }

        public bool CanReuseTransform
        {
            get { return true; }
        }

        public bool CanTransformMultipleBlocks
        {
            get { return true; }
        }

        public int InputBlockSize
        {
            get { return 1; }
        }

        public int OutputBlockSize
        {
            get { return 1; }
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            byte j, temp;
            var length = inputOffset + inputCount;
            for (; inputOffset < length; inputOffset++, outputOffset++)
            {
                _index1 = (byte)((_index1 + 1) % 256);
                _index2 = (byte)((_index2 + _permutation[_index1]) % 256);

                temp = _permutation[_index1];
                _permutation[_index1] = _permutation[_index2];
                _permutation[_index2] = temp;

                j = (byte)((_permutation[_index1] + _permutation[_index2]) % 256);
                outputBuffer[outputOffset] = (byte)(inputBuffer[inputOffset] ^ _permutation[j]);
            }
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var ret = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, ret, 0);
            Init();

            return ret;
        }

        public void Dispose()
        {
        }

        private void Init()
        {
            byte temp;

            for (int i = 0; i < 256; i++)
            {
                _permutation[i] = (byte)i;
            }

            _index1 = 0;
            _index2 = 0;

            for (int j = 0, i = 0; i < 256; i++)
            {
                j = (j + _permutation[i] + _key[i % _length]) % 256;

                temp = _permutation[i];
                _permutation[i] = _permutation[j];
                _permutation[j] = temp;
            }
        }
    }
}
