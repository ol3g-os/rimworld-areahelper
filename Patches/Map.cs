using HarmonyLib;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(Map))]
    [HarmonyPatch(nameof(Map.Dispose))]
    public class Map_Dispose_Patch
    {
        public static void Postfix(Map __instance)
        {
            Events.FireMapDispose(__instance);
        }
    }

    [HarmonyPatch(typeof(Map))]
    [HarmonyPatch(nameof(Map.ExposeData))]
    public class Map_ExposeData_Patch
    {
        static void Postfix(Map __instance)
        {
            Events.FireExposeData(__instance);
        }
    }
}