using System;

namespace Fireasy.Data.Entity.Tests
{
    public class CodedDataConverter : Fireasy.Data.Converter.CodedDataConverter
    {
        protected override CodedData DecodeDataFromBytes(byte[] data)
        {
            throw new NotImplementedException();
        }

        protected override byte[] EncodeDataToBytes(CodedData data)
        {
            throw new NotImplementedException();
        }
    }
}
