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
using System.Data;
using Fireasy.Data.Extensions;
using System.ComponentModel;
using Fireasy.Common;

namespace Fireasy.Data
{
    /// <summary>
    /// 查询参数集合。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class ParameterCollection : IList<Parameter>, 
        ICloneable
    {
        private readonly ArrayList arrayList;

        /// <summary>
        /// 使用对象数组实例化参数集合，该实例化方法产生的参数将以'p'及索引进行命名。
        /// </summary>
        /// <param name="parameters">参数值数组。</param>
        public ParameterCollection(params object[] parameters)
        {
            arrayList = new ArrayList();

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    Add("p" + i, parameters[i]);
                }
            }
        }

        /// <summary>
        /// 使用一个参数集合初始化类的新实例。
        /// </summary>
        /// <param name="parameters">一个 <see cref="ParameterCollection"/> 对象。</param>
        public ParameterCollection(IEnumerable<Parameter> parameters)
        {
            arrayList = new ArrayList();

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    Add(p.ParameterName, p.Value);
                }
            }
        }

        /// <summary>
        /// 使用一个对象初始化参数集合，该实例化方法产生的参数将以对象的属性名称进行命名。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ParameterCollection(object obj)
        {
            Guard.ArgumentNull(obj, "obj");
            arrayList = new ArrayList();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj))
            {
                Add(property.Name, property.GetValue(obj));
            }
        }

        /// <summary>
        /// 使用一个字典对象初始化参数集合，该实例化方法产生的参数将以对象的属性名称进行命名。
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public ParameterCollection(IDictionary<string, object> dic)
        {
            Guard.ArgumentNull(dic, "dic");
            arrayList = new ArrayList();

            foreach (var kvp in dic)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 参数集合1与参数集合2相加。
        /// </summary>
        /// <param name="coll1">参数集合1。</param>
        /// <param name="coll2">参数集合2。</param>
        /// <returns>新的参数集合。</returns>
        public static ParameterCollection operator +(ParameterCollection coll1, ParameterCollection coll2)
        {
            if (coll1 == null && coll2 == null)
            {
                return null;
            }

            if (coll1 == null)
            {
                return coll2;
            }

            if (coll2 == null)
            {
                return coll1;
            }

            foreach (var p in coll2)
            {
                if (!coll1.Contains(p))
                {
                    coll1.Add(p);
                }
            }

            return coll1;
        }

        /// <summary>
        /// 从参数集合1中减去参数集合2。
        /// </summary>
        /// <param name="coll1">参数集合1。</param>
        /// <param name="coll2">参数集合2。</param>
        /// <returns>新的参数集合</returns>
        public static ParameterCollection operator -(ParameterCollection coll1, ParameterCollection coll2)
        {
            if (coll1 == null && coll2 == null)
            {
                return null;
            }

            if (coll1 == null)
            {
                return coll2;
            }

            if (coll2 == null)
            {
                return coll1;
            }

            foreach (var p in coll2)
            {
                if (coll1.Contains(p))
                {
                    coll1.Remove(p);
                }
            }

            return coll1;
        }

        /// <summary>
        /// 将一个 <see cref="ParameterCollection"/> 对象转换为 <see cref="IDictionary"/> 对象。
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static explicit operator Dictionary<string, object>(ParameterCollection parameters)
        {
            if (parameters == null)
            {
                return null;
            }

            var dic = new Dictionary<string, object>();
            foreach (var par in parameters)
            {
                dic.Add(par.ParameterName, par.Value);
            }

            return dic;
        }

        /// <summary>
        /// 将一个 <see cref="IDictionary"/> 对象转换为 <see cref="ParameterCollection"/> 对象。
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static implicit operator ParameterCollection(Dictionary<string, object> dic)
        {
            if (dic == null)
            {
                return null;
            }

            var paramters = new ParameterCollection();
            foreach (var kvp in dic)
            {
                paramters.Add(kvp.Key, kvp.Value);
            }

            return paramters;
        }

        /// <summary>
        /// 将此参数集合克隆一份副本。
        /// </summary>
        /// <returns>新的参数集合。</returns>
        public ParameterCollection Clone()
        {
            var pars = new ParameterCollection();
            foreach (var par in this)
            {
                pars.Add(par.Clone());
            }

            return pars;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// 清除集合中的所有参数。
        /// </summary>
        public void Clear()
        {
            arrayList.Clear();
        }

        /// <summary>
        /// 确定对象是否存在于当前参数集合中。
        /// </summary>
        /// <param name="parameter">要判断的参数对象。</param>
        /// <returns>如果存在返回 true，否则返回 false。</returns>
        public bool Contains(Parameter parameter)
        {
            return arrayList.Contains(parameter);
        }

        /// <summary>
        /// 将集合中的所有参数复制到数组中。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(Parameter[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<Parameter>.Remove(Parameter item)
        {
            if (IndexOf(item) != -1)
            {
                arrayList.Remove(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取元素的个数。
        /// </summary>
        public int Count
        {
            get { return arrayList.Count; }
        }

        bool ICollection<Parameter>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// 使用参数名来判断是否包含指定的项。
        /// </summary>
        /// <param name="parameterName">欲判断的参数名称。</param>
        /// <returns>如果存在返回 true，否则返回 false。</returns>
        public bool ContainsKey(string parameterName)
        {
            for (int i = 0; i < Count; i++)
            {
                if (string.Compare(this[i].ParameterName, parameterName, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据值查看集合内的参数。
        /// </summary>
        /// <param name="value">欲判断参数值。</param>
        /// <param name="parameter">查找到的参数。</param>
        /// <returns>如果存在则为 true。</returns>
        public bool TryGetParameter(object value, out Parameter parameter)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Value.Equals(value))
                {
                    parameter = this[i];
                    return true;
                }
            }

            parameter = null;
            return false;
        }

        /// <summary>
        /// 添加参数对象。
        /// </summary>
        /// <param name="parameter"></param>
        public void Add(Parameter parameter)
        {
            arrayList.Add(parameter);
        }

        /// <summary>
        /// 添加参数对象，用于输入型参数。
        /// </summary>
        /// <param name="parameterName">参数名称。</param>
        /// <param name="sourceColumn">源列名称。</param>
        /// <param name="value">参数的值。</param>
        /// <returns>返回参数对象。</returns>
        public Parameter Add<T>(string parameterName, string sourceColumn, T value)
        {
            if (ContainsKey(parameterName))
            {
                return null;
            }

            var parameter = new Parameter(parameterName);
            parameter.DbType = typeof(T).GetDbType();
            parameter.Value = GetDBNullValue(value);

            arrayList.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// 添加参数对象，用于输入型参数。
        /// </summary>
        /// <param name="parameterName">参数名称。</param>
        /// <param name="value">参数的值。</param>
        /// <returns>返回参数对象。</returns>
        public Parameter Add<T>(string parameterName, T value)
        {
            return Add(parameterName, "", value);
        }

        /// <summary>
        /// 添加输入输出型参数对象。
        /// </summary>
        /// <param name="parameterName">参数名称。</param>
        /// <param name="sourceColumn">源列名称。</param>
        /// <param name="value">参数的值。</param>
        /// <param name="size">数据长度，如果为固定长度数据类型，可使用默认值0。</param>
        /// <returns>参数对象。</returns>
        public Parameter AddInOut<T>(string parameterName, string sourceColumn, T value, int size)
        {
            return Add(parameterName, sourceColumn, value, null, size, ParameterDirection.InputOutput);
        }

        /// <summary>
        /// 添加返回型参数对象。
        /// </summary>
        /// <param name="parameterName">参数名称。</param>
        /// <param name="sourceColumn">源列名称。</param>
        /// <param name="dbType">参数的数据类型。</param>
        /// <param name="size">数据长度，如果为固定长度数据类型，可使用默认值0。</param>
        /// <returns>参数对象。</returns>
        public Parameter AddReturn(string parameterName, string sourceColumn, DbType dbType, int size)
        {
            return Add<object>(parameterName, sourceColumn, null, dbType, size, ParameterDirection.ReturnValue);
        }

        /// <summary>
        /// 添加输出型参数对象。
        /// </summary>
        /// <param name="parameterName">参数名称。</param>
        /// <param name="sourceColumn">源列名称。</param>
        /// <param name="dbType">参数的数据类型。</param>
        /// <param name="size">数据长度。</param>
        /// <returns>参数对象。</returns>
        public Parameter AddOut(string parameterName, string sourceColumn, DbType dbType, int size)
        {
            return Add<object>(parameterName, sourceColumn, null, dbType, size, ParameterDirection.Output);
        }

        /// <summary>
        /// 添加参数对象，用于输入输出型参数。
        /// </summary>
        /// <param name="parameterName">参数名称。</param>
        /// <param name="sourceColumn">源列名称。</param>
        /// <param name="value">参数的值。</param>
        /// <param name="dbType">参数的值类型。</param>
        /// <param name="size">数据长度。</param>
        /// <param name="direction">参数的方向。</param>
        /// <returns>参数对象。</returns>
        private Parameter Add<T>(string parameterName, string sourceColumn, T value, DbType? dbType, int? size, ParameterDirection direction)
        {
            if (ContainsKey(parameterName))
            {
                return null;
            }

            var parameter = new Parameter(parameterName)
                {
                    SourceColumn = string.IsNullOrEmpty(sourceColumn) ? parameterName : sourceColumn
                };

            #region 计算参数的数据类型
            if (dbType != null)
            {
                parameter.DbType = (DbType)dbType;
            }
            else
            {
                parameter.DbType = typeof(T).GetDbType();
            }
            #endregion

            parameter.Value = GetDBNullValue(value);
            parameter.Direction = direction;

            #region 计算参数的长度
            if (direction != ParameterDirection.Input)
            {
                if (size != null)
                {
                    parameter.Size = (int)size;
                }
                else if (value != null)
                {
                    parameter.Size = value.GetType().GetDbTypeSize(size);
                }
            }
            #endregion

            arrayList.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// 从一个参数集合中添加对象。
        /// </summary>
        /// <param name="parameters">另一个参数集合。</param>
        public void Add(ParameterCollection parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (!Contains(parameter))
                {
                    Add(parameter.Clone());
                }
            }
        }

        /// <summary>
        /// 将一个参数数组添加到集合中。
        /// </summary>
        /// <param name="parameters">提供的参数数组。</param>
        public void AddRange(Parameter[] parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (!Contains(parameter))
                {
                    Add(parameter);
                }
            }
        }

        /// <summary>
        /// 移除一个参数对象。
        /// </summary>
        /// <param name="parameter">要移除的参数对象。</param>
        public void Remove(Parameter parameter)
        {
            arrayList.Remove(parameter);
        }

        /// <summary>
        /// 移除指定名称的查询参数。
        /// </summary>
        /// <param name="parameterName">参数名称。</param>
        public void Remove(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index != -1)
            {
                RemoveAt(index);
            }
        }

        /// <summary>
        /// 搜索指定的查询参数在集合中的索引位置。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(Parameter item)
        {
            return arrayList.IndexOf(item);
        }

        /// <summary>
        /// 搜索指定参数名称所在集合中的索引位置。
        /// </summary>
        /// <param name="parameterName">参数名称。</param>
        /// <returns></returns>
        public int IndexOf(string parameterName)
        {
            for (int i = 0; i < Count; i++)
            {
                if (string.Compare(this[i].ParameterName, parameterName, true) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 在指定的索引位置处插入一个查询参数。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, Parameter item)
        {
            if (index < 0 || index > Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            arrayList.Insert(index, item);
        }

        /// <summary>
        /// 移除指定索引位置处的查询参数。
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index > Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            arrayList.RemoveAt(index);
        }

        /// <summary>
        /// 根据索引值获取索引器的值。
        /// </summary>
        /// <returns></returns>
        public Parameter this[int index]
        {
            get
            {
                if (index < 0 || index > Count - 1)
                {
                    throw new IndexOutOfRangeException();
                }

                return arrayList[index] as Parameter;
            }

            set
            {
                if (index < 0 || index > Count - 1)
                {
                    throw new IndexOutOfRangeException();
                }

                arrayList[index] = value;
            }
        }

        /// <summary>
        /// 根据参数名获取索引器的值。
        /// </summary>
        public Parameter this[string parameterName]
        {
            get
            {
                if (string.IsNullOrEmpty(parameterName))
                {
                    throw new ArgumentNullException("parameterName");
                }

                for (int i = 0; i < Count; i++)
                {
                    if (string.Compare(this[i].ParameterName, parameterName, true) == 0)
                    {
                        return this[i];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 获取查询参数的枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Parameter> GetEnumerator()
        {
            var tor = arrayList.GetEnumerator();
            while (tor.MoveNext())
            {
                yield return (Parameter) tor.Current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private object GetDBNullValue(object value)
        {
            return value ?? DBNull.Value;
        }
    }
}