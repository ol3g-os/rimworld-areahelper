using HarmonyLib;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(AreaManager))]
    [HarmonyPatch("Remove")]
    public class AreaManager_Remove_Patch
    {
        static void Postfix(Area area)
        {
            Events.FireAreaRemoved(area);
        }
    }
    [HarmonyPatch(typeof(AreaManager))]
    [HarmonyPatch(nameof(AreaManager.AreaManagerUpdate))]
    public class AreaManager_AreaManagerUpdate_Patch
    {
        static void Postfix(AreaManager __instance)
        {
            Events.FireAreaManagerUpdate(__instance);
        }
    }
}