using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// XTypeInfo 类型信息。
    /// 此类型信息主要提供该类型的成员的缓存。
    /// </summary>
    public sealed class XTypeInfo
    {
        private static readonly XTypeInfoCache InstanceCaches = new XTypeInfoCache();

        private static Type[] ParametersToTypes(ParameterInfo[] parameters)
        {
            return ArrayHelper.Filter(parameters, item => true, item => item.ParameterType);
        }

        private static BindingFlags AsBindingFlags(XBindingFlags flags)
        {
            var result = BindingFlags.DeclaredOnly;

            if ((flags & XBindingFlags.Static) != 0)
            {
                result |= BindingFlags.Static;
            }

            if ((flags & XBindingFlags.Instance) != 0)
            {
                result |= BindingFlags.Instance;
            }

            if ((flags & XBindingFlags.Public) != 0)
            {
                result |= BindingFlags.Public;
            }

            if ((flags & XBindingFlags.NonPublic) != 0)
            {
                result |= BindingFlags.NonPublic;
            }

            return result;
        }

        /// <summary>
        /// 创建 XTypeInfo 类型信息。
        /// </summary>
        /// <param name="type">需要创建 XTypeInfo 类型信息的类型。</param>
        /// <param name="flags">绑定参数</param>
        /// <returns>返回一个 XTypeInfo 类型信息。</returns>
        public static XTypeInfo Create(Type type, XBindingFlags flags = XBindingFlags.Default)
        {
            return InstanceCaches.GetOrCreate(new CacheKey(type, flags));
        }
        
        private static void GetItems(Type type, XBindingFlags flags, List<XFieldInfo> fields, List<XPropertyInfo> properties, List<XIndexerInfo> indexers, List<XMethodInfo> methods)
        {
            if (type.BaseType != null)
            {
                GetItems(type.BaseType, flags, fields, properties, indexers, methods);
            }

            if ((flags & XBindingFlags.Field) != 0)
            {
                foreach (var item in type.GetFields(AsBindingFlags(flags)))
                {
                    fields.Add(XFieldInfo.Create(item, flags));
                }
            }

            if ((flags & (XBindingFlags.Property | XBindingFlags.Indexer)) != 0)
            {
                foreach (var item in type.GetProperties(AsBindingFlags(flags)))
                {
                    var parameters = item.GetIndexParameters();

                    if (parameters != null && parameters.Length != 0)
                    {
                        if ((flags & XBindingFlags.Indexer) != 0)
                        {
                            indexers.Add(XIndexerInfo.Create(item, flags));
                        }
                    }
                    else
                    {
                        if ((flags & XBindingFlags.Property) != 0)
                        {
                            properties.Add(XPropertyInfo.Create(item, flags));
                        }
                    }
                }
            }

            if ((flags & XBindingFlags.Method) != 0)
            {
                foreach (var item in type.GetMethods(AsBindingFlags(flags)))
                {
                    methods.Add(XMethodInfo.Create(item, flags));
                }
            }
        }

        /*
         * 
         * 为什么没有构造函数信息？
         * 
         * 因为系统的 System.Activator 已经非常优秀且方便了，所以没必要或者说我做不到更好的了。
         * 
         * Why no construct method informations?
         * 
         * Because System.Activator has been very good, I can't do better.
         * 
         */

        internal readonly NameCache<XFieldInfo> fieldsCache;
        internal readonly NameCache<XPropertyInfo> propertiesCache;
        internal readonly HashCache<RuntimeParamsSign, XIndexerInfo> indexersCache;
        internal readonly HashCache<RuntimeMethodSign, XMethodInfo> methodsCache;
        internal readonly NameCache<IXFieldRW> rwFieldsCache;



        internal readonly XFieldInfo[] fields;
        internal readonly XPropertyInfo[] properties;
        internal readonly XIndexerInfo[] indexers;
        internal readonly XMethodInfo[] methods;
        internal readonly IXFieldRW[] rwFields;

        internal readonly Type type;
        internal readonly XBindingFlags flags;

        private XTypeInfo(Type type, XBindingFlags flags)
        {
            this.type = type;
            this.flags = flags;

            IEqualityComparer<string> rwKeyComparer;

            if ((flags & XBindingFlags.RWIgnoreCase) != 0)
            {
                rwKeyComparer = new IgnoreCaseEqualityComparer();
            }
            else
            {
                rwKeyComparer = EqualityComparer<string>.Default;
            }

            var fields = new List<XFieldInfo>();
            var properties = new List<XPropertyInfo>();
            var indexers = new List<XIndexerInfo>();
            var methods = new List<XMethodInfo>();
            var rwFields = new List<IXFieldRW>();

            GetItems(type, flags, fields, properties, indexers, methods);

            foreach (var item in fields)
            {
                var rwField = item as IXFieldRW;

                if (rwField == null)
                {
                    continue;
                }

                var attributes = item.FieldInfo.GetCustomAttributes(typeof(RWFieldAttribute), true);

                if (attributes != null && attributes.Length != 0)
                {
                    foreach (RWFieldAttribute attribute in attributes)
                    {
                        var attributedFieldRW = new XAttributedFieldRW(rwField, attribute);

                        if (attributedFieldRW.CanRead || attributedFieldRW.CanWrite)
                        {
                            rwFields.Add(attributedFieldRW);
                        }
                    }
                }
                else
                {
                    rwFields.Add(rwField);
                }
            }

            foreach (var item in properties)
            {
                var rwField = item as IXFieldRW;

                if (rwField == null)
                {
                    continue;
                }

                var attributes = item.PropertyInfo.GetCustomAttributes(typeof(RWFieldAttribute), true);

                if (attributes != null && attributes.Length != 0)
                {
                    foreach (RWFieldAttribute attribute in attributes)
                    {
                        var attributedFieldRW = new XAttributedFieldRW(rwField, attribute);

                        if (attributedFieldRW.CanRead || attributedFieldRW.CanWrite)
                        {
                            rwFields.Add(attributedFieldRW);
                        }
                    }
                }
                else
                {
                    rwFields.Add(rwField);
                }
            }

            rwFields.Sort((x, y) => x.Order.CompareTo(y.Order));

            this.fields = fields.ToArray();
            this.properties = properties.ToArray();
            this.indexers = indexers.ToArray();
            this.methods = methods.ToArray();
            this.rwFields = rwFields.ToArray();

            fieldsCache = new NameCache<XFieldInfo>();
            propertiesCache = new NameCache<XPropertyInfo>();
            indexersCache = new HashCache<RuntimeParamsSign, XIndexerInfo>();
            methodsCache = new HashCache<RuntimeMethodSign, XMethodInfo>();
            rwFieldsCache = new NameCache<IXFieldRW>();

            foreach (var item in fields)
            {
                fieldsCache[item.name] = item;
            }
            foreach (var item in properties)
            {
                propertiesCache[item.name] = item;
            }
            foreach (var item in indexers)
            {
                indexersCache[new RuntimeParamsSign(ParametersToTypes(item.PropertyInfo.GetIndexParameters()))] = item;
            }
            foreach (var item in methods)
            {
                methodsCache[new RuntimeMethodSign(item.MethodInfo.Name, ParametersToTypes(item.MethodInfo.GetParameters()))] = item;
            }
            foreach (var item in rwFields)
            {
                rwFieldsCache[item.Name] = item;
            }
        }

        /// <summary>
        /// 获取表示当前 XTypeInfo 的类型。
        /// </summary>
        public Type Type => type;

        /// <summary>
        /// 获取创建 XTypeInfo 的绑定标识。
        /// </summary>
        public XBindingFlags Flags => flags;

        /// <summary>
        /// 获取字段集合。
        /// </summary>
        public XMemberCollection<XFieldInfo> Fields => new XMemberCollection<XFieldInfo>(fields);

        /// <summary>
        /// 获取属性集合。
        /// </summary>
        public XMemberCollection<XPropertyInfo> Properties => new XMemberCollection<XPropertyInfo>(properties);

        /// <summary>
        /// 获取索引器集合。
        /// </summary>
        public XMemberCollection<XIndexerInfo> Indexers => new XMemberCollection<XIndexerInfo>(indexers);

        /// <summary>
        /// 获取方法集合。
        /// </summary>
        public XMemberCollection<XMethodInfo> Methods => new XMemberCollection<XMethodInfo>(methods);

        /// <summary>
        /// 获取指定名称的字段。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回字段信息或 Null</returns>
        public XFieldInfo GetField(string name)
        {
            fieldsCache.TryGetValue(name, out var value);

            return value;
        }

        /// <summary>
        /// 获取指定名称的属性。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回属性信息或 Null</returns>
        public XPropertyInfo GetProperty(string name)
        {
            propertiesCache.TryGetValue(name, out var value);

            return value;
        }

        /// <summary>
        /// 获取指定参数类型的索引器。
        /// </summary>
        /// <param name="parameters">指定参数类型</param>
        /// <returns>返回索引器信息或 Null</returns>
        public XIndexerInfo GetIndexer(Type[] parameters)
        {
            indexersCache.TryGetValue(new RuntimeParamsSign(parameters), out var value);

            return value;
        }

        /// <summary>
        /// 获取指定参数的索引器。
        /// </summary>
        /// <param name="parameters">指定参数</param>
        /// <returns>返回索引器信息或 Null</returns>
        public XIndexerInfo GetIndexer(object[] parameters)
        {
            indexersCache.TryGetValue(new RuntimeParamsSign(parameters), out var value);

            return value;
        }

        /// <summary>
        /// 获取指定名称和参数类型的方法信息。
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <param name="parameters">方法参数类型</param>
        /// <returns>返回方法信息或 Null</returns>
        public XMethodInfo GetMethod(string name, Type[] parameters)
        {
            methodsCache.TryGetValue(new RuntimeMethodSign(name, parameters), out var value);

            return value;
        }

        /// <summary>
        /// 获取指定名称和参数的方法信息。
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <param name="parameters">方法参数</param>
        /// <returns>返回方法信息或 Null</returns>
        public XMethodInfo GetMethod(string name, object[] parameters)
        {
            methodsCache.TryGetValue(new RuntimeMethodSign(name, parameters), out var value);

            return value;
        }

        private sealed class CacheKey
        {
            public CacheKey(Type type, XBindingFlags flags)
            {
                this.type = type;
                this.flags = flags;
            }

            public readonly Type type;
            public readonly XBindingFlags flags;
        }

        private sealed class XTypeInfoCache : BaseCache<CacheKey, XTypeInfo>, BaseCache<CacheKey, XTypeInfo>.IGetOrCreate<CacheKey>
        {
            public XTypeInfoCache() : base(0)
            {
            }

            public CacheKey AsKey(CacheKey token)
            {
                return token;
            }

            public XTypeInfo AsValue(CacheKey token)
            {
                return new XTypeInfo(token.type, token.flags);
            }

            public XTypeInfo GetOrCreate(CacheKey token)
            {
                return GetOrCreate<XTypeInfoCache>(this, token);
            }

            protected override int ComputeHashCode(CacheKey key)
            {
                return key.type.GetHashCode() ^ (int)key.flags;
            }

            protected override bool Equals(CacheKey key1, CacheKey key2)
            {
                return key1.type == key2.type && key1.flags == key2.flags;
            }
        }
    }
}