using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace AreaHelper.Data
{
    public class PawnAreaStates : AreaStates
    {
        private Map _map;
        private Pawn _pawn;
        
        public Pawn Pawn => _pawn;
        
        public Map Map => _map;

        [UsedImplicitly]
        public PawnAreaStates()
        {
        }

        public PawnAreaStates(Map map, Pawn pawn)
        {
            _map = map;
            _pawn = pawn;
        }
        
        public PawnAreaStates(PawnAreaStates pawnAreaStates) : base(pawnAreaStates)
        {
            _map = pawnAreaStates._map;
            _pawn = pawnAreaStates._pawn;
        }
        
        public PawnAreaStates(PawnAreaStates pawnAreaStates, Map map, Dictionary<int, Area> idMapping)
        {
            _map = map;
            _pawn = pawnAreaStates._pawn;
            
            AreaHelperLogic.LogMessage($"Try migrate areas from old map. States count on old map equals '{pawnAreaStates.States.Count}'");

            foreach (var (oldId, state) in pawnAreaStates.States)
            {
                AreaHelperLogic.LogMessage($"Try add new area with old id '{oldId}'");
                
                if (idMapping.TryGetValue(oldId, out var newArea))
                    States[newArea.ID] = new AreaState(state, newArea);
                else
                    AreaHelperLogic.LogMessage($"Area with new id '{oldId}' not found");
            }

            BuildKey();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref _pawn, "pawn");
            Scribe_References.Look(ref _map, "map");
        }

        public bool IsValid()
        {
            return IsAlive() && _map != null && !string.IsNullOrEmpty(Key);
        }

        public bool IsAlive()
        {
            return _pawn != null && !_pawn.Destroyed;
        }
    }
}