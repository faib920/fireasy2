using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Tests.Models
{
    [EntityMapping("products", Description = "产品表")]
    [Serializable]
    public class OldProduct : EntityObject
    {
    public readonly static IProperty _ProductId = PropertyUnity.RegisterProperty<OldProduct>(s => s.ProductId,
        new PropertyMapInfo
        {
            ColumnName = "PRODUCTID",
            GenerateType = IdentityGenerateType.AutoIncrement,
            IsPrimaryKey = true,
            IsNullable = false
        });
    public readonly static IProperty _ProductName = PropertyUnity.RegisterProperty<OldProduct>(s => s.ProductName,
        new PropertyMapInfo
        {
            Length = 10,
            ColumnName = "PRODUCTNAME"
        });
    public readonly static IProperty _SupplierID = PropertyUnity.RegisterProperty<OldProduct>(s => s.SupplierID,
        new PropertyMapInfo
        {
            ColumnName = "SUPPLIERID"
        });
    public readonly static IProperty _CategoryID = PropertyUnity.RegisterProperty<OldProduct>(s => s.CategoryID,
        new PropertyMapInfo
        {
            ColumnName = "CATEGORYID"
        });
    public readonly static IProperty _QuantityPerUnit = PropertyUnity.RegisterProperty<OldProduct>(s => s.QuantityPerUnit,
        new PropertyMapInfo
        {
            ColumnName = "QUANTITYPERUNIT"
        });
    public readonly static IProperty _UnitPrice = PropertyUnity.RegisterProperty<OldProduct>(s => s.UnitPrice,
        new PropertyMapInfo
        {
            ColumnName = "UNITPRICE"
        });
    public readonly static IProperty _UnitsInStock = PropertyUnity.RegisterProperty<OldProduct>(s => s.UnitsInStock,
        new PropertyMapInfo
        {
            ColumnName = "UNITSINSTOCK"
        });
    public readonly static IProperty _UnitsOnOrder = PropertyUnity.RegisterProperty<OldProduct>(s => s.UnitsOnOrder,
        new PropertyMapInfo
        {
            ColumnName = "UNITSONORDER"
        });
    public readonly static IProperty _ReorderLevel = PropertyUnity.RegisterProperty<OldProduct>(s => s.ReorderLevel,
        new PropertyMapInfo
        {
            ColumnName = "REORDERLEVEL"
        });
    public readonly static IProperty _Discontinued = PropertyUnity.RegisterProperty<OldProduct>(s => s.Discontinued,
        new PropertyMapInfo
        {
            ColumnName = "DISCONTINUED",
            DefaultValue = true
        });
    public readonly static IProperty _OrderDetails = PropertyUnity.RegisterSupposedProperty<OldProduct>(s => s.OrderDetails);

    public int ProductId
    {
        get { return (int)GetValue(_ProductId); }
        set { SetValue(_ProductId, value); }
    }
    public string ProductName
    {
        get { return (string)GetValue(_ProductName); }
        set { SetValue(_ProductName, value); }
    }
    public int? SupplierID
    {
        get { return (int?)GetValue(_SupplierID); }
        set { SetValue(_SupplierID, value); }
    }
    public int? CategoryID
    {
        get { return (int?)GetValue(_CategoryID); }
        set { SetValue(_CategoryID, value); }
    }
    public string QuantityPerUnit
    {
        get { return (string)GetValue(_QuantityPerUnit); }
        set { SetValue(_QuantityPerUnit, value); }
    }
    public decimal? UnitPrice
    {
        get { return (decimal?)GetValue(_UnitPrice); }
        set { SetValue(_UnitPrice, value); }
    }
    public short? UnitsInStock
    {
        get { return (short?)GetValue(_UnitsInStock); }
        set { SetValue(_UnitsInStock, value); }
    }
    public short? UnitsOnOrder
    {
        get { return (short?)GetValue(_UnitsOnOrder); }
        set { SetValue(_UnitsOnOrder, value); }
    }
    public short? ReorderLevel
    {
        get { return (short?)GetValue(_ReorderLevel); }
        set { SetValue(_ReorderLevel, value); }
    }
    public bool? Discontinued
    {
        get { return (bool?)GetValue(_Discontinued); }
        set { SetValue(_Discontinued, value); }
    }
    public EntitySet<OrderDetails> OrderDetails
    {
        get { return PropertyValue.GetValue(GetValue(_OrderDetails)) as EntitySet<OrderDetails>; }
        set { SetValue(_OrderDetails, PropertyValue.NewValue(value)); }
    }
    }

    [EntityMapping("PRODUCTS")]
    [Serializable]
    public class ProductEx : OldProduct
    {
        public readonly static IProperty _Descript = PropertyUnity.RegisterProperty<ProductEx>(s => s.Descript, new PropertyMapInfo { ColumnName = "Descript", IsNullable = false });

        public string Descript
        {
            get { return (string)GetValue(_Descript); }
            set { SetValue(_Descript, value); }
        }
    }

}
