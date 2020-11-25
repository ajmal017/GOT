using System.ComponentModel;
using System.Windows.Data;
using GOT.Logic.Enums;
using GOT.Logic.Strategies.Hedges;
using GOT.SharedKernel.Enums;
using GOT.UI.Common;
using GOT.UI.Views.SettingsViews;

namespace GOT.UI.ViewModels.Hedge
{
    public abstract class BaseHedgeLevelViewModel : BaseContainerViewModel<HedgeStrategy>
    {
        protected readonly HedgeContainer Container;

        protected BaseHedgeLevelViewModel(HedgeContainer container)
        {
            Container = container;

            AddSellStrategyCommand = new DelegateCommand(OnAddSellStrategy, _ => Container.Connector != null);
            AddBuyStrategyCommand = new DelegateCommand(OnAddBuyStrategy, _ => Container.Connector != null);
            DeleteStrategyCommand = new DelegateCommand(OnDeleteStrategy, CanDeleteStrategy);
            StartSingleStrategyCommand = new DelegateCommand(OnStartSingleStrategy, CanStartSingleStrategy);
            StopSingleStrategyCommand = new DelegateCommand(OnStopSingleStrategy, CanStopSingleStrategy);
            StartAllCommand = new DelegateCommand(OnStartAll, CanUseWhenAllStopped);
            StopAllCommand = new DelegateCommand(OnStopAll, CanStopAllStrategies);
            DeleteAllCommand = new DelegateCommand(OnDeleteAll, CanUseWhenAllStopped);

            //контекстное меню
            SetSellActivatePricesCommand = new DelegateCommand(OnSetSellActivatePrices, CanUseWhenAllStopped);
            SetBuyActivatePricesCommand = new DelegateCommand(OnSetBuyActivatePrices, CanUseWhenAllStopped);

            AllActivatePricesChangeCommand = new DelegateCommand(OnAllActivatePricesChange, CanUseWhenAllStopped);
            SellActivatePricesChangeCommand = new DelegateCommand(OnSellActivatePricesChange, CanUseWhenAllStopped);
            BuyActivatePricesChangeCommand = new DelegateCommand(OnBuyActivatePricesChange, CanUseWhenAllStopped);

            SetAntiOffsetCommand = new DelegateCommand(OnSetAntiOffset, CanUseWhenAllStopped);
            SetAntiBreakCommand = new DelegateCommand(OnSetAntiBreak, CanUseWhenAllStopped);
            SetRestartStopCommand = new DelegateCommand(OnSetRestartStop, CanUseWhenAllStopped);

            SetShiftStepPriceCommand = new DelegateCommand(OnSetShiftStepPrice, CanUseWhenAllStopped);
            VolumeChangedCommand = new DelegateCommand(OnVolumeChange, CanUseWhenAllStopped);
            AddVolumeCommand = new DelegateCommand(OnAddVolume, CanUseWhenAllStopped);

            StrategiesCollection = CollectionViewSource.GetDefaultView(Container.GetStrategies());
            UpdateLayout();
        }

        private void UpdateLevels()
        {
            StrategiesCollection.SortDescriptions.Clear();
            StrategiesCollection.SortDescriptions.Add(
                new SortDescription("ActivatePrice", ListSortDirection.Descending));
            Container.UpdateLevels();
            StrategiesCollection.Refresh();
        }

        public void UpdateLayout()
        {
            UpdateLevels();
        }

        public override void OnKeyDown()
        {
            var command = KeyHandler.GetKeyStates();
            switch (command) {
                case GotKeyCommand.AddBuyStrategy:
                    AddBuyStrategyCommand.Execute(command);
                    break;
                case GotKeyCommand.AddSellStrategy:
                    AddSellStrategyCommand.Execute(command);
                    break;
                case GotKeyCommand.SingleStart when SelectedStrategy != null:
                    StartSingleStrategyCommand.Execute(command);
                    break;
                case GotKeyCommand.SingleStop when SelectedStrategy != null:
                    StopSingleStrategyCommand.Execute(command);
                    break;
                case GotKeyCommand.AllStart when Container.IsNotEmpty():
                    StartAllCommand.Execute(command);
                    break;
                case GotKeyCommand.AllStop when Container.IsNotEmpty():
                    StopAllCommand.Execute(command);
                    break;
                case GotKeyCommand.AllDelete when Container.IsNotEmpty():
                    DeleteAllCommand.Execute(command);
                    break;
            }
        }

        #region ContextMenuCommands

        public DelegateCommand SetBuyActivatePricesCommand { get; }

        private void OnSetBuyActivatePrices(object obj)
        {
            var priceValue = new SettingsHedgeView(directions: Directions.Buy);
            // ReSharper disable once PossibleInvalidOperationException
            if (priceValue.ShowDialog().Value) {
                Container.ChangeBuyActivatePrices(priceValue.ValueForCorrection, priceValue.StepPrice);
                UpdateLayout();
            }
        }

        public DelegateCommand SetSellActivatePricesCommand { get; }

        private void OnSetSellActivatePrices(object obj)
        {
            var priceValue = new SettingsHedgeView(directions: Directions.Sell);
            // ReSharper disable once PossibleInvalidOperationException
            if (priceValue.ShowDialog().Value) {
                Container.ChangeSellActivatePrices(priceValue.ValueForCorrection, priceValue.StepPrice);
                UpdateLayout();
            }
        }

        public DelegateCommand AllActivatePricesChangeCommand { get; }

        private void OnAllActivatePricesChange(object obj)
        {
            var fixAntiOffsetDialog = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (fixAntiOffsetDialog.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.ActivatePriceAll,
                    fixAntiOffsetDialog.ValueForCorrection);
            }
        }

