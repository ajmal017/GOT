using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GOT.Logic.Connectors;
using GOT.Logic.Models.Instruments;
using GOT.Notification;
using GOT.SharedKernel;
using Newtonsoft.Json;

namespace GOT.Logic.Strategies.Bases
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BaseContainer<T> : ViewModel
    {
        protected BaseContainer(string name)
        {
            Name = name;
            Strategies = new ObservableCollection<T>();
        }

        [JsonProperty("parentName")]
        public string ParentStrategyName { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("parentInstrument")]
        public virtual Future ParentInstrument { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("strategies")]
        public ObservableCollection<T> Strategies { get; set; }

        public IConnector Connector { get; set; }

        public IGotLogger Logger { get; set; }

        public INotification GotNotification { get; set; }

        public virtual decimal Pnl { get; set; }

        public virtual int Position { get; set; }

        public virtual void StartContainer()
        {
        }

        public virtual void StopContainer()
        {
        }

        public virtual void ClosePositions()
        {
        }

        public virtual void AddStrategy(T strategy)
        {
            Strategies.Add(strategy);
        }

        public virtual IEnumerable<T> GetStrategies()
        {
            return Strategies;
        }

        public virtual bool IsNotEmpty()
        {
            return Strategies.Any();
        }

        public virtual void Remove(T strategy)
        {
            Strategies.Remove(strategy);
        }

        public virtual void Clear()
        {
            Strategies.Clear();
        }

        public virtual void SetConnector(IConnector connector)
        {
        }

        public virtual void SetAccount(string account)
        {
        }
        
        public virtual void SetNotification(INotification notification)
        {
        }

        public virtual void SetLogger(IGotLogger logger)
        {
        }

        public override string ToString()
        {
            return $"{ParentInstrument} {Name}";
        }
    }
}