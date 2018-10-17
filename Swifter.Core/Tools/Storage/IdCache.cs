using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供 Int64 为 Id 的缓存类。
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public sealed class IdCache<TValue>
    {
        private Entity[] entities;

        /// <summary>
        /// 初始化缓存类
        /// </summary>
        public IdCache()
        {
            entities = new Entity[ArrayHelper.GetPrime(0)];

            Count = 0;
        }

        /// <summary>
        /// 获取或设置指定 Id 的缓存。
        /// </summary>
        /// <param name="id">缓存 Id</param>
        /// <exception cref="KeyNotFoundException">当 Id 不存在事抛出异常</exception>
        /// <returns>返回缓存</returns>
        public TValue this[long id]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                var entity = FindEntity(id);

                if (entity == null)
                {
                    throw new KeyNotFoundException();
                }

                return entity.value;
            }
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            set
            {
                Insert(id, value, false);
            }
        }

        /// <summary>
        /// 获取该缓存集合的数量。
        /// </summary>
        public int Count
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get; private set;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int GetIndex(long id)
        {
            return (int)((id & long.MaxValue) % entities.Length);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void Insert(long id, TValue value, bool add)
        {
            if (add && Count == entities.Length)
            {
                Resize();
            }

            var index = GetIndex(id);

            for (var temp = entities[index]; temp != null; temp = temp.next)
            {
                if (temp.id == id)
                {
                    if (add)
                    {
                        throw new ArgumentException("Argument_AddingDuplicate");
                    }
                    else
                    {
                        temp.value = value;

                        return;
                    }
                }
            }

            var entity = new Entity();

            entity.id = id;
            entity.value = value;
            entity.next = entities[index];

            entities[index] = entity;

            ++Count;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private Entity FindEntity(long id)
        {
            var index = GetIndex(id);

            for (var entity = entities[index]; entity != null; entity = entity.next)
            {
                if (entity.id == id)
                {
                    return entity;
                }
            }

            return null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int LastEntity(long id, out Entity last)
        {
            var index = GetIndex(id);

            last = null;

            for (Entity entity = entities[index]; entity != null; last = entity, entity = entity.next)
            {
                if (entity.id == id)
                {
                    return index;
                }
            }

            return -1;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void Resize()
        {
            int newSize = Count * 2;

            unchecked
            {
                if ((uint)newSize > ArrayHelper.MaxPrimeArrayLength && ArrayHelper.MaxPrimeArrayLength > Count)
                {
                    Resize(ArrayHelper.MaxPrimeArrayLength);
                }
                else
                {
                    Resize(ArrayHelper.GetPrime(newSize));
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void Resize(int size)
        {
            var originalEntities = entities;

            entities = new Entity[size];

            Count = 0;

            foreach (var item in originalEntities)
            {
                for (var temp = item; temp != null; temp = temp.next)
                {
                    Insert(temp.id, temp.value, true);
                }
            }
        }

        /// <summary>
        /// 添加一个缓存。
        /// </summary>
        /// <param name="id">缓存 Id</param>
        /// <param name="value">缓存值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Add(long id, TValue value)
        {
            Insert(id, value, true);
        }

        /// <summary>
        /// 尝试获取指定 Id 的缓存。
        /// </summary>
        /// <param name="id">缓存 Id</param>
        /// <param name="value">返回缓存值</param>
        /// <returns>返回该缓存 Id 是否存在</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryGetValue(long id, out TValue value)
        {
            var entity = FindEntity(id);

            if (entity == null)
            {
                value = default(TValue);

                return false;
            }

            value = entity.value;
            return true;
        }

        /// <summary>
        /// 移除一个缓存。
        /// </summary>
        /// <param name="id">缓存 Id</param>
        /// <returns>返回该缓存 Id 之前是否存在</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool Remove(long id)
        {
            var index = LastEntity(id, out var last);

            if (index >= 0)
            {
                if (last == null)
                {
                    entities[index] = entities[index].next;
                }
                else
                {
                    last.next = last.next.next;
                }

                --Count;

                return true;
            }

            return false;
        }

        [Serializable]
        private sealed class Entity
        {
            public long id;
            public TValue value;
            public Entity next;
        }
    }
}