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

namespace GOT.UI.Views.SettingsViews
{
    public partial class SettingsMainView : FrameWindow
    {
        private readonly IGotContext _context;
        private readonly string _oldStrategyName;
        private readonly IEnumerable<string> _strategiesNames;
        private Future _currentInstrument;


        private string _instrumentName;

        public SettingsMainView(IGotContext context, MainStrategy editStrategy, IEnumerable<string> strategiesNames)
        {
            InitializeComponent();
            _context = context;
            _oldStrategyName = editStrategy.Name;
            EditStrategy = editStrategy;
            InstrumentName = editStrategy.Instrument.Code;
            Title = editStrategy.Name;
            CurrentAccount.Text = "Портфель: " + editStrategy.Account;
            _strategiesNames = strategiesNames;
            OpenInstrumentWindowCommand = new DelegateCommand(OnOpenInstrumentWindow, _ => _context != null);
            SaveCommand = new DelegateCommand(OnSave, CanSave);
            ShowActivated = true;
        }

        public string InstrumentName
        {
            get => _instrumentName;
            set
            {
                _instrumentName = value;
                NotifyPropertyChanged();
            }
        }

        public MainStrategy EditStrategy { get; private set; }

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
            try {
                if (!StrategyName.Text.Equals(_oldStrategyName)) {
                    var connectorType = _context.Connector.ConnectorType.ToString();
                    _context.Loader.ReplaceStrategy(connectorType, _oldStrategyName, StrategyName.Text);
                    EditStrategy.Name = StrategyName.Text;
                }

                if (_currentInstrument != null) {
                    EditStrategy.Instrument = _currentInstrument;
                }

                EditStrategy.PercentAutoClosing = decimal.Parse(PercentAutoClosing.Text.Replace(".", ","));
                EditStrategy.AutoClosingShift = decimal.Parse(AutoClosingShiftTextBox.Text.Replace(".", ","));
                DialogResult = true;
            }
            catch (Exception) {
                MessageBox.Show("Указаны некорректные данные, попробуйте еще раз.", "Error!");
            }
        }

        private bool CanSave(object obj)
        {
            return !string.IsNullOrEmpty(StrategyName.Text);
        }

        private void NameLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox) {
                var hasName = _strategiesNames.Any(s => s.Equals(textBox.Text) && !s.Equals(_oldStrategyName));
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