using System;
using AreaHelper.Data;
using Verse;

namespace AreaHelper.Extended
{
    public class AreaExtended : IExposable
    {
        public event EventHandler<AreaChangedArgs> Changed;
        
        public event EventHandler<AreaExtended> Removed;
        
        public event EventHandler<AreaExtended> Invert;

        public Area Area { get; set; }

        public AreaExtended() {}
        
        public AreaExtended(Area area)
        {
            Area = area;
        }

        public void FireChanged(IntVec3 c, bool val)
        {
            Changed?.Invoke(this, new AreaChangedArgs
            {
                Area = Area,
                IntVec3 = c,
                Value = val
            });
        }

        public void FireRemoved()
        {
            if (!Area.Mutable) return;
            Removed?.Invoke(this, this);
        }

        public void FireInvert()
        {
            Invert?.Invoke(this, this);
        }

        public void ExposeData()
        {
        }
    }
}