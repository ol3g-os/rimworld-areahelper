using AreaHelper.Data;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AreaHelper
{
    public static class Events
    {
        public static void FireGameLoad()
        {
            AreaHelper.LogMessage("FireGameLoad");
            AreaHelper.Current.ExposeData();
        }

        public static void FireGameDispose()
        {
            AreaHelper.LogMessage("FireGameDispose");
            AreaHelper.Current.Dispose();
        }

        public static void FireGameSave()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars) return;
            AreaHelper.LogMessage("FireGameSave");
            AreaHelper.Current.ExposeData();
        }
        
        public static void FireMapDispose(Map map)
        {
            AreaHelper.LogMessage("FireMapDispose " + map.uniqueID);

            var extended = AreaHelper.Current.GetExtended(map);
            if (extended == null) return;
            
            extended.Dispose();
            AreaHelper.Current.MapExtended.Remove(map.uniqueID);
        }

        public static void FireAreaChanged(Area area, IntVec3 c, bool val)
        {
            AreaHelper.Current.GetExtended(area)?.FireChanged(c, val);
        }

        public static void FireAreaRemoved(Area area)
        {
            AreaHelper.Current.GetExtended(area)?.FireRemoved();
        }

        public static void FireAreaInvert(Area area)
        {
            AreaHelper.Current.GetExtended(area)?.FireInvert();
        }
        
        public static void FireExposeData(Map map)
        {
            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs) return;
            
            var extended = AreaHelper.Current.GetExtended(map);
            if (extended != null)
                extended.Map = map;
        }

        public static void FireExposeData(Area area)
        {
            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs) return;
            
            var extended = AreaHelper.Current.GetExtended(area);
            if (extended != null)
                extended.Area = area;
        }

        public static void FireExposeData(Pawn pawn)
        {
            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs) return;
            
            var extended = AreaHelper.Current.GetExtended(pawn);
            if (extended != null)
                extended.Pawn = pawn;
        }

        public static void FireBeforeExposeData(AreaManager areaManager)
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                areaManager.AllAreas.RemoveAll(x => x is AreaCombined);
            }
        }

        public static void FireExposeData(AreaManager areaManager)
        {
            var mapExtended = AreaHelper.Current.GetExtended(areaManager.map);
            if (mapExtended == null) return;
            
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                areaManager.AllAreas.AddRange(mapExtended.Areas);
            }
        }

        public static void FireAreaManagerUpdate(AreaManager areaManager)
        {
            var map = AreaHelper.Current.GetExtended(areaManager.map);
            if (map == null) return;

            foreach (var area in map.Areas)
                area.AreaUpdateAll();
        }

        public static bool FireBeforeAreaRestrictionInPawnCurrentMapSet(Pawn pawn)
        {
            // if key pressed we set include/exclude, default area(white) change is prevented
            if (IsSelectionActive())
                return false;
            
            var area = pawn.playerSettings.AreaRestrictionInPawnCurrentMap;
            if (area == null)
                return true;
            
            Tasks.SelectArea(area, pawn, AreaSelectState.Remove, AreaStateLayer.Default);
            
            return true;
        }

        public static void FireAreaRestrictionInPawnCurrentMapSet(Pawn pawn)
        {
            if (IsSelectionActive())
                return;
            
            var area = pawn.playerSettings.AreaRestrictionInPawnCurrentMap;
            if (area == null) 
                return;
            
            Tasks.SelectArea(area, pawn, AreaSelectState.Include,  AreaStateLayer.Default);
        }

        private static bool IsSelectionActive()
        {
            // if key pressed we set include/exclude, default area(white) change is prevented
            if (!Event.current.shift && !Event.current.alt) 
                return false;
            
            return Traverse.Create(typeof(AreaAllowedGUI)).Field("dragging").GetValue<bool>();
        }
    }
}