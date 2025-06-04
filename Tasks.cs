using AreaHelper.Data;
using AreaHelper.Extended;
using UnityEngine;
using Verse;

namespace AreaHelper
{
    public static class Tasks
    {
        public static void SelectArea(Area area, Pawn p, AreaSelectState selectState, AreaStateLayer layer = AreaStateLayer.Combined)
        {
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

            AreaHelper.LogMessage($"{nameof(SelectArea)}: select state={selectState}, layer={layer}, area={area}");
            
            if (selectState == AreaSelectState.Remove)
                pawnAreaStates.Remove(areaExtended, layer);
            else
                pawnAreaStates.Toggle(areaExtended, selectState == AreaSelectState.Include, layer);
            
            var key = pawnAreaStates.Key;
            
            AreaHelper.LogMessage($"{nameof(SelectArea)}: new key={key}");
            
            if (key == null)
            {
                pawnAreaStates.AreaCombined = null;
                return;
            }

            var areaCombined = mapExtended.Areas.FirstOrDefault(x => x.AreaStates.Key == key);
            if (areaCombined == null) // only create area, not change if found
            {
                AreaHelper.LogMessage("Add new combined " + key);
                mapExtended.Areas.Add(areaCombined = new AreaCombined(mapExtended, pawnAreaStates));
            }

            pawnAreaStates.AreaCombined = areaCombined;
            // p.playerSettings.AreaRestrictionInPawnCurrentMap = areaCombined; // we save only in areaHelper node
        }
        
        public static void DoAreaSelector(Rect rect, Area area, Pawn p, bool dragging)
        {
            AreaStates areaStates = null;
            AreaState areaState = null;
            var hasCombinedState = false;
            var combinedState = false;
            
            if (AreaHelper.Current.Pawns.TryGetValue(p.ThingID, out var pawnExtended) &&
                pawnExtended.AreaStatesByMap.TryGetValue(p.Map.uniqueID, out areaStates) &&
                area != null && areaStates.States.TryGetValue(area.ID, out areaState) &&
                (hasCombinedState = areaState.TryGetLayerState(AreaStateLayer.Combined, out combinedState)))
            {
                Widgets.DrawBox(rect, 2, combinedState ? Textures.GreenTex : Textures.RedTex);
            }
            
            var isMouseUp = Event.current.rawType == EventType.MouseUp;
            var isLastSelected = area == _lastSelectedArea && p == _lastSelectedPawn;
            if (isMouseUp && isLastSelected)
            {
                _lastSelectedArea = null;
                _lastSelectedPawn = null;
                return;
            }

            if (!Mouse.IsOver(rect)) return;
            
            var shiftPressed = Event.current.shift;
            var altPressed = Event.current.alt;
            var keyPressed = shiftPressed || altPressed;
            if (keyPressed)
                areaStates?.AreaCombined?.AreaStates.MarkForDraw();
            
            if (area == null || isLastSelected || !dragging || !keyPressed)
                return;

            if (_lastSelectedArea == null || _lastSelectedPawn == null)
            {
                _currentAreaSelectState = shiftPressed ? AreaSelectState.Include : AreaSelectState.Exclude;
                if (areaState != null && hasCombinedState && combinedState == shiftPressed)
                    _currentAreaSelectState = AreaSelectState.Remove;
            }
            
            if (!(_currentAreaSelectState == AreaSelectState.Remove && !hasCombinedState))
                SelectArea(area, p, _currentAreaSelectState);
            
            _lastSelectedArea = area;
            _lastSelectedPawn = p;
        }

        private static AreaSelectState _currentAreaSelectState;
        private static Area _lastSelectedArea;
        private static Pawn _lastSelectedPawn;

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