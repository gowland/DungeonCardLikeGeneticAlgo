using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;

namespace DungeonCardsGeneticAlgo.Support
{
    public class FitnessCache<T, TScore>
    {
        private readonly int _cacheSize;
        private readonly IDictionary<T, CacheItem> _items;

        private struct CacheItem
        {
            public CacheItem(TScore value)
            {
                CachedValue = value;
                LastAccess = DateTime.Now;
            }
            public TScore CachedValue { get; }
            public DateTime LastAccess { get; private set; }

            public void UpdateAccess()
            {
                LastAccess = DateTime.Now;
            }
        }

        public FitnessCache(int minItemsToKeep)
        {
            _cacheSize = minItemsToKeep * 2;
            _items = new Dictionary<T, CacheItem>(_cacheSize);
        }

        public bool TryGetFitness(T key, out TScore value)
        {
            if (_items.TryGetValue(key, out CacheItem cacheItem))
            {
                cacheItem.UpdateAccess();
                value = cacheItem.CachedValue;
                return true;
            }

            value = default;
            return false;
        }

        private void Purge()
        {
            var oldAccesses = _items.OrderBy(a => a.Value.LastAccess).Take(_cacheSize/2).Select(a => a.Key).ToArray();
            foreach (var key in oldAccesses)
            {
                _items.Remove(key);
            }
        }

        public void Cache(T key, TScore value)
        {
            _items[key] = new CacheItem(value);

            if (_items.Count >= _cacheSize)
            {
                Purge();
            }
        }
    }
}