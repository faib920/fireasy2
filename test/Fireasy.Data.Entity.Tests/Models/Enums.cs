using Fireasy.Common;

namespace Fireasy.Data.Entity.Tests.Models
{

    public enum RecorderLevel
    {
        [EnumDescription("һ��")]
        A,
        [EnumDescription("�ؼ�")]
        B = 30,
        [EnumDescription("�͵�")]
        C
    }
}