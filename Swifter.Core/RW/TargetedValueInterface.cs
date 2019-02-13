using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    abstract class TargetedValueInterface
    {
        private static readonly HashCache<Guid, object> cache = new HashCache<Guid, object>();
        private static readonly IdCache<Guid> ids = new IdCache<Guid>();
        private static readonly object cache_lock = new object();

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static unsafe Guid CreateGuid<T>(long id)
        {
            var guid = Guid.Empty;

            var pGuid = (long*)&guid;

            pGuid[0] = TypeInfo<T>.Int64TypeHandle;
            pGuid[1] = id;

            return guid;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Set<T>(long id, IValueInterface<T> valueInterface)
        {
            if (valueInterface == null)
            {
                throw new ArgumentNullException(nameof(valueInterface));
            }

            var guid = CreateGuid<T>(id);
            
            lock (cache_lock)
            {
                ids[id] = guid;
                cache[guid] = valueInterface;
            }
        }
        
        protected static IValueInterface<T> Get<T>(long? id)
        {
            if (id == null)
            {
                return null;
            }

            var guid = CreateGuid<T>(id.Value);
            
            if (cache.TryGetValue(guid, out var value))
            {
                return (IValueInterface<T>)value;
            }

            return null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Remove(long id)
        {
            lock (cache_lock)
            {
                foreach (var item in ids.GetValues(id))
                {
                    cache.Remove(item);
                }

                ids.RemoveAll(id);
            }
        }
    }

    sealed class TargetedValueInterface<T> : TargetedValueInterface, IValueInterface<T>
    {
        private readonly IValueInterface<T> default_interface;
        
        public TargetedValueInterface(IValueInterface<T> default_interface)
        {
            this.default_interface = default_interface;
        }

        public T ReadValue(IValueReader valueReader)
        {
            return (Get<T>((valueReader as ITargetedBind)?.Id) ?? default_interface).ReadValue(valueReader);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            (Get<T>((valueWriter as ITargetedBind)?.Id) ?? default_interface).WriteValue(valueWriter, value);
        }
    }
}