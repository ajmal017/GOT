using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GOT.Logic;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies;
using GOT.SharedKernel.Enums;
using GOT.UI.Common;
using GOT.UI.Views.Adding.Instruments;

namespace GOT.UI.Views.Adding.Strategy
{
    /// <summary>
    ///     Логика взаимодействия для NewStrategyWindow.xaml
    /// </summary>
    public partial class AddMainStrategyView : FrameWindow
    {
        private readonly IGotContext _context;
        private readonly IEnumerable<string> _strategiesNames;
        private Future _currentInstrument;

        private string _instrumentName;

        /// <summary>
        ///     Окно для создания новой стратегии
        /// </summary>
        /// <param name="context"></param>
        public AddMainStrategyView(IGotContext context)
        {
            InitializeComponent();
            _context = context;
            _strategiesNames = _context.MainStrategies.Select(s => s.Name);
            OpenInstrumentWindowCommand = new DelegateCommand(OnOpenInstrumentWindow, _ => _context != null);
            SaveCommand = new DelegateCommand(OnSave, CanSave);
        }

        public IEnumerable<string> Accounts => _context?.GetAccountNames();

        public string InstrumentName
        {
            get => _instrumentName;
            set
            {
                _instrumentName = value;
                NotifyPropertyChanged();
            }
        }

        public MainStrategy Strategy { get; private set; }

        public DelegateCommand OpenInstrumentWindowCommand { get; private set; }

        public DelegateCommand SaveCommand { get; private set; }

        private void OnOpenInstrumentWindow(object obj)
        {
            if (_context.Connector.ConnectorType == ConnectorTypes.IB) {
                var instrument = new AddInstrument(_context.Connector);
                if (instrument.ShowDialog().Value) {
                    _currentInstrument = instrument.SelectedInstrument;
                    InstrumentName = _currentInstrument.Code;
                }
            }
        }

        private void OnSave(object obj)
        {
            if (_context == null) {
                return;
            }

            var account = AccountsComboBox.SelectionBoxItem.ToString();
            var percentAutoClosing = decimal.Parse(PercentAutoClosingTextBox.Text.Replace(".", ","));
            var autoClosingShift = decimal.Parse(AutoClosingShiftTextBox.Text.Replace(".", ","));
            var name = StrategyNameTextBox.Text;
            var instrument = _currentInstrument;

            Strategy = new MainStrategy
            {
                Name = name,
                Account = account,
                Connector = _context.Connector,
                Notification = _context.TelegramNotification,
                Logger = _context.GotLogger,
                CreatingDate = DateTime.Now,
                Instrument = instrument,
                PercentAutoClosing = percentAutoClosing,
                AutoClosingShift = autoClosingShift
            };

            DialogResult = true;
        }

        private bool CanSave(object obj)
        {
            return _context != null &&
                   _currentInstrument != null &&
                   AccountsComboBox.SelectedItem != null &&
                   !string.IsNullOrEmpty(StrategyNameTextBox.Text);
        }

        private void NameOnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox) {
                var hasName = _strategiesNames.Any(s => s.Equals(textBox.Text));
                if (hasName) {
                    NameToolTip.Visibility = Visibility.Visible;
                    textBox.Text = "";
                }
            }
        }

        private void NameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox) {
                if (textBox.Text.Length > 0) {
                    NameToolTip.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}