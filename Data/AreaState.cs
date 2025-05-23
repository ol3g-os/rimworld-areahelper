using AreaHelper.Extended;
using Verse;

namespace AreaHelper.Data
{
    public class AreaState : IExposable
    {
        public AreaState()
        {
        }
        
        public AreaState(AreaExtended area)
        {
            AreaExtended = area;
        }

        public Area Area => AreaExtended.Area;
        
        public AreaExtended AreaExtended { get; set; }
        
        public bool Include { get => _include; set => _include = value; }

        private bool _include;
        
        public void ExposeData()
        {
            Scribe_Values.Look(ref _include, "include", true);
        }
    }
}