using System;
using System.Collections.Generic;
using AreaHelper.Extended;
using HarmonyLib;
using Verse;

namespace AreaHelper
{
    [StaticConstructorOnStartup]
    public class AreaHelper : IExposable, IDisposable
    {
        private static AreaHelper _current;
        
        public static AreaHelper Current => _current ?? (_current = new AreaHelper());

        static AreaHelper()
        {
            var harmony = new Harmony(nameof(AreaHelper));
            harmony.PatchAll();
        }

        public static void LogMessage(string message)
        {
            if (Prefs.LogVerbose)
                Log.Message($"{nameof(AreaHelper)}: {message}");
        }
        
        private Dictionary<int, MapExtended> _mapExtended = new Dictionary<int, MapExtended>();
        
        public Dictionary<int, MapExtended> MapExtended => _mapExtended;
        
        private Dictionary<string, PawnExtended> _pawns = new Dictionary<string, PawnExtended>();

        public Dictionary<string, PawnExtended> Pawns => _pawns;
        
        public MapExtended GetExtended(Map map)
        {
            return _mapExtended.TryGetValue(map.uniqueID, out var extended) ? extended : null;
        }

        public AreaExtended GetExtended(Area area)
        {
            var mapExtended = GetExtended(area.Map);
            if (mapExtended == null) return null;

            return mapExtended.AreaExtended.TryGetValue(area.ID, out var extended) ? extended : null;
        }

        public PawnExtended GetExtended(Pawn pawn)
        {
            return _pawns.TryGetValue(pawn.ThingID, out var extended) ? extended : null;
        }

        public void ExposeData()
        {
            if (!Scribe.EnterNode("areaHelper"))
            {
                LogMessage("AreaHelper ExposeData failed");
                return;
            }

            try
            {
                Scribe_Collections.Look(ref _mapExtended, "maps");
                Scribe_Collections.Look(ref _pawns, "pawns");
            }
            finally
            {
                Scribe.ExitNode();
            }
        }

        public void Dispose()
        {
            foreach (var mapExtended in _mapExtended.Values)
                mapExtended.Dispose();
            
            _current = null;
        }
    }
}