using System;
using System.Collections.Generic;
using System.Text;

namespace Fireasy.Data.Entity.Tests.Models
{
    public class OperateLog : LightEntity<OperateLog>
    {
        [PropertyMapping(IsPrimaryKey = true)]
        public virtual int LogId { get; set; }

        public virtual string CompanyCode { get; set; }

        public virtual string CompanyName { get; set; }

        public virtual string UserName { get; set; }
    }
}
