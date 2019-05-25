using Fireasy.Common;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace Fireasy.Windows.Forms
{
    public partial class TreeList
    {
        private object dataSource;

        /// <summary>
        /// 将一个数据对象绑定到 <see cref="TreeList"/>
        /// </summary>
        /// <param name="dataSource"></param>
        public void DataBind(object dataSource)
        {
            this.dataSource = dataSource;

            BeginUpdate();

            var selectKeyValues = GetSelectedItems();

            Items.Clear();
            SelectedItems.InternalClear();
            CheckedItems.InternalClear();

            if (dataSource == null)
            {
                EndUpdate();
                return;
            }

            if (dataSource is DataSet)
            {
                var ds = (DataSet)dataSource;
                if (ds.Tables.Count > 0)
                {
                    BindDataTable(ds.Tables[0]);
                }
            }
            if (dataSource is DataTable)
            {
                BindDataTable((DataTable)dataSource);
            }
            else if (dataSource is IEnumerable)
            {
                BindEnumerable((IEnumerable)dataSource);
            }

            EndUpdate();

            ReSelectItems(selectKeyValues);
        }

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        public object DataSource
        {
            get { return dataSource; }
            set
            {
                dataSource = value;
                DataBind(dataSource);
            }
        }

        /// <summary>
        /// 把选中项记录下来。
        /// </summary>
        /// <returns></returns>
        private object[] GetSelectedItems()
        {
            if (KeepSelectedItems && !string.IsNullOrEmpty(KeyField))
            {
                return SelectedItems.Where(s => s.KeyValue != null).Select(s => s.KeyValue).ToArray();
            }

            return null;
        }

        /// <summary>
        /// 重新选中选择项。
        /// </summary>
        /// <param name="selectKeyValues"></param>
        private void ReSelectItems(object[] selectKeyValues)
        {
            if (selectKeyValues != null)
            {
                foreach (var vitem in virMgr.Items)
                {
                    var item = vitem.Item as TreeListItem;
                    if (item != null && 
                        item.KeyValue != null && 
                        selectKeyValues.FirstOrDefault(s => s.Equals(item.KeyValue)) != null)
                    {
                        item.Selected = true;
                    }
                }
            }
        }

        /// <summary>
        /// 绑定DataTable对象。
        /// </summary>
        /// <param name="table"></param>
        private void BindDataTable(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {

            }
        }

        /// <summary>
        /// 绑定枚举类型的数据源。
        /// </summary>
        /// <param name="enumerable"></param>
        private void BindEnumerable(IEnumerable enumerable)
        {
            var index = 0;
            foreach (var item in enumerable)
            {
                if (item == null)
                {
                    continue;
                }

                var definition = GetBindingDefinition(item);

                var listitem = new TreeListItem();
                Items.Add(listitem);

                BindListItem(listitem, item, definition, index);

                RaiseItemDataBoundEvent(listitem, item, index++);
            }
        }

        private void BindListItem(TreeListItem listitem, object item, ObjectBindingDefinition definition, int index)
        {
            if (definition.PrimaryProperty != null)
            {
                listitem.KeyValue = definition.PrimaryProperty.GetValue(item);
            }

            var p = 0;
            foreach (var property in definition.Properties)
            {
                if (property != null)
                {
                    var value = property.GetValue(item);
                    listitem.Cells[p].Value = value;

                    RaiseCellDataBoundEvent(listitem.Cells[p], item, value);
                }

                p++;
            }
        }

        /// <summary>
        /// 获取能够绑定到 <see cref="TreeList"/> 上的数据对象的属性集合。
        /// </summary>
        /// <param name="element">序列中的元素。</param>
        /// <returns></returns>
        private ObjectBindingDefinition GetBindingDefinition(object element)
        {
            var binding = new List<PropertyDescriptor>();
            var properties = new List<PropertyDescriptor>();
            PropertyDescriptor primary = null;

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(element))
            {
                properties.Add(pd);
            }

            primary = FindProperty(properties, KeyField);

            foreach (var column in Columns)
            {
                binding.Add(FindProperty(properties, column.DataKey));
            }

            return new ObjectBindingDefinition(primary, binding);
        }

        private PropertyDescriptor FindProperty(List<PropertyDescriptor> properties, string name)
        {
            return string.IsNullOrEmpty(name) ? null : 
                properties.FirstOrDefault(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        private class ObjectBindingDefinition
        {
            public ObjectBindingDefinition(PropertyDescriptor primary, IList<PropertyDescriptor> properties)
            {
                PrimaryProperty = primary;
                Properties = properties;
            }

            public PropertyDescriptor PrimaryProperty { get; set; }

            public IList<PropertyDescriptor> Properties { get; set; }
        }
    }
}
