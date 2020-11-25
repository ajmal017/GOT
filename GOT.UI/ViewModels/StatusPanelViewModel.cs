using GOT.Logic;
using GOT.Logic.Enums;
using GOT.SharedKernel;

namespace GOT.UI.ViewModels
{
    public class StatusPanelViewModel : ViewModel
    {
        private ConnectionStates _gatewayState;

        private string _statusMessage;

        private ConnectionStates _сonnectionState;

        public StatusPanelViewModel(IGotContext context)
        {
            context.NewStatusMessage += message => StatusMessage = message;
            context.NewConnectionStates += states => ConnectionState = states;
            context.NewGatewayStates += states => GatewayState = states;
        }

        public ConnectionStates ConnectionState
        {
            get => _сonnectionState;
            set
            {
                _сonnectionState = value;
                OnPropertyChanged();
            }
        }

        public ConnectionStates GatewayState
        {
            get => _gatewayState;
            set
            {
                _gatewayState = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Сообщение в статусной строке
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }
}