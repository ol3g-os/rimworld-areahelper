using System.Linq;
using AreaHelper.Data;
using UnityEngine;
using Verse;

namespace AreaHelper.Areas
{
    public class AreaCombined : Area
    {
        public override string GetUniqueLoadID() => $"{nameof(AreaCombined)}_{ID}";
        
        public override string Label => $"grouped_{ID}";
        
        public override Color Color => Color.green;
        
        public override bool Mutable => false;
        
        public override int ListPriority => 500;

        public AreaStates AreaStates { get; }

        private readonly AreaExcluded _areaExcluded;

        public AreaCombined(Map map, AreaStates areaStates) : base(map.areaManager)
        {
            _areaExcluded = new AreaExcluded(areaManager);
            AreaStates = new AreaStates(areaStates);
            map.areaManager.AllAreas.Add(this);
            Calculate();
        }

        public void MarkForDrawAll(bool drawIncluded = true)
        {
            if (Prefs.LogVerbose)
                AreaHelperLogic.LogMessage($"Mark for draw {AreaStates.Key} {drawIncluded}");
                
            if (drawIncluded)
                MarkForDraw();
            
            _areaExcluded.MarkForDraw();
        }

        public void AreaUpdateAll()
        {
            AreaUpdate();
            _areaExcluded.AreaUpdate();
        }

        public void Calculate(IntVec3 cell)
        {
            if (Prefs.LogVerbose)
                AreaHelperLogic.LogMessage($"Try calculate cell: x{cell.x} y{cell.y} z{cell.z}");
            
            var state = AreaStates.HasOnlyExcludedLayers();
            var excludedState = false;

            foreach (var areaState in AreaStates.States.Values.Where(x => x.Area[cell]))
            {
                state = areaState.Include;
                if (state) continue;
                
                excludedState = true;
                break;
            }

            if (Prefs.LogVerbose)
                AreaHelperLogic.LogMessage($"Calculate result ${state}");
            
            this[cell] = state;
            
            _areaExcluded.Calculate(cell, excludedState);
        }

        public void Calculate()
        {
            var orderedAreas = AreaStates.States.OrderByDescending(x => x.Value.Include);
            
            foreach (var allCell in ActiveCells)
                this[allCell] = false;
            
            if (AreaStates.HasOnlyExcludedLayers())
                Invert();
            
            foreach (var areaState in orderedAreas)
                foreach (var cell in areaState.Value.Area.ActiveCells)
                    this[cell] = areaState.Value.Include;

            _areaExcluded.Calculate(AreaStates);
        }
    }
}