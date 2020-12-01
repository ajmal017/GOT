using System.Collections.Generic;
using System.Linq;
using GOT.Logic.Connectors;
using GOT.Logic.Enums;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies.Bases;
using GOT.Notification;
using GOT.SharedKernel;
using Newtonsoft.Json;

namespace GOT.Logic.Strategies.Options
{
    public class OptionHolder : IHolder
    {
        private readonly List<OptionContainer> _containers;

        public OptionHolder()
        {
            MainContainer = new OptionContainer(true, "Main");
            FirstContainer = new OptionContainer("First");
            SecondContainer = new OptionContainer("Second");
            ThirdContainer = new OptionContainer("Third");
            _containers = new List<OptionContainer>
            {
                MainContainer, FirstContainer, SecondContainer, ThirdContainer
            };
        }

        public OptionContainer MainContainer { get; }

        public OptionContainer FirstContainer { get; }

        public OptionContainer SecondContainer { get; }

        public OptionContainer ThirdContainer { get; }

        [JsonIgnore]
        public int PositionContainers => _containers.Sum(s => s.Position);

        [JsonIgnore]
        public decimal PnlContainers => _containers.Sum(s => s.Pnl);

        public void StartAllContainers()
        {
            _containers.ForEach(container => container.StartContainer());
        }

        public void CloseAllPositions()
        {
            _containers.ForEach(cont => cont.ClosePositions());
        }

        public void StopAllContainers()
        {
            _containers.ForEach(container => container.StopContainer());
        }

        public bool ExistsStrategies()
        {
            return _containers.Any(cont => cont.IsNotEmpty());
        }

        public void SetParentName(string parentName)
        {
            _containers.ForEach(cont => cont.ParentStrategyName = parentName);
        }

        public void SetParentInstrument(Future instrument)
        {
            _containers.ForEach(cont => cont.ParentInstrument = instrument);
        }

        public void SetConnector(IConnector connector)
        {
            _containers.ForEach(cont => cont.SetConnector(connector));
        }
        
        public void SetAccount(string account)
        {
            _containers.ForEach(cont => cont.SetAccount(account));
        }

        public void SetLogger(IGotLogger logger)
        {
            _containers.ForEach(cont => cont.SetLogger(logger));
        }

        public void SetNotification(INotification notification)
        {
            _containers.ForEach(cont => cont.SetNotification(notification));
        }

        public void UpdateInstruments(decimal shiftStrikeStep)
        {
            _containers.ForEach(c => c.UpdateStrategy(shiftStrikeStep));
        }

        public void ClearContainers()
        {
            _containers.ForEach(container => container.Clear());
        }

        public IEnumerable<OptionStrategy> GetMainStrategies()
        {
            return MainContainer.GetStrategies();
        }

        public int GetVolumeMainPutOptions()
        {
            return MainContainer.GetSellVolumes(OptionTypes.Put);
        }

        public int GetVolumeMainCallOptions()
        {
            return MainContainer.GetSellVolumes(OptionTypes.Call);
        }

        public decimal GetSellPositionCost()
        {
            return _containers.Sum(s => s.GetSellPositionCost());
        }

        public decimal GetPositionCost()
        {
            return _containers.Sum(s => s.GetPositionCost());
        }

        /// <summary>
        ///     Проверяет текущую цену на автозакрытие
        /// </summary>
        /// <param name="currentPrice">текущая цена фьючерса</param>
        /// <param name="shift">коэффециент сдвига</param>
        /// <returns></returns>
        public bool CheckToAutoClose(decimal currentPrice, decimal shift)
        {
            return MainContainer.IsOptionPricesRangeShifted(currentPrice, shift);
        }

        /// <summary>
        ///     Рассчитать величину на которую сдвинулся страйк
        /// </summary>
        /// <param name="currentPrice">Текущая цена инструмента</param>
        /// <returns></returns>
        public decimal GetShiftStrikeStep(decimal currentPrice)
        {
            return MainContainer.GetShiftStrikeStep(currentPrice);
        }
    }
}