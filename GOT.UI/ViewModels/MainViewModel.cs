using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GOT.Logic;
using GOT.Logic.Enums;
using GOT.Logic.Strategies;
using GOT.Logic.Strategies.Hedges;
using GOT.Logic.Strategies.Options;
using GOT.SharedKernel.Enums;
using GOT.UI.Common;
using GOT.UI.ViewModels.Holders;
using GOT.UI.Views;
using GOT.UI.Views.Adding.Strategy;
using GOT.UI.Views.Hedges;
using GOT.UI.Views.Options;
using GOT.UI.Views.SettingsViews;

namespace GOT.UI.ViewModels
{
    /// <summary>
    ///     Модель представления модуля стратегий
    /// </summary>
    public class MainViewModel : BaseContainerViewModel<MainStrategy>
    {
        private const string HEDGE_WINDOW = "HedgeWindow";
        private const string OPTION_WINDOW = "OptionWindow";
        private readonly Dictionary<Guid, Dictionary<string, Window>> _childWindows;
        private readonly IGotContext _context;

        public MainViewModel(IGotContext context)
        {
            _context = context;
            _childWindows = new Dictionary<Guid, Dictionary<string, Window>>();
            AddStrategyCommand = new DelegateCommand(AddStrategy, _ => _context.Connector != null);
            DeleteStrategyCommand = new DelegateCommand(DeleteStrategy, CanDeleteStrategy);
            StartStrategyCommand = new DelegateCommand(StartStrategy, CanStartStrategy);
            StartAllCommand = new DelegateCommand(StartAll, CanStartAll);
            StopStrategyCommand = new DelegateCommand(StopStrategy, CanStopStrategy);
            ClosePositionCommand = new DelegateCommand(ClosePosition, CanClosePosition);
            ResetStrategyCommand = new DelegateCommand(ResetStrategy, CanResetStrategy);
            ManualReenterCommand = new DelegateCommand(ManualReenter, CanManualReopen);
            OpenWindowCommand = new DelegateCommand<string>(OnOpenWindow, _ => SelectedStrategy != null);
            SaveStrategyCommand = new DelegateCommand(SaveStrategy, CanSaveStrategy);
            LoadStrategyCommand = new DelegateCommand(LoadStrategy, CanLoadStrategy);
            SaveLoadHedgeTemplateCommand = new DelegateCommand(SaveLoadHedgeTemplate, _ => SelectedStrategy != null);
            //контекстное меню
            GroupByInstrumentCommand = new DelegateCommand(OnGroupByInstrument);
            GroupByStateStrategyCommand = new DelegateCommand(OnGroupByStateStrategy);
            GroupByAccountCommand = new DelegateCommand(OnGroupByAccount);
            UngroupingCommand = new DelegateCommand(OnUngrouping);
            OpenInfoWindowCommand = new DelegateCommand(OpenMainStrategiesInfo, _ => _context.MainStrategies.Any());
            StrategiesCollection = CollectionViewSource.GetDefaultView(_context.MainStrategies);
            GroupCollection = StrategiesCollection.GroupDescriptions;
            FillChildWindows();
        }

        private void FillChildWindows()
        {
            foreach (var strategy in _context.MainStrategies) {
                CreateChildWindows(strategy);
            }
        }

        private void CreateChildWindows(MainStrategy strategy)
        {
            if (_childWindows.ContainsKey(strategy.Id)) {
                return;
            }

            var title = $"Стратегия {strategy.Name} Счет: {strategy.Account} Инструмент: {strategy.Instrument?.Code}";
            _childWindows.Add(strategy.Id, new Dictionary<string, Window>
            {
                {OPTION_WINDOW, CreateOptionWindow(title, strategy.Id, strategy.OptionHolder)},
                {HEDGE_WINDOW, CreateHedgeWindow(title, strategy.Id, strategy.HedgeHolder)}
            });
        }

        public override void OnDoubleClick()
        {
            EditStrategy();
        }

