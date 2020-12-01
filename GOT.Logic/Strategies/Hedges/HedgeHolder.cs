using System.Collections.Generic;
using System.Linq;
using GOT.Logic.Connectors;
using GOT.Logic.Enums;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies.Bases;
using GOT.Notification;
using GOT.SharedKernel;
using Newtonsoft.Json;

namespace GOT.Logic.Strategies.Hedges
{
    public class HedgeHolder : IHolder
    {
        private readonly List<HedgeContainer> _containers = new List<HedgeContainer>();

        public HedgeHolder()
        {
            MainContainer = new HedgeContainer("Main");
            FirstContainer = new HedgeContainer("First");
            SecondContainer = new HedgeContainer("Second");
            ThirdContainer = new HedgeContainer("Third");
            _containers.AddRange(new[] {MainContainer, FirstContainer, SecondContainer, ThirdContainer});
        }

        public HedgeContainer MainContainer { get; }

        public HedgeContainer FirstContainer { get; }

        public HedgeContainer SecondContainer { get; }

        public HedgeContainer ThirdContainer { get; }

        [JsonIgnore]
        public int PositionContainers => _containers.Sum(cont => cont.Position);

        [JsonIgnore]
        public decimal PnlContainers => _containers.Sum(cont => cont.Pnl);

        public void StopAllContainers()
        {
            _containers.ForEach(cont => cont.StopContainer());
        }

        public void StartAllContainers()
        {
            _containers.ForEach(cont => cont.StartContainer());
        }

        public void CloseAllPositions()
        {
            _containers.ForEach(cont => cont.ClosePositions());
        }
        
        public void UpdateInstruments(decimal shiftStrikeStep)
        {
            _containers.ForEach(cont => cont.ShiftChildStrategies(shiftStrikeStep));
        }
        
        public bool ExistsStrategies()
        {
            return _containers.Any(cont => cont.IsNotEmpty());
        }

        public void SetAccount(string account)
        {
            _containers.ForEach(cont => cont.SetAccount(account));
        }

        public void SetConnector(IConnector connector)
        {
            _containers.ForEach(cont => cont.SetConnector(connector));
        }

        public void SetLogger(IGotLogger logger)
        {
            _containers.ForEach(cont => cont.SetLogger(logger));
        }

        public void SetNotification(INotification notification)
        {
            _containers.ForEach(cont => cont.SetNotification(notification));
        }

        public void SetParentName(string parentName)
        {
            _containers.ForEach(cont => cont.SetParentName(parentName));
        }

        public void SetParentInstrument(Future instrument)
        {
            _containers.ForEach(cont => cont.ParentInstrument = instrument);
        }

        public bool CheckToStopContainers()
        {
            return _containers.All(cont => cont.CheckStrategiesState(StrategyStates.Stopped));
        }

        public bool ResetAllContainers()
        {
            return _containers.All(cont => cont.ResetStrategies());
        }

        public int GetMainBuyVolumes()
        {
            return MainContainer.GetAllStrategiesVolumes(Directions.Buy);
        }

        public int GetMainSellVolumes()
        {
            return MainContainer.GetAllStrategiesVolumes(Directions.Sell);
        }
    }
}