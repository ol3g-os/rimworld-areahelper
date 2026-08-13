using AreaHelper.Data;
using UnityEngine;
using Verse;

namespace AreaHelper
{
    public static class AreaHelperUI
    {
        private static AreaSelectState _lastAreaSelectState;
        private static Area _lastSelectedArea;
        private static Pawn _lastSelectedPawn;

        public static void BeforeDoAreaSelector()
        {
            AreaHelperLogic.SetSelectionActive(Event.current.alt || Event.current.shift);
        }
        
        public static void DoAreaSelector(Rect rect, Area area, Pawn pawn, bool dragging)
        {
            DoAreaSelectorInternal(rect, area, pawn, dragging);
            AreaHelperLogic.SetSelectionActive(false);
        }

        private static void DoAreaSelectorInternal(Rect rect, Area area, Pawn pawn, bool dragging)
        {
            var map = pawn.MapHeld;
            if (map == null)
                return;
            
            if (area == null)
                area = AreaHelperLogic.GetAreaFullFilled(map);

            var pawnAreaActiveState = AreaHelperLogic.GetPawnAreaActiveState(area, pawn);
            if (pawnAreaActiveState.HasValue)
                Widgets.DrawBox(rect, 2, pawnAreaActiveState.Value ? Textures.GreenTex : Textures.RedTex);
            
            if (Event.current.rawType == EventType.MouseUp)
            {
                ClearLastSelection();
                return;
            }
            
            if (!Mouse.IsOver(rect)) 
                return;

            if (!Event.current.shift && !Event.current.alt) 
                return;
            
            AreaHelperLogic.MarkForDraw(pawn);

            if (!dragging)
                return;
            
            if (area == _lastSelectedArea && pawn == _lastSelectedPawn) 
                return;
            
            AreaSelectState areaSelectState;
            var lastAreaSelectState = _lastAreaSelectState;
            if (lastAreaSelectState != AreaSelectState.None)
                areaSelectState = lastAreaSelectState;
            else if (Event.current.alt && Event.current.shift)
                areaSelectState = AreaSelectState.Remove;
            else if (Event.current.alt)
                areaSelectState = AreaHelperLogic.GetAreaSelectStateForPawnActiveState(pawnAreaActiveState, AreaSelectState.Exclude);
            else
                areaSelectState = AreaHelperLogic.GetAreaSelectStateForPawnActiveState(pawnAreaActiveState, AreaSelectState.Include);
            
            AreaHelperLogic.SelectArea(area, pawn, areaSelectState);
            SetLastSelection(area, pawn, areaSelectState);
        }


        private static void SetLastSelection(Area area, Pawn pawn, AreaSelectState selectState)
        {
            _lastSelectedArea = area;
            _lastSelectedPawn = pawn;
            _lastAreaSelectState = selectState;
        }

        private static void ClearLastSelection()
        {
            SetLastSelection(null, null, AreaSelectState.None);
        }
    }
}