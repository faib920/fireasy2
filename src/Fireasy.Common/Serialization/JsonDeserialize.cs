// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Fireasy.Common.Serialization
{
    internal sealed class JsonDeserialize : DeserializeBase
    {
        private readonly JsonSerializeOption _option;
        private readonly JsonSerializer _serializer;
        private readonly JsonReader _jsonReader;
        private readonly TypeConverterCache<JsonConverter> _converters = new TypeConverterCache<JsonConverter>();

        private class MethodCache
        {
            internal protected static readonly MethodInfo ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), BindingFlags.Public | BindingFlags.Static);
        }

        internal JsonDeserialize(JsonSerializer serializer, JsonReader reader, JsonSerializeOption option)
            : base(option)
        {
            _serializer = serializer;
            _jsonReader = reader;
            _option = option;
        }

        internal T Deserialize<T>()
        {
            return (T)Deserialize(typeof(T));
        }

        internal object Deserialize(Type type)
        {
            object value = null;
            if (WithSerializable(type, ref value))
            {
                return value;
            }

            if (WithConverter(type, ref value))
            {
                return value;
            }

            _jsonReader.SkipWhiteSpaces();

            if (type == typeof(Type))
            {
                return DeserializeType();
            }

            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type) ||
                type == typeof(object))
            {
                return DeserializeIntelligently(type);
            }
            if (type == typeof(byte[]))
            {
                return DeserializeBytes();
            }
            if (type.GetNonNullableType() == typeof(TimeSpan))
            {
                return DeserializeTimeSpan(type.IsNullableType());
            }

            if (typeof(ArrayList).IsAssignableFrom(type))
            {
                return DeserializeSingleArray();
            }

            if (typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string))
            {
                return DeserializeDictionary(type);
            }

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                return DeserializeList(type);
            }

            if (type == typeof(DataSet))
            {
                return DeserializeDataSet();
            }

            if (type == typeof(DataTable))
            {
                return DeserializeDataTable();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition().FullName.StartsWith("System.Tuple`", StringComparison.InvariantCulture))
            {
                return DeserializeTuple(type);
            }

            if (type.IsEnum)
            {
                return DeserializeEnum(type);
            }

            return DeserializeValue(type);
        }

        private bool WithSerializable(Type type, ref object value)
        {
            if (typeof(ITextSerializable).IsAssignableFrom(type))
            {
                var obj = type.New<ITextSerializable>();
                var rawValue = _jsonReader.ReadRaw();
                if (rawValue != null)
                {
                    obj.Deserialize(_serializer, rawValue);
                    value = obj;
                }

                return true;
            }

            return false;
        }

        private bool WithConverter(Type type, ref object value)
        {
            var converter = _converters.GetReadableConverter(type, _option);

            if (converter == null || !converter.CanRead)
            {
                return false;
            }

            value = converter.ReadJson(_serializer, _jsonReader, type);

            return true;
        }

        private object DeserializeValue(Type type)
        {
            if (_jsonReader.IsNull())
            {
                if ((type.GetNonNullableType().IsValueType && !type.IsNullableType()))
                {
                    throw new SerializationException(SR.GetString(SRKind.JsonNullableType, type));
                }

                return null;
            }

            var stype = type.GetNonNullableType();
            var typeCode = Type.GetTypeCode(stype);
            if (typeCode == TypeCode.Object)
            {
                return ParseObject(type);
            }

            var value = _jsonReader.ReadValue();
            if (type.IsNullableType() && (value == null || string.IsNullOrEmpty(value.ToString())))
            {
                return null;
            }

            switch (typeCode)
            {
                case TypeCode.DateTime:
                    CheckNullString(value, type);
                    return SerializerUtil.ParseDateTime(value.ToString(), _option.Culture, _option.DateTimeZoneHandling);
                case TypeCode.String:
                    return value == null ? null : DeserializeString(value.ToString());
                default:
                    CheckNullString(value, type);
                    try
                    {
                        return value.ToType(stype);
                    }
                    catch (Exception ex)
                    {
                        throw new SerializationException(SR.GetString(SRKind.DeserializeError, value, type), ex);
                    }
            }
        }

        private static void CheckNullString(object value, Type type)
        {
            if ((value == null || value.ToString().Length == 0) && !type.IsNullableType())
            {
                throw new SerializationException(SR.GetString(SRKind.JsonNullableType, type));
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        private DataSet DeserializeDataSet()
        {
            var ds = new DataSet();
            _jsonReader.SkipWhiteSpaces();
            _jsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                var name = _jsonReader.ReadAsString();
                _jsonReader.SkipWhiteSpaces();
                _jsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                _jsonReader.SkipWhiteSpaces();
                var tb = DeserializeDataTable();
                tb.TableName = name;
                ds.Tables.Add(tb);
                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }
            }

            return ds;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        private DataTable DeserializeDataTable()
        {
            var tb = new DataTable();
            _jsonReader.SkipWhiteSpaces();
            _jsonReader.AssertAndConsume(JsonTokens.StartArrayCharacter);
            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                DeserializeDataRow(tb);
                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndArrayCharacter))
                {
                    break;
                }
            }

            return tb;
        }

        private void DeserializeDataRow(DataTable tb)
        {
            if (_jsonReader.Peek() != JsonTokens.StartObjectLiteralCharacter)
            {
                return;
            }

            var noCols = tb.Columns.Count == 0;
            _jsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            var row = tb.NewRow();
            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                var name = DeserializeString(_jsonReader.ReadKey());
                _jsonReader.SkipWhiteSpaces();
                _jsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                _jsonReader.SkipWhiteSpaces();
                var obj = ParseValue(_jsonReader.ReadValue());
                _jsonReader.SkipWhiteSpaces();
                if (noCols)
                {
                    tb.Columns.Add(name, obj != null ? obj.GetType() : typeof(object));
                }

                if (tb.Columns.Contains(name))
                {
                    row[name] = obj ?? DBNull.Value;
                }

                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }
            }

            tb.Rows.Add(row);
        }

        private object ParseValue(object value)
        {
            if (value is string s)
            {
                if (Regex.IsMatch(s, @"Date\((\d+)\+(\d+)\)"))
                {
                    return DeserializeDateTime(s);
                }

                return DeserializeString(s);
            }

            return value;
        }

        private object DeserializeList(Type listType)
        {
            _jsonReader.SkipWhiteSpaces();
            if (_jsonReader.IsNull())
            {
                return null;
            }

            var isReadonly = listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>);

            CreateListContainer(listType, out Type elementType, out IList container);

            _jsonReader.AssertAndConsume(JsonTokens.StartArrayCharacter);
            while (true)
            {
                if (_jsonReader.IsNextCharacter(JsonTokens.EndArrayCharacter))
                {
                    _jsonReader.Read();
                    break;
                }

                _jsonReader.SkipWhiteSpaces();
                var value = DeserializeIntelligently(elementType);
                if (value != null && value.GetType() != elementType)
                {
                    value = value.ToType(elementType);
                }
                container.Add(value);
                _jsonReader.SkipWhiteSpaces();

                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndArrayCharacter))
                {
                    break;
                }
            }

            if (listType.IsArray)
            {
                var invoker = ReflectionCache.GetInvoker(MethodCache.ToArray.MakeGenericMethod(elementType));
                return invoker.Invoke(null, container);
            }

            if (isReadonly)
            {
                return listType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { container.GetType() }, null).Invoke(new object[] { container });
            }

            return container;
        }

        private IDictionary DeserializeDictionary(Type dictType)
        {
            _jsonReader.SkipWhiteSpaces();
            if (_jsonReader.IsNull())
            {
                return null;
            }

            CreateDictionaryContainer(dictType, out Type[] keyValueTypes, out IDictionary container);

            _jsonReader.SkipWhiteSpaces();
            _jsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            while (true)
            {
                if (_jsonReader.IsNextCharacter(JsonTokens.EndObjectLiteralCharacter))
                {
                    _jsonReader.Read();
                    return container;
                }

                _jsonReader.SkipWhiteSpaces();
                var key = Deserialize(keyValueTypes[0]);
                _jsonReader.SkipWhiteSpaces();
                _jsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                _jsonReader.SkipWhiteSpaces();
                container.Add(key, Deserialize(keyValueTypes[1]));
                _jsonReader.SkipWhiteSpaces();

                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }
            }

            return container;
        }

        private object DeserializeIntelligently(Type type)
        {
            if (_jsonReader.IsNull())
            {
                return null;
            }

            if (_jsonReader.IsNextCharacter(JsonTokens.StartArrayCharacter))
            {
                return DeserializeSingleArray();
            }
            else if (_jsonReader.IsNextCharacter(JsonTokens.StartObjectLiteralCharacter))
            {
                if (typeof(object) == type)
                {
                    return DeserializeDynamicObject(type);
                }

                return Deserialize(type);
            }

            return _jsonReader.ReadValue();
        }

        private object DeserializeDynamicObject(Type type)
        {
            var dynamicObject = type == typeof(object) ? new ExpandoObject() :
                type.New<IDictionary<string, object>>();

            _jsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);

            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.IsNextCharacter(JsonTokens.EndObjectLiteralCharacter))
                {
                    _jsonReader.Read();
                    break;
                }

                var name = _jsonReader.ReadKey();
                _jsonReader.SkipWhiteSpaces();
                _jsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                _jsonReader.SkipWhiteSpaces();

                object value;
                if (_jsonReader.IsNextCharacter(JsonTokens.StartArrayCharacter))
                {
                    value = DeserializeList(typeof(List<dynamic>));
                }
                else if (_jsonReader.IsNextCharacter(JsonTokens.StartObjectLiteralCharacter))
                {
                    value = Deserialize<dynamic>();
                }
                else
                {
                    value = _jsonReader.ReadValue();
                }

                dynamicObject.Add(name, value);
                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }
            }

            return dynamicObject;
        }

        private object DeserializeTuple(Type type)
        {
            if (_jsonReader.IsNull())
            {
                return null;
            }

            var genericTypes = type.GetGenericArguments();
            var arguments = new object[genericTypes.Length];

            _jsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);

            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.IsNextCharacter(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }

                var name = _jsonReader.ReadKey();

                _jsonReader.SkipWhiteSpaces();
                _jsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                _jsonReader.SkipWhiteSpaces();

                var index = GetTupleItemIndex(name);
                if (index != -1)
                {
                    arguments[index] = Deserialize(genericTypes[index]);
                }

                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }
            }

            return type.New(arguments);
        }

        private static int GetTupleItemIndex(string name)
        {
            var match = Regex.Match(name, @"Item(\d)");
            if (match.Success && match.Groups.Count > 0)
            {
                return match.Groups[1].Value.To<int>() - 1;
            }
            else
            {
                return -1;
            }
        }

        private object DeserializeEnum(Type enumType)
        {
            _jsonReader.SkipWhiteSpaces();
            string evalue;
            if (_jsonReader.IsNextCharacter('"'))
            {
                evalue = _jsonReader.ReadAsString();
            }
            else
            {
                evalue = _jsonReader.ReadAsInt32().ToString(_option.Culture);
            }

            return Enum.Parse(enumType, evalue);
        }

        private byte[] DeserializeBytes()
        {
            var str = _jsonReader.ReadAsString();
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return Convert.FromBase64String(str);
        }

        private TimeSpan? DeserializeTimeSpan(bool isNullable)
        {
            if (_jsonReader.IsNull())
            {
                return isNullable ? (TimeSpan?)null : TimeSpan.Zero;
            }

            var str = _jsonReader.ReadAsString();
            if (TimeSpan.TryParse(str, out TimeSpan result))
            {
                return result;
            }

            return null;
        }

        private object DeserializeSingleArray()
        {
            var array = new ArrayList();
            _jsonReader.AssertAndConsume(JsonTokens.StartArrayCharacter);

            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                var value = Deserialize(typeof(object));
                array.Add(value);
                _jsonReader.SkipWhiteSpaces();

                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndArrayCharacter))
                {
                    break;
                }
            }

            return array;
        }

        private DateTime? DeserializeDateTime(string value)
        {
            if (value.Length == 0)
            {
                return null;
            }

            return SerializerUtil.ParseDateTime(value, null, _option.DateTimeZoneHandling);
        }

        private static string DeserializeString(string value)
        {
            if (Regex.IsMatch(value, "(?<code>\\\\u[0-9a-fA-F]{4})", RegexOptions.IgnoreCase))
            {
                return value.DeUnicode();
            }

            return value;
        }

        private Type DeserializeType()
        {
            var value = _jsonReader.ReadAsString();
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value.ParseType();
        }

        private object ParseObject(Type type)
        {
            return type.IsAnonymousType() ? ParseAnonymousObject(type) :
                (HasEmptyConstructor(type) ? ParseGeneralObject(type) : ParseGeneralObjectByConstructor(type));
        }

        private object ParseGeneralObjectByConstructor(Type type)
        {
            if (_jsonReader.IsNull())
            {
                return null;
            }

            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(s => new 
                    { 
                        info = s, 
                        parameters = s.GetParameters().Select(t => new { name = t.Name, alias = t.GetCustomAttribute<TextSerializeParameterBindAttribute>()?.Name ?? t.Name }).ToArray() 
                    })
                .OrderByDescending(s => s.parameters.Length).ToArray();

            var alais = new Dictionary<string, string>();
            foreach (var cons in constructors)
            {
                foreach (var par in cons.parameters)
                {
                    alais.TryAdd(par.alias, par.name);
                }
            }

            var dict = new Dictionary<string, object>();

            _jsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);

            var mappers = GetAccessorMetadataMappers(type);

            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.IsNextCharacter(JsonTokens.EndObjectLiteralCharacter))
                {
                    _jsonReader.Read();
                    break;
                }

                var name = _jsonReader.ReadKey();
                _jsonReader.SkipWhiteSpaces();
                _jsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                _jsonReader.SkipWhiteSpaces();

                var key = alais.FirstOrDefault(s => s.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase)).Value ?? name;
                if (!mappers.TryGetValue(key, out SerializerPropertyMetadata metadata))
                {
                    _jsonReader.ReadValue();
                }
                else
                {
                    var value = Deserialize(metadata.PropertyInfo.PropertyType);
                    if (value != null)
                    {
                        dict.Add(name, value);
                    }
                }

                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }
            }

            //找最匹配的构造器
            var args = new object[constructors.Length][];
            var mustMatchIndex = -1;
            for (var i = 0; i < constructors.Length; i++)
            {
                var parameters = constructors[i].parameters;
                args[i] = new object[parameters.Length];
                var matchCount = 0;
                for (var j = 0; j < parameters.Length; j++)
                {
                    foreach (var kvp in dict)
                    {
                        if (parameters[j].alias.Equals(kvp.Key, StringComparison.CurrentCultureIgnoreCase))
                        {
                            args[i][j] = kvp.Value;
                            matchCount++;
                            break;
                        }
                    }
                }

                if (parameters.Length == matchCount)
                {
                    mustMatchIndex = i;
                    break;
                }
            }

            if (mustMatchIndex == -1)
            {
                throw new SerializationException(string.Empty, new AmbiguousMatchException());
            }

            var index = Math.Min(mustMatchIndex, 0);
            var instance = type.New(args[index]);

            //属性赋值
            foreach (var kvp in dict)
            {
                //忽略构造器里使用了的key
                if (constructors[index].parameters.Any(s => s.alias.Equals(kvp.Key, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                if (mappers.TryGetValue(kvp.Key, out SerializerPropertyMetadata metadata))
                {
                    metadata.Setter?.Invoke(instance, kvp.Value);
                }
            }

            return instance;
        }

        private object ParseGeneralObject(Type type)
        {
            if (_jsonReader.IsNull())
            {
                return null;
            }


            _jsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);

            var instance = CreateGeneralObject(type);
            var mappers = GetAccessorMetadataMappers(type);
            var processor = instance as IDeserializeProcessor;
            processor?.PreDeserialize();

            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.IsNextCharacter(JsonTokens.EndObjectLiteralCharacter))
                {
                    _jsonReader.Read();
                    break;
                }

                var name = _jsonReader.ReadKey();
                _jsonReader.SkipWhiteSpaces();
                _jsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                _jsonReader.SkipWhiteSpaces();

                if (!mappers.TryGetValue(name, out SerializerPropertyMetadata metadata))
                {
                    _jsonReader.ReadValue();
                }
                else
                {
                    var value = Deserialize(metadata.PropertyInfo.PropertyType);
                    if (value != null)
                    {
                        if (processor == null || !processor.SetValue(name, value))
                        {
                            metadata.Setter?.Invoke(instance, value);
                        }
                    }
                }

                _jsonReader.SkipWhiteSpaces();
                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }
            }

            processor?.PostDeserialize();

            return instance;
        }

        private object ParseAnonymousObject(Type type)
        {
            _jsonReader.AssertAndConsume(JsonTokens.StartObjectLiteralCharacter);
            var dic = new Dictionary<string, object>();

            var constructor = type.GetConstructors()[0];
            var conInvoker = ReflectionCache.GetInvoker(constructor);

            var mapper = GenerateParameterDictionary(constructor);
            var values = GenerateParameterValues(mapper);

            while (true)
            {
                _jsonReader.SkipWhiteSpaces();
                var name = _jsonReader.ReadKey();

                _jsonReader.SkipWhiteSpaces();
                _jsonReader.AssertAndConsume(JsonTokens.PairSeparator);
                _jsonReader.SkipWhiteSpaces();

                var par = mapper.FirstOrDefault(s => s.Key == name);

                if (string.IsNullOrEmpty(par.Key))
                {
                    _jsonReader.ReadRaw();
                }
                else
                {
                    values[name] = Deserialize(par.Value);
                }

                _jsonReader.SkipWhiteSpaces();

                if (_jsonReader.AssertNextIsDelimiterOrSeparator(JsonTokens.EndObjectLiteralCharacter))
                {
                    break;
                }
            }

            return conInvoker.Invoke(values.Values.ToArray());
        }

        private static Dictionary<string, Type> GenerateParameterDictionary(ConstructorInfo constructor)
        {
            var dic = new Dictionary<string, Type>();
            foreach (var par in constructor.GetParameters())
            {
                dic.Add(par.Name, par.ParameterType);
            }

            return dic;
        }

        private static Dictionary<string, object> GenerateParameterValues(Dictionary<string, Type> mapper)
        {
            var dic = new Dictionary<string, object>();
            foreach (var par in mapper)
            {
                dic.Add(par.Key, null);
            }

            return dic;
        }
    }
}
