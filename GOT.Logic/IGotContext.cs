using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GOT.Logic.Connectors;
using GOT.Logic.Enums;
using GOT.Logic.Strategies;
using GOT.Logic.Utils;
using GOT.Notification;
using GOT.SharedKernel;

namespace GOT.Logic
{
    public interface IGotContext
    {
        IConnector Connector { get; }
        IGotLogger GotLogger { get; }
        public INotification EmailNotification { get; set; }
        public INotification TelegramNotification { get; set; }
        IConfiguration Config { get; }
        ILoader Loader { get; }
        ObservableCollection<MainStrategy> MainStrategies { get; set; }
        event Action<string> NewStatusMessage;
        event Action<ConnectionStates> NewConnectionStates;
        event Action<ConnectionStates> NewGatewayStates;
        void SendNotification(string message = "", int type = 1);
        void Shutdown();
        void Save();
        void AddStrategy(MainStrategy newStrategy);
        IEnumerable<string> GetAccountNames();
        void Connect();
        void Disconnect();
    }
}