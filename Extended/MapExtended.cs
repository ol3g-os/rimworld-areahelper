using System;
using System.Collections.Generic;
using Verse;

namespace AreaHelper.Extended
{
    public class MapExtended : IExposable, IDisposable
    {
        private List<AreaCombined> _areas;
        
        public List<AreaCombined> Areas => _areas;

        private Dictionary<int, AreaExtended> _areaExtended;
        public Dictionary<int, AreaExtended> AreaExtended => _areaExtended;

        public Map Map { get; set; }
        
        public AreaFullFilled AreaFullFilled { get; private set; }
        
        public MapExtended() {}
        
        public MapExtended(Map map)
        {
            Map = map;
            _areas = new List<AreaCombined>();
            _areaExtended = new Dictionary<int, AreaExtended>();
            AreaFullFilled = new AreaFullFilled(map.areaManager);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _areaExtended, "areaExtended");
            Scribe_Collections.Look(ref _areas, "areas", LookMode.Deep);

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs) return;
            
            AreaFullFilled = new AreaFullFilled(Map.areaManager);
            var areaFullAreaExtended = AreaHelper.Current.GetExtended(AreaFullFilled);
            if (areaFullAreaExtended != null)
                areaFullAreaExtended.Area = AreaFullFilled;

            foreach (var areaCombined in Areas)
            {
                areaCombined.areaManager = Map.areaManager;
                areaCombined.MapExtended = this;
            }
        }

        public void Dispose()
        {
            foreach (var area in _areas)
                area.Dispose();
        }
    }
}