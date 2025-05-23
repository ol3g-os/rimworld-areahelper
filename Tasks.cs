using AreaHelper.Data;
using AreaHelper.Extended;
using UnityEngine;
using Verse;

namespace AreaHelper
{
    public static class Tasks
    {
        private static void SelectArea(Area area, Pawn p)
        {
            if (!Event.current.shift && !Event.current.alt || Event.current.rawType != EventType.MouseUp) return;

            var current = AreaHelper.Current;
            var currentPawns = current.Pawns;
            var currentMaps = current.MapExtended;
            var thingId = p.ThingID;
            var map = area.Map;
            var mapId = map.uniqueID;
            var areaId = area.ID;
            
            if (!currentPawns.TryGetValue(thingId, out var pawnExtended))
                currentPawns.Add(thingId, pawnExtended = new PawnExtended(p));
            
            if (!currentMaps.TryGetValue(mapId, out var mapExtended))
                currentMaps.Add(mapId, mapExtended = new MapExtended(map));

            if (!mapExtended.AreaExtended.TryGetValue(areaId, out var areaExtended))
                mapExtended.AreaExtended.Add(areaId, areaExtended = new AreaExtended(area));
            
            if (!pawnExtended.AreaStatesByMap.TryGetValue(mapId, out var pawnAreaStates))
                pawnExtended.AreaStatesByMap.Add(mapId, pawnAreaStates = new AreaStates(mapExtended));
            
            var include = Event.current.shift;
            if (pawnAreaStates.States.TryGetValue(areaId, out var areaState) && areaState.Include == include)
            {
                pawnAreaStates.Remove(areaExtended);
            }
            else
            {
                pawnAreaStates.Toggle(areaExtended, include);
            }
            
            var key = pawnAreaStates.Key;
            if (key == null)
            {
                pawnAreaStates.AreaCombined = null;
                return;
            }

            var areaCombined = mapExtended.Areas.FirstOrDefault(x => x.AreaStates.Key == key);
            
            if (areaCombined == null)
            {
                AreaHelper.LogMessage("Add new combined " + key);
                mapExtended.Areas.Add(areaCombined = new AreaCombined(mapExtended));

                foreach (var keyValueArea in pawnAreaStates.States)
                    areaCombined.AreaStates.Toggle(keyValueArea.Value.AreaExtended, keyValueArea.Value.Include);
            }
            else
            {
                AreaHelper.LogMessage("Change exists combined " + key);
                areaCombined.AreaStates.Toggle(areaExtended, include);
            }

            pawnAreaStates.AreaCombined = areaCombined;
            // p.playerSettings.AreaRestrictionInPawnCurrentMap = areaCombined; // we save only in areaHelper node
        }
        
        public static void DoAreaSelector(Rect rect, Area area, Pawn p)
        {
            if (area == null) return;

            AreaStates areaStates = null;
            if (AreaHelper.Current.Pawns.TryGetValue(p.ThingID, out var pawnExtended) &&
                pawnExtended.AreaStatesByMap.TryGetValue(area.Map.uniqueID, out areaStates) &&
                areaStates.States.TryGetValue(area.ID, out var areaState))
            {
                Widgets.DrawBox(rect, 2, areaState.Include ? Textures.GreenTex : Textures.RedTex);
            }

            if (!Mouse.IsOver(rect)) return;

            if ((Event.current.shift || Event.current.alt) && areaStates != null)
                areaStates.AreaCombined?.MarkForDraw();

            SelectArea(area, p);
        }

        public static Area GetPawnArea(Pawn pawn)
        {
            if (Prefs.LogVerbose)
                AreaHelper.LogMessage("GetPawnArea");
            
            var pawnExtended = AreaHelper.Current.GetExtended(pawn);
            if (pawnExtended == null) 
                return null;
            
            if (!pawnExtended.AreaStatesByMap.TryGetValue(pawn.Map.uniqueID, out var areaStates))
                return null;
            
            if (Prefs.LogVerbose)
                AreaHelper.LogMessage($"PawnArea {pawn.ThingID}={areaStates.AreaCombined}");
            
            return areaStates.AreaCombined;
        }
    }
}