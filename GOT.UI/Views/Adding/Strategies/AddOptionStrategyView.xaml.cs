using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GOT.Logic.Enums;
using GOT.Logic.Models.Instruments;
using GOT.Logic.Strategies.Options;
using GOT.UI.Common;
using GOT.UI.Views.Adding.Instruments;

namespace GOT.UI.Views.Adding.Strategies
{
    /// <summary>
    ///     Логика взаимодействия для NewStrategyWindow.xaml
    /// </summary>
    public partial class AddOptionStrategyView : FrameWindow
    {
        private readonly OptionContainer _container;
        private Option _currentInstrument;

        /// <summary>
        ///     Окно для создания новой стратегии
        /// </summary>
        public AddOptionStrategyView(OptionContainer container)
        {
            _container = container;
            InitializeComponent();
            if (!container.IsMain) {
                IsBasisGrid.Visibility = Visibility.Collapsed;
            }

            OpenInstrumentWindowCommand = new DelegateCommand(OnOpenInstrumentWindow, CanOpenInstrumentWindow);
            SaveCommand = new DelegateCommand(OnSave, CanSave);
        }

        public string ParentCode => _container.ParentInstrument.Code;
        public IEnumerable<string> DirectionsList => Enum.GetNames(typeof(Directions)).ToList();
        public IEnumerable<string> LifetimesList => Enum.GetNames(typeof(LifetimeOptions)).ToList();
        public OptionStrategy Strategy { get; private set; }

        public DelegateCommand OpenInstrumentWindowCommand { get; private set; }

        public DelegateCommand SaveCommand { get; private set; }

        private void OnOpenInstrumentWindow(object obj)
        {
            try {
                var instrument = new AddOptionInstrumentView(_container.Connector, _container.ParentInstrument);
                if (instrument.ShowDialog().Value) {
                    var selectedInstr = instrument.SelectedInstrument;
                    _currentInstrument = selectedInstr;
                    InstrumentTextBox.Text = _currentInstrument.TradingClass;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private bool CanOpenInstrumentWindow(object obj)
        {
            return _container.Connector != null;
        }

        private void OnSave(object obj)
        {
            try {
                Strategy = new OptionStrategy(_container.ParentStrategyName);
                Strategy.Connector = _container.Connector;
                Strategy.Logger = _container.Logger;
                Strategy.Notification = _container.GotNotification;
                Strategy.Account = _container.Account;
                var direction = DirectionComboBox.SelectedItem.ToString();
                Strategy.Direction = (Directions) Enum.Parse(typeof(Directions), direction);
                var lifetime = LifetimeComboBox.SelectedItem.ToString();
                Strategy.Lifetime = (LifetimeOptions) Enum.Parse(typeof(LifetimeOptions), lifetime);
                Strategy.PriceOffset = int.Parse(PriceOffsetTextBox.Text);
                Strategy.PriceStep = decimal.Parse(PriceStepTextBox.Text.Replace(".", ","));
                Strategy.Volume = int.Parse(VolumeTextBox.Text);
                Strategy.IsBasis = (bool) IsBasisCheckBox.IsChecked;
                Strategy.Instrument = _currentInstrument;
                Strategy.OptionType = Strategy.Instrument.OptionType;

                DialogResult = true;
            }
            catch (Exception) {
                MessageBox.Show("Указаны некорректные данные, попробуйте еще раз.", "Error!");
            }
        }

        private bool CanSave(object obj)
        {
            return _currentInstrument != null && !string.IsNullOrEmpty(PriceStepTextBox.Text);
        }

        protected override void PreviewTextInputHandler(object sender, TextCompositionEventArgs e)
        {
            var approvedDecimalPoint = false;
            PriceStepToolTip.Visibility = Visibility.Hidden;

            if (e.Text == ".") {
                if (!((TextBox) sender).Text.Contains(".")) {
                    approvedDecimalPoint = true;
                }
            }

            if (!(char.IsDigit(e.Text, e.Text.Length - 1) || approvedDecimalPoint)) {
                e.Handled = true;
            }
        }

        private void NameLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PriceStepTextBox.Text == "0") {
                PriceStepToolTip.Visibility = Visibility.Visible;
                PriceStepTextBox.Text = "";
            }
        }
    }
}