using System;
using System.Linq;
using AreaHelper.Data;
using AreaHelper.Extended;
using UnityEngine;
using Verse;

namespace AreaHelper
{
    public class AreaCombined : Area, IDisposable
    {
        public override string GetUniqueLoadID() => $"{nameof(AreaCombined)}_{ID}";
        
        public override string Label => $"grouped_{ID}";
        
        public override Color Color => Color.green;
        
        public override bool Mutable => false;
        
        public override int ListPriority => 500;
        
        private AreaStates _areaStates;
        
        public AreaStates AreaStates => _areaStates;

        public MapExtended MapExtended { get; set; }

        private AreaExcluded AreaExcluded { get; set; }

        public bool IsDestroyed = false;

        public event EventHandler<AreaCombined> Destroyed;

        public AreaCombined()
        {
        }

        public AreaCombined(MapExtended mapExtended, AreaStates areaStates) : base(mapExtended.Map.areaManager)
        {
            MapExtended = mapExtended;
            AreaExcluded = new AreaExcluded(areaManager);
            _areaStates = new AreaStates(mapExtended);
            Subscribe(_areaStates);
            
            foreach (var keyValueArea in areaStates.States)
            {
                if (keyValueArea.Value.TryGetLayerState(AreaStateLayer.Combined, out var state))
                    _areaStates.Toggle(keyValueArea.Value.AreaExtended, state);
                    
                if (keyValueArea.Value.TryGetLayerState(AreaStateLayer.Default, out state))
                    _areaStates.Toggle(keyValueArea.Value.AreaExtended, state, AreaStateLayer.Default);
            }
            
            mapExtended.Map.areaManager.AllAreas.Add(this);
        }

        public void MarkForDrawAll(bool drawIncluded = true)
        {
            if (Prefs.LogVerbose)
                AreaHelper.LogMessage($"Mark for draw {_areaStates.Key} {drawIncluded}");
                
            if (drawIncluded)
                MarkForDraw();
            
            AreaExcluded.MarkForDraw();
        }

        public void AreaUpdateAll()
        {
            AreaUpdate();
            AreaExcluded.AreaUpdate();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Deep.Look(ref _areaStates, "areaStates");

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                areaManager = MapExtended.Map.areaManager;
                AreaStates.MapExtended = MapExtended;
                AreaExcluded = new AreaExcluded(areaManager);
                areaManager.AllAreas.Add(this);
            }
            else if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Subscribe(_areaStates);
            
                foreach (var areaState in _areaStates.States.Values)
                    Subscribe(areaState.AreaExtended);
                
                AreaExcluded.Calculate(_areaStates);
            }
        }

        public void Dispose()
        {
            Unsubscribe(_areaStates);
            
            foreach (var areaState in _areaStates.States.Values)
                Unsubscribe(areaState.AreaExtended);
            
            _areaStates.Dispose();
        }

        private void Destroy()
        {
            Dispose();
            MapExtended.Areas.Remove(this);
            Map.areaManager.AllAreas.Remove(this);
            IsDestroyed = true;
            Destroyed?.Invoke(this, this);
        }

        private void OnAreaStateChanged(object sender, AreaState areaState)
        {
            Calculate(areaState.Area, !areaState.Include);
        }

        private void OnAreaStateAdded(object sender, AreaState areaState)
        {
            Subscribe(areaState.AreaExtended);
        }

        private void OnAreaStateRemoved(object sender, AreaState areaState)
        {
            if (_areaStates.Key == null || 
                MapExtended.Areas.FindAll(x => x.AreaStates.Key == _areaStates.Key).Count > 1)
            {
                Destroy();
                return;
            }

            Unsubscribe(areaState.AreaExtended);
            Calculate(areaState.Area);
        }

        private void OnAreaStateAreaInvert(object sender, AreaExtended area)
        {
            Calculate();
        }

        private void OnAreaStateAreaChanged(object sender, AreaChangedArgs args)
        {
            Calculate(args.IntVec3, args.Value && !_areaStates.States[args.Area.ID].Include);
        }

        private void Calculate(Area area, bool exclude = false)
        {
            foreach (var cell in area.ActiveCells)
                Calculate(cell, exclude);
        }

        private void Calculate(IntVec3 cell, bool exclude = false)
        {
            if (Prefs.LogVerbose)
                AreaHelper.LogMessage($"Try calculate cell: exclude={exclude} x{cell.x} y{cell.y} z{cell.z}");
            
            if (exclude)
            {
                this[cell] = false;
                AreaExcluded.Calculate(cell, true);
                return;
            }

            var state = false;
            
            foreach (var areaState in _areaStates.States.Values)
            {
                if (areaState.Area[cell] && !areaState.Include)
                {
                    state = false;
                    break;
                }

                if (areaState.Area[cell] && areaState.Include)
                    state = true;
            }
            
            if (Prefs.LogVerbose)
                AreaHelper.LogMessage($"Calculate result ${state}");
            
            this[cell] = state;
            AreaExcluded.Calculate(cell, !state);
        }

        private void Calculate()
        {
            AreaExcluded.Calculate(_areaStates);
            
            var orderedAreas = _areaStates.States.OrderByDescending(x => x.Value.Include);
            
            foreach (var allCell in ActiveCells)
                this[allCell] = false;
            
            foreach (var areaState in orderedAreas)
                foreach (var cell in areaState.Value.Area.ActiveCells)
                    this[cell] = areaState.Value.Include;
        }

        private void Subscribe(AreaExtended area)
        {
            area.Changed += OnAreaStateAreaChanged;
            area.Invert += OnAreaStateAreaInvert;
        }

        private void Unsubscribe(AreaExtended area)
        {
            area.Changed -= OnAreaStateAreaChanged;
            area.Invert -= OnAreaStateAreaInvert;
        }

        private void Subscribe(AreaStates areaStates)
        {
            areaStates.Removed += OnAreaStateRemoved;
            areaStates.Added += OnAreaStateAdded;
            areaStates.Changed += OnAreaStateChanged;
        }

        private void Unsubscribe(AreaStates areaStates)
        {
            areaStates.Removed -= OnAreaStateRemoved;
            areaStates.Added -= OnAreaStateAdded;
            areaStates.Changed -= OnAreaStateChanged;
        }
    }
}