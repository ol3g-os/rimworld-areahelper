using HarmonyLib;
using RimWorld;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(Pawn_PlayerSettings))]
    [HarmonyPatch(nameof(Pawn_PlayerSettings.EffectiveAreaRestrictionInPawnCurrentMap))]
    [HarmonyPatch(MethodType.Getter)]
    public class Pawn_PlayerSettings_EffectiveAreaRestrictionInPawnCurrentMap_Patch
    {
        static void Postfix(ref Pawn ___pawn, ref Area __result)
        {
            __result = Tasks.GetPawnArea(___pawn) ?? __result;
        }
    }
}