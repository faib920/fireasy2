using Fireasy.Common;

namespace Fireasy.Data.Entity.Tests.Models
{

    public enum RecorderLevel
    {
        [EnumDescription("一般")]
        A,
        [EnumDescription("特级")]
        B = 30,
        [EnumDescription("低等")]
        C
    }
}