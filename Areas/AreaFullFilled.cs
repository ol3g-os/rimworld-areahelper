using UnityEngine;
using Verse;

namespace AreaHelper.Areas
{
    public class AreaFullFilled : Area
    {
        public override string GetUniqueLoadID() => _uniqueId;
        
        public override string Label => "AreaFullFilled";
        
        public override Color Color => Color.green;

        public override int ListPriority => 500;

        public override bool Mutable => false;

        private string _uniqueId;

        public AreaFullFilled()
        {
        }

        public AreaFullFilled(AreaManager areaManager) : base(areaManager)
        {
            ID = -9999;
            _uniqueId = $"AreaFullFilled_{areaManager.map.uniqueID}_{ID}";
            areaManager.AllAreas.Add(this);
            Invert();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _uniqueId, "uniqueId");
        }
    }
}