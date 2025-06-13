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
            Events.FireAreaChanged(__instance, c, val);
        }
    }

    [HarmonyPatch(typeof(Area))]
    [HarmonyPatch(nameof(Area.Invert))]
    public class Area_Invert_Patch
    {
        static void Postfix(Area __instance)
        {
            Events.FireAreaInvert(__instance);
        }
    }

    [HarmonyPatch(typeof(Area))]
    [HarmonyPatch(nameof(Area.ExposeData))]
    public class Area_ExposeData_Patch
    {
        static void Postfix(Area __instance)
        {
            Events.FireExposeData(__instance);
        }
    }

    [HarmonyPatch(typeof(Area))]
    [HarmonyPatch(nameof(Area.MarkForDraw))]
    public class Area_MarkForDraw_Patch
    {
        static bool Prefix(Area __instance)
        {
            var keyPressed = Event.current.alt || Event.current.shift;
            var isCoreArea = __instance is Area_Allowed || __instance is Area_Home;
            return !keyPressed || !isCoreArea;
        }
    }
}