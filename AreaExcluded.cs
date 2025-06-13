using System.Linq;
using AreaHelper.Data;
using UnityEngine;
using Verse;

namespace AreaHelper
{
    public class AreaExcluded : Area
    {
        public override string GetUniqueLoadID() => $"{nameof(AreaExcluded)}_{ID}";
        
        public override string Label => $"grouped_excluded_{ID}";
        
        public override Color Color => Color.red;

        public override int ListPriority => 500;

        public override bool Mutable => true;
        
        public AreaExcluded(AreaManager areaManager) : base(areaManager)
        {
        }

        public void Calculate(IntVec3 cell, bool state)
        {
            this[cell] = state;
        }

        public void Calculate(AreaStates areaStates)
        {
            var excluded = areaStates.States.Where(x => !x.Value.Include);
            
            foreach (var allCell in ActiveCells)
                this[allCell] = false;
            
            foreach (var areaState in excluded)
                foreach (var cell in areaState.Value.Area.ActiveCells)
                    this[cell] = true;
        }
    }
}
