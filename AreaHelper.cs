using System.Collections.Generic;
using System.Linq;
using AreaHelper.Areas;
using AreaHelper.Data;
using Verse;

namespace AreaHelper
{
    public class AreaHelper : GameComponent
    {
        public static AreaHelper Instance { get; private set; }
        
        public Dictionary<int, PawnAreaStates> GravshipTransfer;
        
        public MapIndexedStore<int, PawnAreaStates> PawnStatesByMap { get; } = new MapIndexedStore<int, PawnAreaStates>();

        public MapIndexedStore<string, AreaCombined> AreaCombinedByMap { get; } = new MapIndexedStore<string, AreaCombined>();

        public Dictionary<int, AreaFullFilled> AreaFullFilledByMap = new Dictionary<int, AreaFullFilled>();
        
        public AreaHelper(Game game)
        {
            AreaHelperLogic.LogMessage($"initialized for game: {game}"); 
            Instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            
            PawnStatesByMap.ExposeData("pawnStatesByMap");
    
            Scribe_Collections.Look(ref AreaFullFilledByMap, "areaFullFilledByMap", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref GravshipTransfer, "gravshipTransfer", LookMode.Value, LookMode.Deep);

            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

            var validMapIds = new HashSet<int>(Find.Maps.Select(m => m.uniqueID));
            PawnStatesByMap.Data.RemoveAll(x => !validMapIds.Contains(x.Key));

            foreach (var pawnStatesDict in PawnStatesByMap.Data.Values.ToList())
            {
                pawnStatesDict.Data.RemoveAll(x => !x.Value.IsValid());

                foreach (var statesDict in pawnStatesDict.Data.Values.ToList())
                    statesDict.States.RemoveAll(x => !x.Value.IsValid());
            }

            foreach (var map in Find.Maps.Where(x => AreaFullFilledByMap.ContainsKey(x.uniqueID)))
            {
                var areaFullFilled = AreaFullFilledByMap[map.uniqueID];
                map.areaManager.AllAreas.Add(areaFullFilled);
                areaFullFilled.areaManager = map.areaManager;
            }

            GravshipTransfer?.RemoveAll(x => !x.Value.IsAlive());
        }
    }
}