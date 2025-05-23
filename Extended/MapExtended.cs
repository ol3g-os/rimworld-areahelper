using System;
using System.Collections.Generic;
using Verse;

namespace AreaHelper.Extended
{
    public class MapExtended : IExposable, IDisposable
    {
        private int _id;

        private List<AreaCombined> _areas;
        
        public List<AreaCombined> Areas => _areas;

        private Dictionary<int, AreaExtended> _areaExtended;
        public Dictionary<int, AreaExtended> AreaExtended => _areaExtended;

        public Map Map { get; set; }
        
        public MapExtended() {}
        
        public MapExtended(Map map)
        {
            Map = map;
            _areas = new List<AreaCombined>();
            _areaExtended = new Dictionary<int, AreaExtended>();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _id, "id");
            Scribe_Collections.Look(ref _areaExtended, "areaExtended");
            Scribe_Collections.Look(ref _areas, "areas", LookMode.Deep);

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs) return;
            
            Map = Current.Game.Maps.FirstOrDefault(x => x.uniqueID == _id);

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