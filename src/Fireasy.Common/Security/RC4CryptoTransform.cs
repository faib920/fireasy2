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
        private byte[] key;
        private int kenLength;
        private byte[] permutation;
        private byte index1;
        private byte index2;

        public RC4CryptoTransform(byte[] key)
        {
			this.key = (byte[])key.Clone();
			kenLength = key.Length;
			permutation = new byte[256];
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
                index1 = (byte)((index1 + 1) % 256);
                index2 = (byte)((index2 + permutation[index1]) % 256);

                temp = permutation[index1];
                permutation[index1] = permutation[index2];
                permutation[index2] = temp;

                j = (byte)((permutation[index1] + permutation[index2]) % 256);
                outputBuffer[outputOffset] = (byte)(inputBuffer[inputOffset] ^ permutation[j]);
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
                permutation[i] = (byte)i;
            }

            index1 = 0;
            index2 = 0;

            for (int j = 0, i = 0; i < 256; i++)
            {
                j = (j + permutation[i] + key[i % kenLength]) % 256;

                temp = permutation[i];
                permutation[i] = permutation[j];
                permutation[j] = temp;
            }
        }
    }
}
