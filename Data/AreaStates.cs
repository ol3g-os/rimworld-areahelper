using System;
using System.Collections.Generic;
using System.Linq;
using AreaHelper.Extended;
using Verse;

namespace AreaHelper.Data
{
    public class AreaStates : IExposable
    {
        private Dictionary<int, AreaState> _states;
        
        public Dictionary<int, AreaState> States => _states;
        
        private string _key;
        
        public string Key => _key;

        public EventHandler<AreaState> Removed;
        
        public EventHandler<AreaState> Added;
        
        public EventHandler<AreaState> Changed;

        public MapExtended MapExtended;
        
        public AreaCombined AreaCombined;
        
        public AreaStates()
        {
        }

        public AreaStates(MapExtended mapExtended)
        {
            MapExtended = mapExtended;
            _states = new Dictionary<int, AreaState>();
        }
        
        public void Toggle(AreaExtended area, bool state)
        {
            var id = area.Area.ID;

            if (!_states.TryGetValue(id, out var areaState))
            {
                _states.Add(id, areaState = new AreaState(area));
                Added?.Invoke(this, areaState);
                area.Removed += OnAreaRemoved;
            }

            areaState.Include = state;
            Changed?.Invoke(this, areaState);
            _key = BuildKey(_states);
        }

        public void Remove(AreaExtended area)
        {
            var id = area.Area.ID;
            var areaState = _states[id];

            _states.Remove(id);
            
            area.Removed -= OnAreaRemoved;
            Removed?.Invoke(this, areaState);
            _key = BuildKey(_states);
        }

        private void OnAreaRemoved(object sender, AreaExtended area)
        {
            Remove(area);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _key, "key");
            Scribe_Collections.Look(ref _states, "states");

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs) return;
            
            foreach (var keyValue in _states)
                keyValue.Value.AreaExtended = MapExtended.AreaExtended[keyValue.Key];

            if (_key == null) return;
            
            AreaCombined = MapExtended.Areas.FirstOrDefault(x => x.AreaStates.Key == _key);
        }
        
        public static string BuildKey(Dictionary<int, AreaState> combinedFrom)
        {
            if (!combinedFrom.Any()) return null;
            
            var keys = combinedFrom
                .OrderBy(x => x.Key)
                .ThenBy(x => x.Value.Include)
                .Select(x => x.Key + ":" + (x.Value.Include ? "1" : "0"));
            
            return string.Join(",", keys);
        }
    }
}