using System;
using System.Collections.Generic;
using System.Linq;
using AreaHelper.Extended;
using Verse;

namespace AreaHelper.Data
{
    public class AreaState : IExposable
    {
        public AreaState()
        {
        }
        
        public AreaState(AreaExtended area)
        {
            AreaExtended = area;
        }

        public Area Area => AreaExtended.Area;
        
        public AreaExtended AreaExtended { get; set; }
        
        public bool Include => _layers.Values.All(x => x);

        [Obsolete]
        private bool _include;

        private Dictionary<AreaStateLayer, bool> _layers = new Dictionary<AreaStateLayer, bool>();

        public void ToggleLayer(AreaStateLayer layer, bool state)
        {
            _layers.SetOrAdd(layer, state);
        }
        
        public void RemoveLayer(AreaStateLayer layer)
        {
            var removed = _layers.Remove(layer);
            AreaHelper.LogMessage($"{nameof(AreaState)}: {nameof(RemoveLayer)}: removed={removed}, count={_layers.Count}");
        }

        public bool HasState()
        {
            return _layers.Any();
        }

        public bool TryGetLayerState(AreaStateLayer layer, out bool state)
        {
            return _layers.TryGetValue(layer, out state);
        }
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref _layers, "layers");
            
            if (Scribe.mode != LoadSaveMode.Saving)
                Scribe_Values.Look(ref _include, "include", true); // not save old settings

            if (Scribe.mode != LoadSaveMode.LoadingVars) return;
            
            if (!_layers.Any()) // if _layers is empty then it prev version of save file
                _layers.SetOrAdd(AreaStateLayer.Combined, _include);
        }
    }
}