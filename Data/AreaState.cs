using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace AreaHelper.Data
{
    public class AreaState : IExposable
    {
        public bool Include => _layers.Count > 0 && _layers.Values.All(x => x);

        private Area _area;

        public Area Area => _area;

        private Dictionary<AreaStateLayer, bool> _layers = new Dictionary<AreaStateLayer, bool>();
        
        [UsedImplicitly]
        public AreaState()
        {
        }
        
        public AreaState(Area area)
        {
            _area = area;
        }

        public AreaState(AreaState state)
        {
            _area = state._area;
            _layers = new Dictionary<AreaStateLayer, bool>(state._layers);
        }
        
        public AreaState(AreaState state, Area area)
        {
            _area = area;
            _layers = new Dictionary<AreaStateLayer, bool>(state._layers);
        }

        public bool TryGetLayerState(AreaStateLayer layer, out bool state)
        {
            return _layers.TryGetValue(layer, out state);
        }
        
        public void ExposeData()
        {
            Scribe_References.Look(ref _area, "area");
            Scribe_Collections.Look(ref _layers, "layers");
        }
        
        public void SetOrAdd(AreaStateLayer layer, bool state)
        {
            AreaHelperLogic.LogMessage($"{nameof(AreaState)}: {nameof(SetOrAdd)}: {_area.ID} toggleLayer={layer}, state={state}");
            _layers.SetOrAdd(layer, state);
        }
        
        public void Remove(AreaStateLayer layer)
        {
            var removed = _layers.Remove(layer);
            AreaHelperLogic.LogMessage($"{nameof(AreaState)}: {nameof(Remove)}: removed={removed}, count={_layers.Count}");
        }

        public bool HasLayers()
        {
            return _layers.Count > 0;
        }

        public bool IsValid()
        {
            return _area != null;
        }
    }
}