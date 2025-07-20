using LudeonTK;
using Verse;

namespace AreaHelper
{
    public static class DebugActions
    {
        [DebugAction(nameof(AreaHelper), nameof(AllCombinedKeys), actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void AllCombinedKeys()
        {
            foreach (var mapExtended in AreaHelper.Current.MapExtended.Values)
            {
                AreaHelper.LogMessage($"map={mapExtended.Map.uniqueID}");
                foreach (var areaCombined in mapExtended.Areas)
                {
                    AreaHelper.LogMessage($"key={areaCombined.AreaStates.Key}");
                }
            }
        }
        
        [DebugAction(nameof(AreaHelper), nameof(AllAreas), actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void AllAreas()
        {
            foreach (var mapExtended in AreaHelper.Current.MapExtended.Values)
            {
                AreaHelper.LogMessage($"map={mapExtended.Map.uniqueID}");
                foreach (var area in mapExtended.Map.areaManager.AllAreas)
                {
                    AreaHelper.LogMessage($"key={area.ID} caption={area.Label}");
                }
            }
        }
        
        [DebugAction(nameof(AreaHelper), nameof(AllPawnAreas), actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void AllPawnAreas()
        {
            foreach (var pawn in AreaHelper.Current.Pawns.Values)
            {
                AreaHelper.LogMessage($"Pawn={pawn.Pawn.ThingID}");
                foreach (var states in pawn.AreaStatesByMap)
                {
                    AreaHelper.LogMessage($"key={states.Key} value={states.Value}");
                }
            }
        }
    }
}