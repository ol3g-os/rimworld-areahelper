using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(Gravship), "CopyAreas")]
    public static class Gravship_CopyAreas_Patch
    {
        static void Prefix(Map oldMap, IntVec3 origin, HashSet<IntVec3> engineFloors)
        {   
            AreaHelperLogic.OnGravshipCopyAreas(oldMap);
        }
    }
}