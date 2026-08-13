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
            AreaHelperLogic.OnMapDispose(__instance);
        }
    }
}