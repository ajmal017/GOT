using System;
using System.Windows.Data;
using GOT.Logic.Enums;
using GOT.Logic.Strategies.Options;
using GOT.SharedKernel.Enums;
using GOT.UI.Common;
using GOT.UI.Views.Adding.Strategies;
using GOT.UI.Views.Adding.Strategy;
using GOT.UI.Views.SettingsViews;

namespace GOT.UI.ViewModels.Option
{
    public abstract class BaseOptionLevelViewModel : BaseContainerViewModel<OptionStrategy>
    {
        protected readonly OptionContainer Container;

        public BaseOptionLevelViewModel(OptionContainer container)
        {
            Container = container;
            StrategiesCollection = CollectionViewSource.GetDefaultView(Container.GetStrategies());
            AddStrategyCommand = new DelegateCommand(OnAddStrategy, CanAddStrategy);
            DeleteStrategyCommand = new DelegateCommand(OnDeleteStrategy, CanUseStoppedStrategy);
            StartStrategyCommand = new DelegateCommand(OnStartStrategy, CanUseStoppedStrategy);
            StopStrategyCommand = new DelegateCommand(OnStopStrategy, CanStopStrategy);
            ClosePositionCommand = new DelegateCommand(OnClosePosition, CanUseStoppedStrategy);
        }

        public override void OnKeyDown()
        {
            var command = KeyHandler.GetKeyStates();
            switch (command) {
                case GotKeyCommand.AddStrategy:
                    AddStrategyCommand.Execute(command);
                    break;
                case GotKeyCommand.SingleDelete:
                    DeleteStrategyCommand.Execute(command);
                    break;
                case GotKeyCommand.SingleStart when SelectedStrategy != null:
                    StartStrategyCommand.Execute(command);
                    break;
                case GotKeyCommand.SingleStop when SelectedStrategy != null:
                    StopStrategyCommand.Execute(command);
                    break;
            }
        }

        public override void OnDoubleClick()
        {
            var stBox = new SettingsOptionView(SelectedStrategy);
            // ReSharper disable once PossibleInvalidOperationException
            if (stBox.ShowDialog().Value) {
                SelectedStrategy = stBox.EditStrategy;
            }
        }

        #region Commands

        public DelegateCommand AddStrategyCommand { get; }

        private void OnAddStrategy(object obj)
        {
            try {
                var dlg = new AddOptionStrategyView(Container);
                if (dlg.ShowDialog().Value) {
                    var strategy = dlg.Strategy;
                    Container.AddStrategy(strategy);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private bool CanAddStrategy(object obj)
        {
            return Container.CanAddStrategy();
        }

        public DelegateCommand DeleteStrategyCommand { get; }

        private void OnDeleteStrategy(object obj)
        {
            if (ShowMessageBox("Удалить стратегию?")) {
                Container.Remove(SelectedStrategy);
            }
        }

        public DelegateCommand StartStrategyCommand { get; }

        private void OnStartStrategy(object obj)
        {
            if (obj is GotKeyCommand) {
                if (!ShowMessageBox("Запустить выбранную стратегию?")) {
                    return;
                }
            }

            SelectedStrategy.Start();
        }

        public DelegateCommand StopStrategyCommand { get; }

        private void OnStopStrategy(object obj)
        {
            if (obj is GotKeyCommand) {
                if (!ShowMessageBox("Остановить выбранную стратегию?")) {
                    return;
                }
            }

            SelectedStrategy.Stop();
        }

        private bool CanStopStrategy(object obj)
        {
            return SelectedStrategy != null && SelectedStrategy.StrategyState != StrategyStates.Stopped;
        }

        public DelegateCommand ClosePositionCommand { get; }

        private void OnClosePosition(object obj)
        {
            SelectedStrategy.ClosePositions();
        }

        private bool CanUseStoppedStrategy(object obj)
        {
            return SelectedStrategy != null && SelectedStrategy.StrategyState != StrategyStates.Started;
        }

        #endregion
    }
}