using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;

namespace iTunesLyricOverlay.Database
{
    public sealed class CachedCollection<T>
        where T : class
    {
        public event Action CollectionUpdated;

        private readonly LiteCollection<T> m_collection;
        private readonly Func<T, BsonValue> m_getId;

        private readonly IDictionary<BsonValue, WeakReference<T>> m_cache = new SortedDictionary<BsonValue, WeakReference<T>>();

        public CachedCollection(LiteCollection<T> collection, Expression<Func<T, BsonValue>> indexProperty)
        {
            this.m_collection = collection;
            collection.EnsureIndex(indexProperty, true);

            this.m_getId = indexProperty.Compile();
        }

        private T GetCache(BsonValue id)
        {
            if (this.m_cache.TryGetValue(id, out var itemRef) && itemRef.TryGetTarget(out var item))
                return item;

            return null;
        }

        private void SetCache(T item) => this.SetCache(item, out var id);
        private void SetCache(T item, out BsonValue id)
        {
            id = this.m_getId(item);

            if (this.m_cache.ContainsKey(id))
                this.m_cache[id].SetTarget(item);
            else
                this.m_cache.Add(id, new WeakReference<T>(item));

            this.CollectionUpdated?.Invoke();
        }
        
        private void DelCache(BsonValue id)
        {
            this.m_cache.Remove(id);

            this.CollectionUpdated?.Invoke();
        }

        public void Upsert(T item)
        {
            lock (this.m_cache)
            {
                this.SetCache(item, out var id);

                this.m_collection.Upsert(id, item);
            }
        }

        public T FindById(BsonValue id)
        {
            lock (this.m_cache)
            {
                T item;

                item = this.GetCache(id);
                if (item != null)
                    return item;

                item = this.m_collection.FindById(id);
                if (item != null)
                {
                    this.SetCache(item);
                    return item;
                }

                return null;
            }
        }

        public IEnumerable<T> FindAll()
        {
            var lst = new List<T>();
            T t;

            lock (this.m_cache)
            {
                foreach (var item in this.m_collection.FindAll())
                {
                    var id = this.m_getId(item);

                    t = this.GetCache(id);
                    if (t != null)
                    {
                        lst.Add(t);
                        continue;
                    }

                    this.SetCache(item);
                    lst.Add(item);
                }
            }

            return lst;
        }

        public void Delete(BsonValue id)
        {
            lock (this.m_cache)
            {
                this.m_collection.Delete(id);

                this.DelCache(id);
            }
        }
    }
}
