using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(Area))]
    [HarmonyPatch("Set")]
    public class Area_Set_Patch
    {
        static void Postfix(Area __instance, IntVec3 c, bool val)
        {
            AreaHelperLogic.OnAreaChanged(__instance, c);
        }
    }

    [HarmonyPatch(typeof(Area))]
    [HarmonyPatch(nameof(Area.Invert))]
    public class Area_Invert_Patch
    {
        static void Postfix(Area __instance)
        {
            AreaHelperLogic.OnAreaInvert(__instance);
        }
    }

    [HarmonyPatch(typeof(Area))]
    [HarmonyPatch(nameof(Area.MarkForDraw))]
    public class Area_MarkForDraw_Patch
    {
        static bool Prefix(Area __instance)
        {
            return AreaHelperLogic.OnBeforeMarkForDraw(__instance);
        }
    }
}