using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(AreaAllowedGUI))]
    [HarmonyPatch("DoAreaSelector")]
    public class AreaAllowedGUI_DoAreaSelector_Patch
    {
        private static void Postfix(Rect rect, Pawn p, Area area, ref bool ___dragging)
        {
            Tasks.DoAreaSelector(rect, area, p, ___dragging);
        }
    }
}