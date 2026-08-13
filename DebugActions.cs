using LudeonTK;
using Verse;

namespace AreaHelper
{
    public static class DebugActions
    {
        private static void LogMessage(string message, int indent = 0) => AreaHelperLogic.LogMessage(message, indent, true); 
        
        [DebugAction(nameof(AreaHelper), nameof(LogAreasCombinedInfo), actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void LogAreasCombinedInfo()
        {
            // Map: id
            //   AreaCombined: key
            //     UsingAreas: count
            //       AreaName(id): state
            //     UsedByPawns: count
            //       Name1(id)
            //       Name2(id)
            
            LogMessage($"==={nameof(LogAreasCombinedInfo)}===");

            var areaData = AreaHelper.Instance.AreaCombinedByMap.Data;

            foreach (var (mapId, areaCombinedByKey) in areaData)
            {
                LogMessage($"Map: {mapId}");
                
                foreach (var (stateKey, areaCombined) in areaCombinedByKey.Data)
                {
                    LogMessage($"AreaCombined: {stateKey}", 1);
                    
                    LogMessage($"UsingAreas: {areaCombined.AreaStates.States.Count}", 2);
                    
                    foreach (var (_, areaState) in areaCombined.AreaStates.States)
                    {
                        LogMessage($"{areaState.Area.Label}({areaState.Area.ID}): {areaState.Include}", 3);
                    }
                    
                    var usedByPawns = AreaHelperLogic.GetPawnAreaStatesByStateKey(areaCombined.Map, stateKey);

                    LogMessage($"UsedByPawns: {usedByPawns.Count}", 2);
                    
                    foreach (var pawnAreaStates in usedByPawns)
                    {
                        var pawn = pawnAreaStates.Pawn;
                        LogMessage($"{pawn.LabelShort}({pawn.thingIDNumber})", 3);
                    }
                }
            }
        }
        
        [DebugAction(nameof(AreaHelper), nameof(LogPawnsStatesInfo), actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void LogPawnsStatesInfo()
        {
            // Map: id
            //   Pawn: Name(id), stateKey
            
            LogMessage($"==={nameof(LogPawnsStatesInfo)}===");
            
            var pawnData = AreaHelper.Instance.PawnStatesByMap.Data;

            foreach (var (mapId, pawnAreaStatesByPawnId) in pawnData)
            {
                LogMessage($"Map: {mapId}");

                foreach (var (_, pawnAreaStates) in pawnAreaStatesByPawnId.Data)
                {
                    var pawn = pawnAreaStates.Pawn;
                    LogMessage($"Pawn: {pawn.LabelShort}({pawn.thingIDNumber}) {pawnAreaStates.Key}", 1);
                }
            }
        }
    }
}