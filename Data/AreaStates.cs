using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace AreaHelper.Data
{
    public class AreaStates : IExposable
    {
        private Dictionary<int, AreaState> _states = new Dictionary<int, AreaState>();
        
        public Dictionary<int, AreaState> States => _states;
        
        private string _key;
        
        public string Key => _key;

        [UsedImplicitly]
        public AreaStates()
        {
        }
        
        public AreaStates(AreaStates areaStates)
        {
            _states = areaStates._states.ToDictionary(x => x.Key, x => new AreaState(x.Value));
            _key = areaStates._key;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref _key, "key");
            Scribe_Collections.Look(ref _states, "states", LookMode.Value, LookMode.Deep);
        }
        
        public override string ToString()
        {
            var combinedFrom = States;
            
            if (!combinedFrom.Any()) return "";
            
            var keys = combinedFrom
                .OrderBy(x => x.Key)
                .ThenBy(x => x.Value.Include)
                .Select(x => x.Key + ":" + (x.Value.Include ? "1" : "0"));
            
            return string.Join(",", keys);
        }

        public bool HasLayerByArea(Area area, AreaStateLayer layer)
        {
            return _states.TryGetValue(area.ID, out var state) && state.TryGetLayerState(layer, out _);
        }

        protected void BuildKey()
        {
            _key = ToString();
        }

        private bool HasCombinedLayer()
        {
            return _states.Values.Any(x => x.TryGetLayerState(AreaStateLayer.Combined, out _));
        }

        private bool HasIncludedCombinedLayer()
        {
            return _states.Values.Any(x => x.TryGetLayerState(AreaStateLayer.Combined, out var state) && state);
        }

        public bool HasOnlyExcludedCombinedLayer()
        {
            return HasCombinedLayer() && !HasIncludedCombinedLayer();
        }

        public bool HasOnlyExcludedLayers()
        {
            return _states.Values.All(x => !x.Include);
        }

        private void Remove(int id, AreaStateLayer layer)
        {
            if (!_states.ContainsKey(id)) return;
            _states[id].Remove(layer);
            
            if (!_states[id].HasLayers())
                _states.Remove(id);
            
            BuildKey();
        }

        public void Remove(Area area, AreaStateLayer layer)
        {
            Remove(area.ID, layer);
        }

        public void Remove(Area area)
        {
            _states.Remove(area.ID);
            BuildKey();
        }

        public void SetOrAdd(Area area, bool include, AreaStateLayer layer)
        {
            var id = area.ID;
            
            if (!_states.TryGetValue(id, out var state))
                _states.Add(id, state = new AreaState(area));
            
            state.SetOrAdd(layer, include);
            BuildKey();
        }
    }
}