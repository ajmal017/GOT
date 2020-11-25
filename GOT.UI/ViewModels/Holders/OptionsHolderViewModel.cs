using System;
using GOT.Logic.Strategies.Options;
using GOT.SharedKernel;
using GOT.SharedKernel.Enums;
using GOT.UI.Common;
using GOT.UI.ViewModels.Option;

namespace GOT.UI.ViewModels.Holders
{
    public class OptionsHolderViewModel : ViewModel
    {
        private readonly OptionFirstLevelViewModel _firstLevelViewModel;
        private readonly OptionMainLevelViewModel _mainLevelViewModel;

        private readonly Action _openHedgeWindow;
        private readonly OptionSecondLevelViewModel _secondLevelViewModel;
        private readonly OptionThirdLevelViewModel _thirdLevelViewModel;

        private BaseOptionLevelViewModel _currentViewModel;

        public OptionsHolderViewModel(OptionHolder holder, Action openHedgeWindow)
        {
            _openHedgeWindow = openHedgeWindow;
            _mainLevelViewModel = new OptionMainLevelViewModel(holder.MainContainer);
            _firstLevelViewModel = new OptionFirstLevelViewModel(holder.FirstContainer);
            _secondLevelViewModel = new OptionSecondLevelViewModel(holder.SecondContainer);
            _thirdLevelViewModel = new OptionThirdLevelViewModel(holder.ThirdContainer);

            OpenHedgeWindowCommand = new DelegateCommand(OnOpenHedgeWindow);
            NavigationCommand = new DelegateCommand<string>(ShowSelectedView);
            CurrentViewModel = _mainLevelViewModel;
        }

        public BaseOptionLevelViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand OpenHedgeWindowCommand { get; }

        public DelegateCommand<string> NavigationCommand { get; set; }

        private void OnOpenHedgeWindow(object obj)
        {
            _openHedgeWindow?.Invoke();
        }

        private void ShowSelectedView(string destinationView)
        {
            switch (destinationView) {
                case "mainView":
                    CurrentViewModel = _mainLevelViewModel;
                    break;
                case "firstView":
                    CurrentViewModel = _firstLevelViewModel;
                    break;
                case "secondView":
                    CurrentViewModel = _secondLevelViewModel;
                    break;
                case "thirdView":
                    CurrentViewModel = _thirdLevelViewModel;
                    break;
            }
        }

        public void OnKeyDown()
        {
            var command = KeyHandler.GetKeyStates();
            if (command == GotKeyCommand.OpenStopStrategyWindow) {
                OpenHedgeWindowCommand.Execute(command);
            }
        }
    }
}