using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XTypeInfo 类型信息。
    /// 此类型信息主要提供该类型的成员的缓存。
    /// </summary>
    public sealed class XTypeInfo
    {
        private static readonly Dictionary<CacheKey, XTypeInfo> InstanceCaches;
        private static readonly object InstanceCachesLock;

        static XTypeInfo()
        {
            InstanceCaches = new Dictionary<CacheKey, XTypeInfo>();
            InstanceCachesLock = new object();
        }

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
            var cacheKey = new CacheKey(type, flags);

            if (InstanceCaches.TryGetValue(cacheKey, out var value))
            {
                return value;
            }

            lock (InstanceCachesLock)
            {
                if (InstanceCaches.TryGetValue(cacheKey, out value))
                {
                    return value;
                }

                value = new XTypeInfo(type, flags);

                InstanceCaches.Add(cacheKey, value);

                return value;
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

        readonly XDictionary<string, XFieldInfo> fields;
        readonly XDictionary<string, XPropertyInfo> properties;
        readonly XDictionary<RuntimeParamsSign, XIndexerInfo> indexers;
        readonly XDictionary<RuntimeMethodSign, XMethodInfo> methods;
        internal readonly Dictionary<string, IXFieldRW> rwFields;
        internal readonly IXFieldRW[] rFields;

        /// <summary>
        /// 获取所有字段的集合。
        /// </summary>
        public ICollection<XFieldInfo> Fields
        {
            get
            {
                return fields.Values;
            }
        }

        /// <summary>
        /// 获取所有属性的集合。
        /// </summary>
        public ICollection<XPropertyInfo> Properties
        {
            get
            {
                return properties.Values;
            }
        }

        /// <summary>
        /// 获取所有索引器的集合
        /// </summary>
        public ICollection<XIndexerInfo> Indexers
        {
            get
            {
                return indexers.Values;
            }
        }

        /// <summary>
        /// 获取所有方法的集合。
        /// </summary>
        public ICollection<XMethodInfo> Methods
        {
            get
            {
                return methods.Values;
            }
        }

        /// <summary>
        /// 获取表示当前 XTypeInfo 的类型。
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// 获取创建 XTypeInfo 的绑定标识。
        /// </summary>
        public XBindingFlags Flags { get; private set; }

        private XTypeInfo(Type type, XBindingFlags flags)
        {
            Type = type;
            Flags = flags;

            IEqualityComparer<string> rwKeyComparer;

            if ((flags & XBindingFlags.RWIgnoreCase) != 0)
            {
                rwKeyComparer = new IgnoreCaseEqualityComparer();
            }
            else
            {
                rwKeyComparer = EqualityComparer<string>.Default;
            }

            fields = new XDictionary<string, XFieldInfo>(XDictionaryOption.AllowRepeat);
            properties = new XDictionary<string, XPropertyInfo>(XDictionaryOption.AllowRepeat);
            indexers = new XDictionary<RuntimeParamsSign, XIndexerInfo>(XDictionaryOption.AllowRepeat);
            methods = new XDictionary<RuntimeMethodSign, XMethodInfo>(XDictionaryOption.AllowRepeat);
            rwFields = new Dictionary<string, IXFieldRW>(rwKeyComparer);

            GetItems(type, flags);

            var rwFieldList = new List<IXFieldRW>();

            foreach (var item in fields)
            {
                var rwField = item.Value as IXFieldRW;

                if (rwField == null)
                {
                    continue;
                }

                var attributes = item.Value.FieldInfo.GetCustomAttributes(typeof(RWFieldAttribute), true);

                if (attributes != null && attributes.Length !=0)
                {
                    foreach (RWFieldAttribute attribute in attributes)
                    {
                        var attributedFieldRW = new XAttributedFieldRW(rwField, attribute);

                        if (attributedFieldRW.CanRead || attributedFieldRW.CanWrite)
                        {
                            rwFieldList.Add(attributedFieldRW);
                        }
                    }
                }
                else
                {
                    rwFieldList.Add(rwField);
                }
            }

            foreach (var item in properties)
            {
                var rwField = item.Value as IXFieldRW;

                if (rwField == null)
                {
                    continue;
                }

                var attributes = item.Value.PropertyInfo.GetCustomAttributes(typeof(RWFieldAttribute), true);

                if (attributes != null && attributes.Length != 0)
                {
                    foreach (RWFieldAttribute attribute in attributes)
                    {
                        var attributedFieldRW = new XAttributedFieldRW(rwField, attribute);

                        if (attributedFieldRW.CanRead || attributedFieldRW.CanWrite)
                        {
                            rwFieldList.Add(attributedFieldRW);
                        }
                    }
                }
                else
                {
                    rwFieldList.Add(rwField);
                }
            }

            rwFieldList.Sort((x, y) => x.Order.CompareTo(y.Order));

            foreach (var item in rwFieldList)
            {
                rwFields[item.Name] = item;
            }

            rFields = ArrayHelper.Filter(rwFieldList, item => item.CanRead, item => item);
        }

        void GetItems(Type type, XBindingFlags flags)
        {
            if (type.BaseType != null)
            {
                GetItems(type.BaseType, flags);
            }

            if ((flags & XBindingFlags.Field) != 0)
            {
                var fields = type.GetFields(AsBindingFlags(flags));

                foreach (var item in fields)
                {
                    this.fields[item.Name] = XFieldInfo.Create(item, flags);
                }
            }

            if ((flags & (XBindingFlags.Property | XBindingFlags.Indexer)) != 0)
            {
                var properties = type.GetProperties(AsBindingFlags(flags));

                foreach (var item in properties)
                {
                    var parameters = item.GetIndexParameters();

                    if (parameters != null && parameters.Length != 0)
                    {
                        if ((flags & XBindingFlags.Indexer) != 0)
                        {
                            indexers[new RuntimeParamsSign(ParametersToTypes(parameters))] = XIndexerInfo.Create(item, flags);
                        }

                    }
                    else
                    {
                        if ((flags & XBindingFlags.Property) != 0)
                        {
                            this.properties[item.Name] = XPropertyInfo.Create(item, flags);
                        }
                    }
                }
            }

            if ((flags & XBindingFlags.Method) != 0)
            {
                var methods = type.GetMethods(AsBindingFlags(flags));

                foreach (var item in methods)
                {
                    this.methods[new RuntimeMethodSign(item.Name, ParametersToTypes(item.GetParameters()))] = XMethodInfo.Create(item, flags);
                }
            }
        }

        /// <summary>
        /// 获取指定名称的字段。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回字段信息或 Null</returns>
        public XFieldInfo GetField(string name)
        {
            fields.TryGetValue(name, out var value);

            return value;
        }

        /// <summary>
        /// 获取指定名称的属性。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回属性信息或 Null</returns>
        public XPropertyInfo GetProperty(string name)
        {
            properties.TryGetValue(name, out var value);

            return value;
        }

        /// <summary>
        /// 获取指定参数类型的索引器。
        /// </summary>
        /// <param name="parameters">指定参数类型</param>
        /// <returns>返回索引器信息或 Null</returns>
        public XIndexerInfo GetIndexer(Type[] parameters)
        {
            indexers.TryGetValue(new RuntimeParamsSign(parameters), out var value);

            return value;
        }

        /// <summary>
        /// 获取指定参数的索引器。
        /// </summary>
        /// <param name="parameters">指定参数</param>
        /// <returns>返回索引器信息或 Null</returns>
        public XIndexerInfo GetIndexer(object[] parameters)
        {
            indexers.TryGetValue(new RuntimeParamsSign(parameters), out var value);

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
            methods.TryGetValue(new RuntimeMethodSign(name, parameters), out var value);

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
            methods.TryGetValue(new RuntimeMethodSign(name, parameters), out var value);

            return value;
        }
        
        private sealed class CacheKey : IEquatable<CacheKey>
        {
            public CacheKey(Type type, XBindingFlags flags)
            {
                this.type = type;
                this.flags = flags;
            }

            private readonly Type type;
            private readonly XBindingFlags flags;

            public override bool Equals(object obj)
            {
                return (obj is CacheKey cacheKey) && type == cacheKey.type && flags == cacheKey.flags;
            }

            public override int GetHashCode()
            {
                return type.GetHashCode() ^ flags.GetHashCode();
            }

            public bool Equals(CacheKey other)
            {
                return type == other.type && flags == other.flags;
            }
        }
    }
}
