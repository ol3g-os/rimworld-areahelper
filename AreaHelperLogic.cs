using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AreaHelper.Areas;
using AreaHelper.Data;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AreaHelper
{
    public static class AreaHelperLogic
    {
        private static readonly FieldInfo MoveableAreaLabel = AccessTools.Field(typeof(MoveableArea), "label");
        private static readonly FieldInfo MoveableAreaId = AccessTools.Field(typeof(MoveableArea), "id");
        
        private static bool _selectionActive;
        
        private static AreaHelper Instance => AreaHelper.Instance;

        public static void LogMessage(string message, int indent = 0, bool force = false)
        {
            if (!Prefs.LogVerbose && !force) return;

            Log.Message($"[{nameof(AreaHelper)}] {new string(' ', indent*3)}{message}");
        }

        private static IEnumerable<AreaCombined> GetCombinedAreasWithArea(Area area)
        {
            var areas = GetCombinedAreasByMap(area.Map)?.Values;
            return areas?.Where(x => x.AreaStates.States.ContainsKey(area.ID));
        }

        private static AreaFullFilled GetOrAddAreaFullFilled(Map map)
        {
            if (!Instance.AreaFullFilledByMap.TryGetValue(map.uniqueID, out var areaFullFilled))
            {
                areaFullFilled = new AreaFullFilled(map.areaManager);
                Instance.AreaFullFilledByMap.Add(map.uniqueID, areaFullFilled);
            }
            return areaFullFilled;
        }

        public static AreaFullFilled GetAreaFullFilled(Map map)
        {
            Instance.AreaFullFilledByMap.TryGetValue(map.uniqueID, out var areaFullFilled);
            return areaFullFilled;
        }

        private static PawnAreaStates GetOrAddPawnAreaStates(Map map, Pawn pawn)
        {
            return Instance.PawnStatesByMap.GetOrAdd(map, pawn.thingIDNumber, () => new PawnAreaStates(map, pawn));
        }

        private static PawnAreaStates GetPawnAreaStates(Map map, Pawn pawn)
        {
            return Instance.PawnStatesByMap.Get(map.uniqueID, pawn.thingIDNumber);
        }

        private static AreaCombined GetOrAddAreaCombined(Map map, PawnAreaStates pawnAreaStates)
        {
            return Instance.AreaCombinedByMap.GetOrAdd(map, pawnAreaStates.Key, () => new AreaCombined(map, pawnAreaStates));
        }

        private static AreaCombined GetAreaCombined(Map map, PawnAreaStates pawnAreaStates)
        {
            return Instance.AreaCombinedByMap.Get(map.uniqueID, pawnAreaStates.Key);
        }

        private static IReadOnlyDictionary<string, AreaCombined> GetCombinedAreasByMap(Map map)
        {
            return Instance.AreaCombinedByMap.GetByIndex(map.uniqueID);
        }

        public static List<PawnAreaStates> GetPawnAreaStatesByStateKey(Map map, string key)
        {
            return Instance.PawnStatesByMap.GetByIndex(map.uniqueID)?.Values
                .Where(x => x.Key == key).ToList() ?? new List<PawnAreaStates>();
        }

        public static Area OnGetPawnArea(Pawn pawn)
        {
            var map = pawn.MapHeld;
            if (map == null) 
                return null;

            var pawnAreaStates = GetPawnAreaStates(map, pawn);
            if (pawnAreaStates == null || string.IsNullOrEmpty(pawnAreaStates.Key))
                return null;

            return GetOrAddAreaCombined(map, pawnAreaStates);
        }

        public static void OnMapDispose(Map map)
        {
            Instance.PawnStatesByMap.Remove(map.uniqueID);
            Instance.AreaFullFilledByMap.Remove(map.uniqueID);
            Instance.AreaCombinedByMap.Remove(map.uniqueID);
        }

        public static void OnAreaChanged(Area area, IntVec3 c)
        {
            var combinedAreas = GetCombinedAreasWithArea(area);
            if (combinedAreas == null) return;

            foreach (var combinedArea in combinedAreas)
                combinedArea.Calculate(c);
        }

        public static void OnAreaRemoved(Area area)
        {
            var map = area.Map;
            var mapId = map.uniqueID;
            var areas = GetCombinedAreasByMap(map);
            var allAreas = map.areaManager.AllAreas;

            if (areas != null)
            {
                var areasToRemove = new List<AreaCombined>();
                areasToRemove.AddRange(areas.Values.Where(x =>
                    x.AreaStates.States.ContainsKey(area.ID)));

                foreach (var areaCombined in areasToRemove)
                {
                    allAreas.Remove(areaCombined);
                    Instance.AreaCombinedByMap.Remove(map.uniqueID, areaCombined.AreaStates.Key);
                }
            }

            var pawnAreaStatesList = Instance.PawnStatesByMap.GetByIndex(mapId)?.Values;
            if (pawnAreaStatesList == null) return;

            foreach (var pawnAreaStates in pawnAreaStatesList.ToList())
            {
                pawnAreaStates.Remove(area);
                
                if (string.IsNullOrEmpty(pawnAreaStates.Key))
                    Instance.PawnStatesByMap.Remove(map.uniqueID, pawnAreaStates.Pawn.thingIDNumber);
            }
        }

        public static void OnAreaInvert(Area area)
        {
            var combinedAreas = GetCombinedAreasWithArea(area);
            if (combinedAreas == null) return;

            foreach (var combinedArea in combinedAreas)
                combinedArea.Calculate();
        }

        public static void OnBeforeExposeData(AreaManager areaManager)
        {
            if (Scribe.mode != LoadSaveMode.Saving && Scribe.mode != LoadSaveMode.LoadingVars) return;
            areaManager.AllAreas.RemoveAll(x => x is AreaCombined || x is AreaFullFilled);
        }

        public static void OnExposeData(AreaManager areaManager)
        {
            if (Scribe.mode != LoadSaveMode.Saving) return;
            
            var areas = GetCombinedAreasByMap(areaManager.map);
            if (areas != null)
                areaManager.AllAreas.AddRange(areas.Values);
            
            var areaFullFilled = GetAreaFullFilled(areaManager.map);
            if (areaFullFilled != null)
                areaManager.AllAreas.Add(areaFullFilled);
        }

        public static void OnAreaManagerUpdate(AreaManager areaManager)
        {
            var areas = GetCombinedAreasByMap(areaManager.map);
            if (areas == null) return;

            foreach (var area in areas.Values)
                area.AreaUpdateAll();
        }

        public static bool OnBeforeAreaRestrictionInPawnCurrentMapSet(Pawn pawn)
        {
            if (Instance.GravshipTransfer != null)
                return true;
            
            if (_selectionActive)
                return false;

            var area = pawn.playerSettings.AreaRestrictionInPawnCurrentMap;
            if (area == null)
                return true;

            SelectArea(area, pawn, AreaSelectState.Remove, AreaStateLayer.Default);
            return true;
        }

        public static void OnAreaRestrictionInPawnCurrentMapSet(Pawn pawn)
        {
            if (Instance.GravshipTransfer != null)
                return;
            
            if (_selectionActive)
                return;

            var area = pawn.playerSettings.AreaRestrictionInPawnCurrentMap;
            if (area == null)
                return;

            SelectArea(area, pawn, AreaSelectState.Include, AreaStateLayer.Default);
        }

        public static bool OnBeforeMarkForDraw(Area area)
        {
            var isCoreArea = area is Area_Allowed || area is Area_Home;
            return !_selectionActive || !isCoreArea;
        }

        public static void SelectArea(Area area, Pawn pawn, AreaSelectState selectState, AreaStateLayer layer = AreaStateLayer.Combined)
        {
            if (selectState == AreaSelectState.None)
                throw new ArgumentException();

            var map = pawn.MapHeld;
            if (map == null)
                return;
            
            var areaFullFilled = GetOrAddAreaFullFilled(map);
            var pawnAreaStates = GetOrAddPawnAreaStates(map, pawn);
            var oldKey = pawnAreaStates.Key;

            var isTargetFullFilled = area == null || area == areaFullFilled;
            if (isTargetFullFilled)
                area = areaFullFilled;
            
            LogMessage($"{nameof(SelectArea)}: select state={selectState}, layer={layer}, area={area}");

            if (selectState == AreaSelectState.Remove)
            {
                pawnAreaStates.Remove(area, layer);

                if (string.IsNullOrEmpty(pawnAreaStates.Key))
                {
                    Instance.PawnStatesByMap.Remove(map.uniqueID, pawn.thingIDNumber);
                    
                    LogMessage($"{nameof(SelectArea)}: removing PawnStates for pawn '{pawn.LabelShort}' because his key is empty");
                }
            }
            else
                pawnAreaStates.SetOrAdd(area, selectState == AreaSelectState.Include, layer);
            
            LogMessage($"{nameof(SelectArea)}: new key={pawnAreaStates.Key}");
            
            CheckAndUpdateAreaCombinedByKey(map, oldKey);
        }

        private static void CheckAndUpdateAreaCombinedByKey(Map map, string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            var areaCombined = Instance.AreaCombinedByMap.Get(map.uniqueID, key);
            if (areaCombined == null) return;
            
            if (GetPawnAreaStatesByStateKey(map, key).Count != 0) return;
            
            Instance.AreaCombinedByMap.Remove(map.uniqueID, key);
            map.areaManager.AllAreas.Remove(areaCombined);
        }

        public static bool? GetPawnAreaActiveState(Area area, Pawn pawn)
        {
            if (pawn == null || area == null) return null;
            
            var map = pawn.MapHeld;
            if (map == null) 
                return null;

            var pawnStates = GetPawnAreaStates(map, pawn);
            if (pawnStates == null) return null;

            if (!pawnStates.States.TryGetValue(area.ID, out var areaState))
                return null;

            if (!areaState.TryGetLayerState(AreaStateLayer.Combined, out var combinedState))
                return null;

            return combinedState;
        }

        public static AreaSelectState GetAreaSelectStateForPawnActiveState(bool? pawnAreaActiveState, AreaSelectState areaSelectState)
        {
            if (pawnAreaActiveState == false && areaSelectState == AreaSelectState.Exclude
                || pawnAreaActiveState == true && areaSelectState == AreaSelectState.Include)
                areaSelectState = AreaSelectState.Remove;

            return areaSelectState;
        }

        public static void MarkForDraw(Pawn pawn)
        {
            var map = pawn.MapHeld;
            if (map == null) return;
            
            var pawnAreaStates = GetPawnAreaStates(map, pawn);
            if (pawnAreaStates == null) return;

            var areaFullFilled = GetAreaFullFilled(map);
            if (areaFullFilled == null) return;
            
            var areaCombined = GetOrAddAreaCombined(map, pawnAreaStates);
            var hasFullFilledCombinedLayer = pawnAreaStates.HasLayerByArea(areaFullFilled, AreaStateLayer.Combined);
            var hasOnlyExcludedLayers = pawnAreaStates.HasOnlyExcludedLayers();

            areaCombined.MarkForDrawAll(hasOnlyExcludedLayers && hasFullFilledCombinedLayer || !hasOnlyExcludedLayers);
        }

        public static void SetSelectionActive(bool active)
        {
            _selectionActive = active;
        }

        public static void OnGravshipCopyAreas(Map map)
        {
            var pawnStates = Instance.PawnStatesByMap.GetByIndex(map.uniqueID);

            if (pawnStates != null && pawnStates.Count > 0)
                Instance.GravshipTransfer = pawnStates.Where(x => x.Value.IsAlive()).ToDictionary(
                    x => x.Key, 
                    x => new PawnAreaStates(x.Value));
        }

        public static void OnPlaceGravshipInMap(Gravship gravship, Map map)
        {
            try
            {
                OnPlaceGravshipInMapInternal(gravship, map);
            }
            finally
            {
                Instance.GravshipTransfer = null;
            }
        }

        private static void OnPlaceGravshipInMapInternal(Gravship gravship, Map map)
        {
            var instance = Instance;
            var transfer = instance.GravshipTransfer;
            
            if (transfer == null) return;

            var areaFullFilled = new AreaFullFilled(map.areaManager);
            instance.AreaFullFilledByMap[map.uniqueID] = areaFullFilled;
            
            var idMapping = new Dictionary<int, Area>();
            
            foreach (var allowedArea in gravship.areas.allowedAreas)
            {
                var label = (string)MoveableAreaLabel.GetValue(allowedArea);
                var oldId = (int)MoveableAreaId.GetValue(allowedArea);
                LogMessage($"Try find area with label '{label}' and id '{oldId}'");

                var newArea = map.areaManager.GetLabeled(label);
                if (newArea != null)
                    idMapping[oldId] = newArea;
                else
                    LogMessage($"Area with label '{label}' and id '{oldId}' not found on new map");
            }

            idMapping[areaFullFilled.ID] = areaFullFilled;

            foreach (var (pawnId, pawnAreaStates) in transfer)
            {
                var newPawnState = new PawnAreaStates(pawnAreaStates, map, idMapping);
                instance.PawnStatesByMap.Add(map.uniqueID, pawnId, newPawnState);
            }
        }
    }
}