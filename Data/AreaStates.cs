using System;
using System.Collections.Generic;
using System.Linq;
using AreaHelper.Extended;
using Verse;

namespace AreaHelper.Data
{
    public class AreaStates : IExposable, IDisposable
    {
        private Dictionary<int, AreaState> _states;
        
        public Dictionary<int, AreaState> States => _states;
        
        private string _key;
        
        public string Key => _key;

        public event EventHandler<AreaState> Removed;
        
        public event EventHandler<AreaState> Added;
        
        public event EventHandler<AreaState> Changed;

        public MapExtended MapExtended;
        
        private AreaCombined _areaCombined;
        
        public AreaCombined AreaCombined
        {
            get => _areaCombined;
            set
            {
                // if (_areaCombined != null)
                //     _areaCombined.Destroyed -= OnAreaCombinedDestroyed;
                
                _areaCombined = value;
                
                // if (_areaCombined != null)
                //     _areaCombined.Destroyed += OnAreaCombinedDestroyed;
            }
        }

        public AreaStates()
        {
        }

        public AreaStates(MapExtended mapExtended)
        {
            MapExtended = mapExtended;
            _states = new Dictionary<int, AreaState>();
        }
        
        public void Toggle(AreaExtended area, bool state, AreaStateLayer layer = AreaStateLayer.Combined)
        {
            var id = area.Area.ID;

            if (!_states.TryGetValue(id, out var areaState))
            {
                _states.Add(id, areaState = new AreaState(area));
                Added?.Invoke(this, areaState);
                area.Removed += OnAreaRemoved;
            }

            areaState.ToggleLayer(layer, state);
            
            OnStateChanged(areaState);
        }

        /// <summary>
        /// Remove state layer
        /// </summary>
        /// <param name="area"></param>
        /// <param name="layer"></param>
        public void Remove(AreaExtended area, AreaStateLayer layer = AreaStateLayer.Combined)
        {
            var areaState = _states[area.Area.ID];
            areaState.RemoveLayer(layer);

            if (areaState.HasState())
                OnStateChanged(areaState);
            else
                Remove(areaState);
        }

        /// <summary>
        /// Remove state
        /// </summary>
        /// <param name="areaState"></param>
        public void Remove(AreaState areaState)
        {
            areaState.AreaExtended.Removed -= OnAreaRemoved;
            _states.Remove(areaState.Area.ID);
            _key = BuildKey(_states);
            
            if (_areaCombined != null)
            {
                AreaHelper.LogMessage($"Area remove, find new area with key {_key}");
                _areaCombined = MapExtended.Areas.FirstOrDefault(x => x.AreaStates.Key == _key);
                AreaHelper.LogMessage($"Found {_areaCombined != null}");

                if (_areaCombined == null)
                {
                    AreaHelper.LogMessage($"Try create and set new area with key {_key}");
                    _areaCombined = new AreaCombined(MapExtended, this);
                    MapExtended.Areas.Add(_areaCombined);
                }
            }

            Removed?.Invoke(this, areaState);
        }

        public void Remove(int id)
        {
            Remove(_states[id]);
        }

        public bool HasIncludedState()
        {
            return _states.Values.Any(x => x.Include);
        }

        public bool HasLayerByArea(int id, AreaStateLayer layer)
        {
            return _states.ContainsKey(id) && _states[id].TryGetLayerState(layer, out _);
        }

        private void OnStateChanged(AreaState state)
        {
            _key = BuildKey(_states);
            Changed?.Invoke(this, state);
        }

        private void OnAreaRemoved(object sender, AreaExtended area)
        {
            Remove(_states[area.Area.ID]);
        }

        // private void OnAreaCombinedDestroyed(object sender, AreaCombined area)
        // {
        //     AreaHelper.LogMessage($"AreaCombined destroyed, find new area with key {_key}");
        //     AreaCombined = MapExtended.Areas.FirstOrDefault(x => x.AreaStates.Key == _key && x != area);
        // }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _key, "key");
            Scribe_Collections.Look(ref _states, "states");

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs) return;
            
            foreach (var keyValue in _states)
            {
                keyValue.Value.AreaExtended = MapExtended.AreaExtended[keyValue.Key];
                keyValue.Value.AreaExtended.Removed += OnAreaRemoved;
            }

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

        public void Dispose()
        {
            foreach (var state in _states.Values)
                state.AreaExtended.Removed -= OnAreaRemoved;
            //
            // if (_areaCombined != null)
            //     _areaCombined.Destroyed -= OnAreaCombinedDestroyed;
        }
    }
}