        public DelegateCommand SellActivatePricesChangeCommand { get; }

        private void OnSellActivatePricesChange(object obj)
        {
            var fixAntiOffsetDialog = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (fixAntiOffsetDialog.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.ActivatePriceSell,
                    fixAntiOffsetDialog.ValueForCorrection);
            }
        }

        public DelegateCommand BuyActivatePricesChangeCommand { get; }

        private void OnBuyActivatePricesChange(object obj)
        {
            var fixAntiOffsetDialog = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (fixAntiOffsetDialog.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.ActivatePriceBuy,
                    fixAntiOffsetDialog.ValueForCorrection);
            }
        }

        public DelegateCommand SetAntiOffsetCommand { get; }

        private void OnSetAntiOffset(object obj)
        {
            var fixAntiOffsetDialog = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (fixAntiOffsetDialog.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.AntiOffsetAntistop,
                    fixAntiOffsetDialog.ValueForCorrection);
            }
        }

        public DelegateCommand SetAntiBreakCommand { get; }

        private void OnSetAntiBreak(object obj)
        {
            var fixAntiOffsetDialog = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (fixAntiOffsetDialog.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.AntiBreakAntistop,
                    fixAntiOffsetDialog.ValueForCorrection);
            }
        }

        public DelegateCommand SetRestartStopCommand { get; }

        private void OnSetRestartStop(object obj)
        {
            var fixAntiOffsetDialog = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (fixAntiOffsetDialog.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.RestartStop,
                    fixAntiOffsetDialog.ValueForCorrection);
            }
        }

        public DelegateCommand SetShiftStepPriceCommand { get; }

        private void OnSetShiftStepPrice(object obj)
        {
            var shiftStepPrice = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (shiftStepPrice.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.ShiftStepPrice,
                    shiftStepPrice.ValueForCorrection);
            }
        }

        public DelegateCommand VolumeChangedCommand { get; }

        private void OnVolumeChange(object obj)
        {
            var fixAntiOffsetDialog = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (fixAntiOffsetDialog.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.VolumeHedge,
                    fixAntiOffsetDialog.ValueForCorrection);
            }
        }

        public DelegateCommand AddVolumeCommand { get; }

        private void OnAddVolume(object obj)
        {
            var fixAntiOffsetDialog = new SettingsHedgeView(false);
            // ReSharper disable once PossibleInvalidOperationException
            if (fixAntiOffsetDialog.ShowDialog().Value) {
                Container.StopStrategyFieldChange(HedgeStrategyFields.ChangeVolume,
                    fixAntiOffsetDialog.ValueForCorrection);
            }
        }

        #endregion

        #region ToolBarCommands

        public DelegateCommand AddSellStrategyCommand { get; }

        private void OnAddSellStrategy(object obj)
        {
            Container.AddStrategy(Directions.Sell);
            UpdateLevels();
        }

        public DelegateCommand AddBuyStrategyCommand { get; }

        private void OnAddBuyStrategy(object obj)
        {
            Container.AddStrategy(Directions.Buy);
            UpdateLevels();
        }

        public DelegateCommand DeleteStrategyCommand { get; }

        private void OnDeleteStrategy(object obj)
        {
            if (ShowMessageBox("Удалить стратегию?")) {
                Container.Remove(SelectedStrategy);
            }
        }

        private bool CanDeleteStrategy(object obj)
        {
            return SelectedStrategy != null && SelectedStrategy.StrategyState != StrategyStates.Started;
        }

        public DelegateCommand DeleteAllCommand { get; }

        private void OnDeleteAll(object obj)
        {
            if (ShowMessageBox("Вы уверены что хотите удалить все стратегии?")) {
                Container.Clear();
            }
        }

        public DelegateCommand StartSingleStrategyCommand { get; }

        private void OnStartSingleStrategy(object obj)
        {
            if (obj is GotKeyCommand) {
                if (!ShowMessageBox("Запустить выбранную стратегию?")) {
                    return;
                }
            }

            Container.StartSingleStrategy(SelectedStrategy);
        }

        private bool CanStartSingleStrategy(object obj)
        {
            
            return Container.Connector != null &&
                   SelectedStrategy != null && 
                   SelectedStrategy.StrategyState != StrategyStates.Started;
        }

        public DelegateCommand StartAllCommand { get; }

        private void OnStartAll(object obj)
        {
            if (obj is GotKeyCommand) {
                if (!ShowMessageBox("Запустить все стратегии?")) {
                    return;
                }
            }

            Container.StartContainer();
        }

        public DelegateCommand StopSingleStrategyCommand { get; }

        private void OnStopSingleStrategy(object obj)
        {
            if (obj is GotKeyCommand) {
                if (!ShowMessageBox("Остановить выбранную стратегию?")) {
                    return;
                }
            }

            Container.StopSingleStrategy(SelectedStrategy);
        }

        private bool CanStopSingleStrategy(object obj)
        {
            return Container.Connector != null &&
                   SelectedStrategy != null && 
                   SelectedStrategy.StrategyState != StrategyStates.Stopped;
        }

        public DelegateCommand StopAllCommand { get; }

        private void OnStopAll(object obj)
        {
            if (obj is GotKeyCommand) {
                if (!ShowMessageBox("Остановить все стратегии?")) {
                    return;
                }
            }

            Container.StopContainer();
        }

        private bool CanStopAllStrategies(object obj)
        {
            return Container.CheckStrategiesState(StrategyStates.Started);
        }

        private bool CanUseWhenAllStopped(object obj)
        {
            return Container.IsNotEmpty() && Container.CheckStrategiesState(StrategyStates.Stopped);
        }

        #endregion
    }
}