using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AreaHelper.Data
{
    public class MapDataStore<TKey, TResult> : IExposable
    {
        public Dictionary<TKey, TResult> Data = new Dictionary<TKey, TResult>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Data, "data", LookMode.Value, LookMode.Deep);
        }
    }

    public class MapIndexedStore<TKey, TResult>
    {
        private Dictionary<int, MapDataStore<TKey, TResult>> _dictionary 
            = new Dictionary<int, MapDataStore<TKey, TResult>>();

        public Dictionary<int, MapDataStore<TKey, TResult>> Data => _dictionary;

        public TResult GetOrAdd(Map map, TKey key, Func<TResult> factory)
        {
            AreaHelperLogic.LogMessage($"GetOrAdd for mapKey {map.uniqueID} and key {key}");
            var mapId = map.uniqueID;

            if (!_dictionary.TryGetValue(mapId, out var store))
            {
                _dictionary.Add(mapId, store = new MapDataStore<TKey, TResult>());
            }

            if (!store.Data.TryGetValue(key, out var result))
                store.Data.Add(key, result = factory());

            return result;
        }

        public Dictionary<TKey, TResult> GetByIndex(int mapId)
        {
            _dictionary.TryGetValue(mapId, out var store);
            return store?.Data;
        }

        public TResult Get(int mapId, TKey key)
        {
            if (_dictionary.TryGetValue(mapId, out var store)
                && store.Data.TryGetValue(key, out var result))
                return result;

            return default;
        }

        public void Remove(int mapId)
        {
            _dictionary.Remove(mapId);
        }

        public void Remove(int mapId, TKey key)
        {
            if (_dictionary.TryGetValue(mapId, out var store))
                store.Data.Remove(key);
        }

        public void ExposeData(string name)
        {
            Scribe_Collections.Look(ref _dictionary, name, LookMode.Value, LookMode.Deep);
        }

        public void Add(int mapId, TKey key, TResult result)
        {
            AreaHelperLogic.LogMessage($"Add for mapKey {mapId} and key {key}");
            if (!_dictionary.TryGetValue(mapId, out var store))
                _dictionary.Add(mapId, store = new MapDataStore<TKey, TResult>());

            store.Data.Add(key, result);
        }
    }
}