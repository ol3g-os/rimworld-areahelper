using System.Collections.Generic;
using AreaHelper.Data;
using Verse;

namespace AreaHelper.Extended
{
    public class PawnExtended : IExposable
    {
        private Dictionary<int, AreaStates> _areaStatesByMap; 
        
        public Dictionary<int, AreaStates> AreaStatesByMap => _areaStatesByMap;

        public Pawn Pawn { get; set; }

        public PawnExtended()
        {
        }

        public PawnExtended(Pawn pawn)
        {
            Pawn = pawn;
            _areaStatesByMap = new Dictionary<int, AreaStates>();
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _areaStatesByMap, "areaStatesByMap");

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs) return;
            
            foreach (var areaState in AreaStatesByMap)
                areaState.Value.MapExtended = AreaHelper.Current.MapExtended[areaState.Key];
        }
    }
}