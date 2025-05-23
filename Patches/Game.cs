using HarmonyLib;
using Verse;

namespace AreaHelper.Patches
{
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.LoadGame))]
    public class Game_LoadGame_Patch
    {
        static void Prefix()
        {
            Events.FireGameLoad();
        }
    }
    
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.Dispose))]
    public class Game_Dispose_Patch
    {
        static void Postfix()
        {
            Events.FireGameDispose();
        }
    }
    
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.ExposeData))]
    public class Game_ExposeData_Patch
    {
        static void Postfix()
        {
            Events.FireGameSave();
        }
    }
}