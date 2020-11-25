using System;
using GOT.Logic.Strategies.Hedges;
using GOT.SharedKernel;
using GOT.SharedKernel.Enums;
using GOT.UI.Common;
using GOT.UI.ViewModels.Hedge;

namespace GOT.UI.ViewModels.Holders
{
    public class HedgeHolderViewModel : ViewModel
    {
        private readonly FirstLevelViewModel _firstLevelViewModel;

        private readonly HedgeHolder _holder;
        private readonly MainLevelViewModel _mainLevelViewModel;

        private readonly Action _openOptionWindow;
        private readonly SecondLevelViewModel _secondLevelViewModel;
        private readonly ThirdLevelViewModel _thirdLevelViewModel;

        private BaseHedgeLevelViewModel _currentViewModel;

        public HedgeHolderViewModel(HedgeHolder holder, Action openOptionWindow)
        {
            _openOptionWindow = openOptionWindow;
            _holder = holder;

            _mainLevelViewModel = new MainLevelViewModel(_holder.MainContainer);
            _firstLevelViewModel = new FirstLevelViewModel(_holder.FirstContainer);
            _secondLevelViewModel = new SecondLevelViewModel(_holder.SecondContainer);
            _thirdLevelViewModel = new ThirdLevelViewModel(_holder.ThirdContainer);

            NavigationCommand = new DelegateCommand<string>(ShowSelectedView);
            OpenOptionWindowCommand = new DelegateCommand(OnOpenOptionWindow);
            ResetCommand = new DelegateCommand(Reset, OnReset);
            CurrentViewModel = _mainLevelViewModel;
        }

        public BaseHedgeLevelViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand<string> NavigationCommand { get; set; }
        public DelegateCommand ResetCommand { get; set; }
        public DelegateCommand OpenOptionWindowCommand { get; }

        private void OnOpenOptionWindow(object obj)
        {
            _openOptionWindow?.Invoke();
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

            CurrentViewModel.UpdateLayout();
        }

        private void Reset(object obj)
        {
            _holder.ResetAllContainers();
        }

        private bool OnReset(object obj)
        {
            return _holder.CheckToStopContainers();
        }

        public void OnKeyDown()
        {
            var command = KeyHandler.GetKeyStates();
            if (command == GotKeyCommand.OpenOptionWindow) {
                OpenOptionWindowCommand.Execute(command);
            }
        }
    }
}