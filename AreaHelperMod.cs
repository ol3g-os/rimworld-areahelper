using HarmonyLib;
using Verse;

namespace AreaHelper
{
    [StaticConstructorOnStartup]
    public class AreaHelperMod : Mod
    {
        public AreaHelperMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(nameof(AreaHelper));
            harmony.PatchAll();
        }
    }
}