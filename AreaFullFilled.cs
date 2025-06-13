using UnityEngine;
using Verse;

namespace AreaHelper
{
    public class AreaFullFilled : Area
    {
        public override string GetUniqueLoadID() => $"{nameof(AreaFullFilled)}_{ID}";
        
        public override string Label => $"full_filled_{ID}";
        
        public override Color Color => Color.green;

        public override int ListPriority => 500;

        public override bool Mutable => true;

        public AreaFullFilled(AreaManager areaManager) : base(areaManager)
        {
            ID = -1;
            Invert();
        }
    }
}