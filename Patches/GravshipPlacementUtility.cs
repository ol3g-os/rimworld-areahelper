using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(GravshipPlacementUtility), nameof(GravshipPlacementUtility.PlaceGravshipInMap))]
    public static class GravshipPlacementUtility_PlaceGravshipInMap_Patch
    {
        static void Postfix(Gravship gravship, Map map)
        {
            try
            {
                AreaHelperLogic.OnPlaceGravshipInMap(gravship, map);
            }
            catch (ArgumentException ex)
            {
                Log.Error(
                    $"[AreaHelper] Migration failed (maybe duplicate key).\n" +
                    $"mapId={map.uniqueID}, gravshipLoadId={gravship.GetUniqueLoadID()}.\n" +
                    $"This is a bug: migration must run once.\n" +
                    $"{ex.Message}\n" +
                    $"{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Log.Error(
                    $"[AreaHelper] Unexpected error during migration.\n" +
                    $"mapId={map.uniqueID}, gravshipLoadId={gravship.GetUniqueLoadID()}.\n" +
                    $"{ex.Message}\n" +
                    $"{ex.StackTrace}");
            }
        }
    }
}