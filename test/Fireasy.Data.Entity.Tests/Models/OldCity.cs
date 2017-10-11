using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Tests.Models
{
    [EntityMapping("City")]
    [EntityTreeMapping(
        InnerSign = "InnerID",
        Name = "Name",
        FullName = "FullName",
        Order = "Order",
        Level = "Level")]
    public class City : EntityObject
    {
        public readonly static IProperty _ID = PropertyUnity.RegisterProperty("ID",
            typeof(string), typeof(City),
            new PropertyMapInfo
            {
                FieldName = "id",
                GenerateType = IdentityGenerateType.Generator,
                IsPrimaryKey = true
            });
        public readonly static IProperty _InnerID = PropertyUnity.RegisterProperty("InnerID",
            typeof(string), typeof(City), new PropertyMapInfo
            {
                FieldName = "innerid"
            });
        public readonly static IProperty _Name = PropertyUnity.RegisterProperty("Name",
            typeof(string), typeof(City), new PropertyMapInfo
            {
                FieldName = "name",
                Length = 30
            });
        public readonly static IProperty _FullName = PropertyUnity.RegisterProperty("FullName",
            typeof(string), typeof(City), new PropertyMapInfo
            {
                FieldName = "fullname"
            });
        public readonly static IProperty _Level = PropertyUnity.RegisterProperty("Level",
            typeof(short), typeof(City), new PropertyMapInfo
            {
                FieldName = "level"
            });
        public readonly static IProperty _Order = PropertyUnity.RegisterProperty("Order",
            typeof(short), typeof(City), new PropertyMapInfo
            {
                FieldName = "order"
            });

        public string ID
        {
            get { return (string)GetValue(_ID); }
            set { SetValue(_ID, value); }
        }

        public string InnerID
        {
            get { return (string)GetValue(_InnerID); }
            set { SetValue(_InnerID, value); }
        }

        public string Name
        {
            get { return (string)GetValue(_Name); }
            set { SetValue(_Name, value); }
        }

        public string FullName
        {
            get { return (string)GetValue(_FullName); }
            set { SetValue(_FullName, value); }
        }

        public short Level
        {
            get { return (short)GetValue(_Level); }
            set { SetValue(_Level, value); }
        }

        public short Order
        {
            get { return (short)GetValue(_Order); }
            set { SetValue(_Order, value); }
        }
    }
}
