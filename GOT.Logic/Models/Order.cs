using System;
using GOT.Logic.Enums;
using GOT.Logic.Models.Instruments;
using GOT.SharedKernel;
using Newtonsoft.Json;

namespace GOT.Logic.Models
{
    public sealed class Order : Entity
    {
        public Order()
        {
        }

        public Order(int orderId)
        {
            Id = orderId;
        }

        [JsonProperty("strategyId")]
        public Guid StrategyId { get; set; }

        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("executionPrice")]
        public decimal ExecutionPrice { get; set; }

        [JsonProperty("orderType")]
        public OrderTypes OrderType { get; set; }

        [JsonProperty("orderState")]
        public OrderState OrderState { get; set; }

        [JsonProperty("volume")]
        public int Volume { get; set; }

        [JsonProperty("filledVolume")]
        public int FilledVolume { get; set; }

        [JsonProperty("direction")]
        public Directions Direction { get; set; }

        [JsonProperty("sendTime")]
        public DateTime SendTime { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}