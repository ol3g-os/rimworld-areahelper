using HarmonyLib;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.ExposeData))]
    public class Pawn_ExposeData_Patch
    {
        static void Postfix(Pawn __instance)
        {
            Events.FireExposeData(__instance);
        }
    }
}