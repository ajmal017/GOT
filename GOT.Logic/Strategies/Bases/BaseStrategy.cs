using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using GOT.Logic.Connectors;
using GOT.Logic.DataTransferObjects;
using GOT.Logic.Enums;
using GOT.Logic.Models;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Utils.Helpers;
using GOT.Notification;
using GOT.SharedKernel;
using Newtonsoft.Json;

namespace GOT.Logic.Strategies.Bases
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BaseStrategy<T> : ViewModel where T : Instrument
    {
        private Directions _direction;

        private string _name;

        private ObservableCollection<Order> _orders = new ObservableCollection<Order>();

        private decimal _pnl;

        private StrategyStates _strategyState;

        private int _volume = 1;
        private Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;

        protected BaseStrategy(string name)
        {
            _name = name;
        }

        protected BaseStrategy()
        {
        }

        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonProperty("name")]
        public virtual string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("direction")]
        public virtual Directions Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("volume")]
        public virtual int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("orders")]
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            private set
            {
                _orders = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<Order> FilledOrders => Orders.Where(o => o.OrderState == OrderState.Filled).ToList();

        public StrategyStates StrategyState
        {
            get => _strategyState;
            set
            {
                _strategyState = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("instrument")]
        public virtual T Instrument { get; set; }

        [JsonProperty("position")]
        public virtual int Position => CalculatePositionByOrders();

        [JsonProperty("pnl")]
        public virtual decimal Pnl
        {
            get => _pnl;
            set
            {
                _pnl = value;
                OnPropertyChanged();
                OnPropertyChanged("PnlCurrency");
            }
        }

        /// <summary>
        ///     Pnl в валюте.
        /// </summary>
        /// <remarks>Рассчитывается по формуле Pnl</remarks>
        public virtual decimal PnlCurrency => Pnl * Instrument.Multiplier;

        public virtual IConnector Connector { get; set; }
        public virtual INotification[] Notifications { get; set; }
        public virtual IGotLogger Logger { get; set; }

        protected virtual void SendInfoNotification(string message = "")
        {
            var notify = Notifications.First(f => f.GetType() == typeof(TelegramNotification));
            notify.SendMessage(message);
            Logger.AddLog(message);
        }

        protected virtual void OnOrderChanged(Order ord)
        {
        }

        protected virtual void OnInstrumentChanged(InstrumentDTO inst)
        {
        }

        public virtual void Start()
        {
            StrategyState = StrategyStates.Started;
            Logger.AddLog($"Strategy: {Name} was started");
        }

        public virtual void Stop()
        {
            StrategyState = StrategyStates.Stopped;
            Logger.AddLog($"Strategy: {Name} was stopped");
        }

        /// <summary>
        ///     Обнуляет и сбрасывает информацию по стратегии
        /// </summary>
        public virtual bool Reset()
        {
            return true;
        }

        public virtual void ClosePositions()
        {
        }

        public virtual void SubscribeInstrument(Instrument instrument)
        {
            Connector?.SubscribeInstrument(instrument);
        }

        protected virtual void SendOrder(Instrument instrument, Directions direction, int volume,
            decimal price = decimal.Zero,
            string description = "")
        {
            Connector?.SendOrder(Id, instrument, direction, volume, price, description);
        }

        protected void CancelOrders()
        {
            Connector?.CancelAllOrders(Id);
            Logger.AddLog($"Cancel all orders of {Name} strategy");
        }

        protected void CancelOrder(int id)
        {
            Connector?.CancelOrder(id);
        }

        protected void SetPnl(decimal lastPrice)
        {
            if (Orders.Any()) {
                Pnl = PnlCalculator.CalculatePnlByOrders(FilledOrders, lastPrice);
            }
        }

        private int CalculatePositionByOrders()
        {
            var calcPosition = 0;
            if (!Orders.Any()) {
                return calcPosition;
            }

            var orders = Orders
                         .Where(o => o.OrderState == OrderState.Filled || o.OrderState == OrderState.Active)
                         .ToList();
            calcPosition += orders.Sum(order =>
            {
                if (order.Direction == Directions.Sell) {
                    return -order.FilledVolume;
                }

                return order.FilledVolume;
            });

            return calcPosition;
        }

        protected void ReplaceOrder(Order ord)
        {
            Dispatcher.Invoke(() =>
            {
                Orders.Remove(Orders.Last());
                Orders.Add(ord);
            });
        }

        protected void CheckOrderToContains(Order ord)
        {
            Dispatcher.Invoke(() =>
            {
                if (!Orders.Contains(ord)) {
                    Orders.Add(ord);
                } else {
                    Orders.Remove(Orders.Last());
                    Orders.Add(ord);
                }
            });
        }
    }
}