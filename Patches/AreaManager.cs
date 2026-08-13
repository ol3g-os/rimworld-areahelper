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
            AreaHelperLogic.OnAreaRemoved(area);
        }
    }
    
    [HarmonyPatch(typeof(AreaManager))]
    [HarmonyPatch(nameof(AreaManager.ExposeData))]
    public class AreaManager_ExposeData_Patch
    {
        static void Prefix(AreaManager __instance)
        {
            AreaHelperLogic.OnBeforeExposeData(__instance);
        }
        
        static void Postfix(AreaManager __instance)
        {
            AreaHelperLogic.OnExposeData(__instance);
        }
    }
    
    [HarmonyPatch(typeof(AreaManager))]
    [HarmonyPatch(nameof(AreaManager.AreaManagerUpdate))]
    public class AreaManager_AreaManagerUpdate_Patch
    {
        static void Postfix(AreaManager __instance)
        {
            AreaHelperLogic.OnAreaManagerUpdate(__instance);
        }
    }
}