        private void EditStrategy()
        {
            var strategiesNames = _context.MainStrategies.Select(s => s.Name);
            var stBox = new SettingsMainView(_context, SelectedStrategy, strategiesNames);
            // ReSharper disable once PossibleInvalidOperationException
            if (stBox.ShowDialog().Value) {
                SelectedStrategy = stBox.EditStrategy;
            }
        }

        public override void OnKeyDown()
        {
            var command = KeyHandler.GetKeyStates();
            switch (command) {
                case GotKeyCommand.AddStrategy:
                    AddStrategyCommand.Execute(command);
                    break;
                case GotKeyCommand.SingleDelete:
                    if (SelectedStrategy != null) {
                        DeleteStrategyCommand.Execute(command);
                    }

                    break;
                case GotKeyCommand.SingleStart:
                    if (SelectedStrategy != null) {
                        StartStrategyCommand.Execute(command);
                    }

                    break;
                case GotKeyCommand.AllStart when _context.MainStrategies.Any() &&
                                                 _context.MainStrategies.All(s =>
                                                     s.StrategyState != StrategyStates.Started):
                    StartAllCommand.Execute(command);
                    break;
                case GotKeyCommand.SingleStop:
                    if (SelectedStrategy != null) {
                        StopStrategyCommand.Execute(command);
                    }

                    break;
                case GotKeyCommand.InfoStrategy:
                    OpenInfoWindowCommand.Execute(command);
                    break;
                case GotKeyCommand.OpenStopStrategyWindow:
                    if (SelectedStrategy != null) {
                        OpenWindowCommand.Execute(HEDGE_WINDOW);
                    }

                    break;
                case GotKeyCommand.OpenOptionWindow:
                    if (SelectedStrategy != null) {
                        OpenWindowCommand.Execute(OPTION_WINDOW);
                    }

                    break;
                case GotKeyCommand.Save:
                    _context.Save();
                    break;
            }
        }

        #region Commands

        /// <summary>
        ///     Добавляет стратегию
        /// </summary>
        public DelegateCommand AddStrategyCommand { get; }

        private void AddStrategy(object obj)
        {
            var dlg = new AddMainStrategyView(_context);
            if (dlg.ShowDialog().Value) {
                var strategy = dlg.Strategy;
                _context.MainStrategies.Add(strategy);
                CreateChildWindows(strategy);
                _context.Save();
            }
        }

        /// <summary>
        ///     Удаляет стратегию
        /// </summary>
        public DelegateCommand DeleteStrategyCommand { get; }

