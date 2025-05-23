using LudeonTK;
using Verse;

namespace AreaHelper
{
    public static class DebugActions
    {
        [DebugAction(nameof(AreaHelper), nameof(AllCombinedKeys), actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        public static void AllCombinedKeys()
        {
            foreach (var mapExtended in AreaHelper.Current.MapExtended.Values)
            {
                AreaHelper.LogMessage($"map={mapExtended.Map.uniqueID}");
                foreach (var areaCombined in mapExtended.Areas)
                {
                    AreaHelper.LogMessage($"key={areaCombined.AreaStates.Key}");
                }
            }
        }
    }
}