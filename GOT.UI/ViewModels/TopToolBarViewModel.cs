using GOT.Logic;
using GOT.Logic.Enums;
using GOT.SharedKernel;
using GOT.SharedKernel.Enums;
using GOT.UI.Common;
using GOT.UI.Views;

namespace GOT.UI.ViewModels
{
    public class TopToolBarViewModel : ViewModel
    {
        private readonly IGotContext _context;
        private IConfiguration _configuration;
        private ConnectionStates _connectionState;
        private ConnectorTypes _connectorType;

        public TopToolBarViewModel(IGotContext context)
        {
            _configuration = context.Config;
            _connectorType = context.Config.ConnectorType;
            _context = context;
            context.NewConnectionStates += states => ConnectionState = states;

            SettingsCommand = new DelegateCommand(ShowSettings, CanShowSettings);
            ConnectCommand = new DelegateCommand(Connect, CanConnect);
        }

        public ConnectionStates ConnectionState
        {
            get => _connectionState;
            set
            {
                _connectionState = value;
                OnPropertyChanged();
            }
        }

        public ConnectorTypes ConnectorType
        {
            get => _connectorType;
            set
            {
                _connectorType = value;
                OnPropertyChanged();
            }
        }

        #region Commands

        public DelegateCommand ConnectCommand { get; set; }

        private void Connect(object obj)
        {
            if (_context.Connector.ConnectionState != ConnectionStates.Connected) {
                _context.Connect();
            } else {
                _context.Disconnect();
            }
        }

        private bool CanConnect(object obj)
        {
            return _context.Connector != null;
        }

        public DelegateCommand SettingsCommand { get; set; }

        private void ShowSettings(object obj)
        {
            var dlg = new ConfigurationWindow(_configuration);
            if (dlg.ShowDialog().Value) {
                _configuration = dlg.Configuration;
                ConnectorType = _configuration.ConnectorType;
                _configuration.OnConfigurationChange(_configuration);
            }
        }

        private bool CanShowSettings(object obj)
        {
            return _context.Connector.ConnectionState != ConnectionStates.Connected;
        }

        #endregion
    }
}