        private void DeleteStrategy(object obj)
        {
            try {
                if (MessageBox.Show(" Удалить стратегию? ", "",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                    var isNotUsingInstrument = !_context.MainStrategies.Any(
                        s => s.Instrument.Symbol.Equals(SelectedStrategy.Instrument.Symbol));
                    if (isNotUsingInstrument) {
                        _context.Connector.RemoveInstrument(SelectedStrategy.Instrument);
                    }

                    var connectorType = _context.Connector.ConnectorType.ToString();

                    var strategyForDelete = SelectedStrategy;
                    _context.Loader.SaveToDeleteFolder(connectorType, SelectedStrategy.Name);
                    _context.MainStrategies.Remove(strategyForDelete);
                    _childWindows.Remove(strategyForDelete.Id);
                }
            }
            catch {
                var errorMessage =
                    @"Произошла ошибка при переносе файла в папку 'Deleted strategies', сделайте перенос вручную.";
                MessageBox.Show(errorMessage, "Error!");
            }
        }

        private bool CanDeleteStrategy(object obj)
        {
            return SelectedStrategy != null && SelectedStrategy.StrategyState != StrategyStates.Started;
        }

        /// <summary>
        ///     Запускает стратегию
        /// </summary>
        public DelegateCommand StartStrategyCommand { get; }

        private void StartStrategy(object obj)
        {
            if (obj is GotKeyCommand) {
                if (!ShowMessageBox("Запустить выбранную стратегию?")) {
                    return;
                }
            }

            SelectedStrategy.Start();
            _context.Save();
        }

        private bool CanStartStrategy(object obj)
        {
            var isConnection = _context.Connector?.ConnectionState != ConnectionStates.Disconnected;
            var isStopStrategy = SelectedStrategy?.StrategyState == StrategyStates.Stopped;
            return _context.Connector != null &&
                SelectedStrategy != null &&
                isStopStrategy && isConnection;
        }

        /// <summary>
        ///     Запускает все стратегии
        /// </summary>
        public DelegateCommand StartAllCommand { get; }

        private void StartAll(object obj)
        {
            if (obj is GotKeyCommand) {
                if (MessageBox.Show("Запустить все стратегии?", "", MessageBoxButton.OKCancel,
                    MessageBoxImage.Question) != MessageBoxResult.OK) {
                    return;
                }
            }

            Parallel.ForEach(_context.MainStrategies, strategy => strategy.Start());
            _context.Save();
        }

        private bool CanStartAll(object obj)
        {
            var isConnection = _context.Connector?.ConnectionState != ConnectionStates.Disconnected;
            return _context.MainStrategies.Count > 0 && isConnection &&
                   _context.MainStrategies.All(s => s.StrategyState == StrategyStates.Stopped);
        }

        /// <summary>
        ///     Останавливает стратегию
        /// </summary>
        public DelegateCommand StopStrategyCommand { get; }

        private void StopStrategy(object obj)
        {
            if (obj is GotKeyCommand) {
                if (!ShowMessageBox("Остановить выбранную стратегию?")) {
                    return;
                }
            }

            SelectedStrategy.Stop();
            _context.Save();
        }

        private bool CanStopStrategy(object obj)
        {
            return SelectedStrategy != null && SelectedStrategy.StrategyState != StrategyStates.Stopped;
        }

        /// <summary>
        ///     Останавливает стратегию и закрывает все позиции
        /// </summary>
        public DelegateCommand ClosePositionCommand { get; }

        private void ClosePosition(object obj)
        {
            SelectedStrategy.SetClosingState(ClosingState.Manual);
        }

        private bool CanClosePosition(object obj)
        {
            var isConnection = SelectedStrategy?.Connector?.ConnectionState != ConnectionStates.Disconnected;
            return SelectedStrategy != null && isConnection;
        }

        public DelegateCommand ResetStrategyCommand { get; }

        private void ResetStrategy(object obj)
        {
            SelectedStrategy.Reset();
        }

        private bool CanResetStrategy(object obj)
        {
            return SelectedStrategy != null && SelectedStrategy.StrategyState == StrategyStates.Stopped;
        }

        public DelegateCommand ManualReenterCommand { get; }

        private void ManualReenter(object obj)
        {
            SelectedStrategy.SetClosingState(ClosingState.Reenter);
        }

        private bool CanManualReopen(object obj)
        {
            return SelectedStrategy != null && SelectedStrategy.ClosingState == ClosingState.None
                                            && SelectedStrategy.StrategyState == StrategyStates.Started;
        }

        /// <summary>
        ///     Открывает окно опционов или хеджей
        /// </summary>
        public DelegateCommand<string> OpenWindowCommand { get; }

        private Window CreateOptionWindow(string title, Guid id, OptionHolder holder)
        {
            var viewModel = new OptionsHolderViewModel(holder, () => OnOpenWindow(id, HEDGE_WINDOW));
            return new OptionContainerView {Title = title, DataContext = viewModel};
        }

        private Window CreateHedgeWindow(string title, Guid id, HedgeHolder holder)
        {
            var viewModel = new HedgeHolderViewModel(holder, () => OnOpenWindow(id, OPTION_WINDOW));
            return new HedgeContainerView {Title = title, DataContext = viewModel};
        }

        private void OnOpenWindow(string windowType)
        {
            OnOpenWindow(SelectedStrategy.Id, windowType);
        }

        private void OnOpenWindow(Guid id, string windowType)
        {
            if (!_childWindows.ContainsKey(id) || !_childWindows[id].ContainsKey(windowType)) {
                return;
            }

            var window = _childWindows[id][windowType];
            if (window.IsLoaded) {
                if (window.WindowState == WindowState.Minimized) {
                    window.WindowState = WindowState.Normal;
                }

                if (!window.IsActive && window.Visibility == Visibility.Visible) {
                    window.Activate();
                    return;
                }
            } else {
                window.Closing += WindowOnClosing;
            }

            window.Show();
        }

        private void WindowOnClosing(object sender, CancelEventArgs e)
        {
            ((Window) sender).Hide();
            e.Cancel = true;
        }

        public DelegateCommand OpenInfoWindowCommand { get; }

        private void OpenMainStrategiesInfo(object obj)
        {
            var wBox = new StrategyMessageBox(_context.MainStrategies);
            wBox.Show();
        }

        /// <summary>
        ///     Сохраняет все стратегии
        /// </summary>
        public DelegateCommand SaveStrategyCommand { get; }

        private void SaveStrategy(object obj)
        {
            _context.Loader.Save(SelectedStrategy, "Сохранение стратегии");
        }

        private bool CanSaveStrategy(object obj)
        {
            return _context.Connector != null &&
                   SelectedStrategy != null &&
                   SelectedStrategy.IsExistsHolders();
        }

        public DelegateCommand LoadStrategyCommand { get; }

        private void LoadStrategy(object obj)
        {
            var newStrategy = _context.Loader.LoadDialog<MainStrategy>("Загрузка стратегии");
            if (newStrategy == null) {
                return;
            }

            _context.AddStrategy(newStrategy);
            _context.Save();
            CreateChildWindows(newStrategy);
        }

        private bool CanLoadStrategy(object obj)
        {
            return _context.Connector != null;
        }

        /// <summary>
        ///     Сохраняет все стратегии
        /// </summary>
        public DelegateCommand SaveLoadHedgeTemplateCommand { get; }

        private void SaveLoadHedgeTemplate(object obj)
        {
            if (SelectedStrategy.HedgeHolder.ExistsStrategies()) {
                _context.Loader.Save(SelectedStrategy.HedgeHolder, "Сохранение сетки");
            } else {
                var oldHedgeWindow = _childWindows[SelectedStrategy.Id][HEDGE_WINDOW];
                var loadedHolder = _context.Loader.LoadDialog<HedgeHolder>("Загрузка сетки");

                if (loadedHolder == null) {
                    return;
                }

                SelectedStrategy.HedgeHolder = loadedHolder;
                _childWindows[SelectedStrategy.Id][HEDGE_WINDOW] = CreateHedgeWindow(oldHedgeWindow.Title,
                    SelectedStrategy.Id, SelectedStrategy.HedgeHolder);
            }

            _context.Save();
        }

        public ObservableCollection<GroupDescription> GroupCollection { get; set; }

        /// <summary>
        ///     Группирует элементы по включенным стратегиям
        /// </summary>
        public DelegateCommand GroupByStateStrategyCommand { get; }

        private void OnGroupByStateStrategy(object obj)
        {
            GroupCollection.Add(new PropertyGroupDescription("StrategyState"));
        }

        /// <summary>
        ///     Группирует элементы по инструментам
        /// </summary>
        public DelegateCommand GroupByInstrumentCommand { get; }

        private void OnGroupByInstrument(object obj)
        {
            GroupCollection.Add(new PropertyGroupDescription("Instrument.Code"));
        }

        /// <summary>
        ///     Группирует элементы по портфелю
        /// </summary>
        public DelegateCommand GroupByAccountCommand { get; }

        private void OnGroupByAccount(object obj)
        {
            GroupCollection.Add(new PropertyGroupDescription("Account"));
        }

        /// <summary>
        ///     Убирает группировку
        /// </summary>
        public DelegateCommand UngroupingCommand { get; }

        private void OnUngrouping(object obj)
        {
            GroupCollection.Clear();
        }

        #endregion
    }
}