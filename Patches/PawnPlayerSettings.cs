using HarmonyLib;
using RimWorld;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(Pawn_PlayerSettings))]
    [HarmonyPatch(nameof(Pawn_PlayerSettings.EffectiveAreaRestrictionInPawnCurrentMap))]
    [HarmonyPatch(MethodType.Getter)]
    public class Pawn_PlayerSettings_EffectiveAreaRestrictionInPawnCurrentMap_Get_Patch
    {
        static void Postfix(ref Pawn ___pawn, ref Area __result)
        {
            __result = Tasks.GetPawnArea(___pawn) ?? __result;
        }
    }
    
    [HarmonyPatch(typeof(Pawn_PlayerSettings))]
    [HarmonyPatch(nameof(Pawn_PlayerSettings.AreaRestrictionInPawnCurrentMap))]
    [HarmonyPatch(MethodType.Setter)]
    public class Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap_Set_Patch
    {
        static bool Prefix(ref Pawn ___pawn)
        {
            return Events.FireBeforeAreaRestrictionInPawnCurrentMapSet(___pawn);;
        }
        
        static void Postfix(ref Pawn ___pawn)
        {
            Events.FireAreaRestrictionInPawnCurrentMapSet(___pawn);
        }
    }
}