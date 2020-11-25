using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GOT.Logic.Enums;
using GOT.Logic.Strategies.Options;
using GOT.UI.Common;

namespace GOT.UI.Views.SettingsViews
{
    public partial class SettingsOptionView : FrameWindow
    {
        public SettingsOptionView(OptionStrategy editStrategy)
        {
            InitializeComponent();
            Title = editStrategy.Name;
            EditStrategy = editStrategy;
            DirectionsComboBox.SelectedValue = editStrategy.Direction.ToString();
            WorkingModesComboBox.SelectedValue = editStrategy.WorkingMode.ToString();
            LifeTimesComboBox.SelectedValue = editStrategy.Lifetime.ToString();
            ShowActivated = true;
            SaveCommand = new DelegateCommand(OnSave, CanSave);
        }

        public IEnumerable<string> DirectionsList => Enum.GetNames(typeof(Directions)).ToList();
        public IEnumerable<string> WorkingModesList => Enum.GetNames(typeof(WorkingMode)).ToList();
        public IEnumerable<string> LifetimesList => Enum.GetNames(typeof(LifetimeOptions)).ToList();

        public OptionStrategy EditStrategy { get; set; }

        public DelegateCommand SaveCommand { get; private set; }

        private void OnSave(object obj)
        {
            try {
                var direction = DirectionsComboBox.SelectedItem.ToString();
                EditStrategy.Direction = (Directions) Enum.Parse(typeof(Directions), direction);
                var workingMode = WorkingModesComboBox.SelectedItem.ToString();
                EditStrategy.WorkingMode = (WorkingMode) Enum.Parse(typeof(WorkingMode), workingMode);
                var lifetime = LifeTimesComboBox.SelectedItem.ToString();
                EditStrategy.Lifetime = (LifetimeOptions) Enum.Parse(typeof(LifetimeOptions), lifetime);
                EditStrategy.Volume = int.Parse(VolumeTextBox.Text);
                EditStrategy.PriceStep = decimal.Parse(PriceStepTextBox.Text.Replace(".", ","));
                EditStrategy.PriceOffset = int.Parse(PriceOffSetTextBox.Text);
                if (IsBasisCheckBox.IsChecked != null) {
                    EditStrategy.IsBasis = (bool) IsBasisCheckBox.IsChecked;
                }

                DialogResult = true;
            }
            catch (Exception) {
                MessageBox.Show("Указаны некорректные данные, попробуйте еще раз.", "Error!");
            }
        }

        private bool CanSave(object obj)
        {
            return !string.IsNullOrEmpty(PriceStepTextBox.Text);
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