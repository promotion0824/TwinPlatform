using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Extensions
{
    public class ConcurrentMultiDictionary<TK,TV>
    {
        private ConcurrentDictionary<TK, List<TV>>  _dict = new ConcurrentDictionary<TK, List<TV>>();
        private static readonly IReadOnlyCollection<TV> TheEmptyList = new List<TV>().AsReadOnly();

        public void Add(TK key, TV value)
        {
            var list = _dict.GetOrAdd(key, _ => new List<TV>() );
            lock (list)
            {
                // Could use HashSet here, but our usage is for small lists
                if (!list.Contains(value))
                {
                    list.Add(value);
                }
            }
        }

        public IReadOnlyCollection<TV> Get(TK key)
        {
            _dict.TryGetValue(key, out var list);
            if (list == null)
            {
                return TheEmptyList;
            }
            lock (list)
            {
                return list.AsReadOnly();
            }
        }

        public TV TryRemoveFirst(TK key, Func<TV, bool> predRemove)
        {
            _dict.TryGetValue(key, out var list);
            if (list == null)
            {
                return default;
            }
            lock (list)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (predRemove(item))
                    {
                        list.RemoveAt(i);
                        return item;
                    }
                }
            }
            return default;
        }

        public List<TV> RemoveAll(TK key)
        {
            _dict.TryRemove(key, out var list);
            return list;
        }

    }
}
