using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GOT.Logic.Connectors;
using GOT.Logic.Connectors.Cqg;
using GOT.Logic.Connectors.InteractiveBrokers;
using GOT.Logic.Enums;
using GOT.Logic.Strategies;
using GOT.Logic.Utils;
using GOT.Notification;
using GOT.SharedKernel;
using GOT.SharedKernel.Enums;
using MoreLinq.Extensions;


namespace GOT.Logic
{
   public class GotContext : IGotContext
    {
        public GotContext(IGotLogger logger)
        {
            GotLogger = logger;
            MainStrategies = new ObservableCollection<MainStrategy>();
            Config = GetConfiguration();
            Config.ConfigurationChanged += OnConfigurationChanged;
            INotificationFactory factory = new NotificationFactory(Config);
            EmailNotification = factory.GetEmailNotification();
            TelegramNotification = factory.GetTelegramNotification();
            CreateConnector(Config, GotLogger);

            Connector.ConnectionStateChanged += OnConnectionStateChanged;
            Connector.GatewayStateChanged += OnGatewayStateChanged;

            LoadStrategies();
        }

        public IConfiguration Config { get; set; }
        public IConnector Connector { get; set; }
        public IGotLogger GotLogger { get; }
        public INotification EmailNotification { get; set; }
        public INotification TelegramNotification { get; set; }
        public ObservableCollection<MainStrategy> MainStrategies { get; set; }
        public ILoader Loader { get; } = new JsonFileManager();
        public event Action<string> NewStatusMessage = delegate { };
        public event Action<ConnectionStates> NewConnectionStates = delegate { };
        public event Action<ConnectionStates> NewGatewayStates = delegate { };

        /// <summary>
        /// Отправить уведомление.
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="type">тип сообщения. 1 - info, 2 - warning, 3 - error, 4 - fatal</param>
        public void SendNotification( string message = "", int type = 1)
        {
            TelegramNotification.SendMessage(message);
            EmailNotification.SendMessage(message);
            GotLogger.AddLog(message, type);
        }

        public void Shutdown()
        {
            if (Connector.ConnectionState == ConnectionStates.Connected) {
                Connector.Disconnect();
            }

            Connector.ConnectionStateChanged -= OnConnectionStateChanged;
            Save();
        }

        public void Save()
        {
            if (!MainStrategies.Any()) {
                return;
            }

            Loader.SaveStrategies(MainStrategies, Connector.ConnectorType.ToString());
        }

        public void AddStrategy(MainStrategy newStrategy)
        {
            MainStrategies.ForEach(s =>
            {
                if (s.Name.Equals(newStrategy.Name)) {
                    newStrategy.Name = $"{s.Name} duplicate";
                }
            });

            newStrategy.Id = Guid.NewGuid();
            newStrategy.Logger = GotLogger;
            newStrategy.Notifications = new []{EmailNotification, TelegramNotification};
            newStrategy.Connector = Connector;
            MainStrategies.Add(newStrategy);
        }

        public IEnumerable<string> GetAccountNames()
        {
            return Connector.GetAccounts().Select(s => s.Name);
        }

        public void Connect()
        {
            Connector.Connect();
            RequestAllInstruments();
            Connector.RequestMarketDataType(Config.DataType);
        }

        public void Disconnect()
        {
            Connector.Disconnect();
        }

        private void OnConfigurationChanged(IConfiguration configuration)
        {
            if (configuration.ConnectorType == ConnectorTypes.IB && Connector.ConnectorType == ConnectorTypes.IB) {
                ((IbConnector) Connector).UpdateConfig(configuration);
            } else if (configuration.ConnectorType != Connector.ConnectorType) {
                CreateConnector(configuration, GotLogger);
            }

            EmailNotification.UpdateServiceInfo(configuration);
            TelegramNotification.UpdateServiceInfo(configuration);
            Config = configuration;
        }

        private void LoadStrategies()
        {
            var strategies = Loader.LoadStrategies(Connector.ConnectorType.ToString());
            foreach (var s in strategies) {
                if (s == null) {
                    SendNotification("error loading strategy", 3);
                    return;
                }

                MainStrategies.Add(s);
            }
        }

        private void OnNewStatusMessage(string message)
        {
            NewStatusMessage?.Invoke(message);
        }

        private Configuration GetConfiguration()
        {
            var newConfig = Loader.Load<Configuration>(FolderBuilder.GetConfigurationPath());
            return newConfig ?? new Configuration();
        }

        private void CreateConnector(IConfiguration config, IGotLogger gotLogger)
        {
            try {
                Connector = config.ConnectorType switch
                {
                    ConnectorTypes.CQG => new CqgConnector(gotLogger),
                    ConnectorTypes.IB => new IbConnector(gotLogger, TelegramNotification, config),
                    _ => throw new Exception("error connector type")
                };

                FolderBuilder.CreateConnectorFolder(config.ConnectorType.ToString());
            }
            catch (Exception ex) {
                OnNewStatusMessage("Connection state is: " + ex.Message);
            }
        }

        private void OnConnectionStateChanged(ConnectionStates state)
        {
            switch (state) {
                case ConnectionStates.Connected:
                    MainStrategies.ForEach(s =>
                    {
                        if (s.Logger == null) {
                            s.Connector = Connector;
                            s.Logger = GotLogger;
                            s.Notifications = new []{EmailNotification, TelegramNotification};
                        }
                    });
                    break;
                case ConnectionStates.LossConnect:
                    MainStrategies.ForEach(s => s.Stop());
                    break;
                case ConnectionStates.Delay:
                    SendNotification("Данные идут с задержкой, перезагрузите клиент.");
                    break;
            }

            NewConnectionStates?.Invoke(state);
        }

        /// <summary>
        /// Запрашивает информацию по указанным инструментам у биржи.
        /// Необходимо для таких функций как добавление инструментов фьючерс, опцион, а также автоперезахода.
        /// </summary>
        private void RequestAllInstruments()
        {
            foreach (var mainStrategy in MainStrategies) {
                Connector.RequestInstrument(mainStrategy.Instrument.Code, InstrumentTypes.Futures);    
                Connector.RequestInstrument(mainStrategy.Instrument.Code, InstrumentTypes.Options);    
            }
        }

        private void OnGatewayStateChanged(ConnectionStates state)
        {
            GotLogger.AddLog($"Gateway state changed: {state}");
            OnNewStatusMessage("Gateway state is: " + state);
            NewGatewayStates?.Invoke(state);
        }
    }
}