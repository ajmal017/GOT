using System.Windows;
using System.Windows.Controls;
using GOT.Logic.Enums;
using GOT.UI.Common;

namespace GOT.UI.Views.SettingsViews
{
    public partial class SettingsHedgeView : FrameWindow
    {
        private readonly bool _isActivatePrice;

        /// <summary>
        ///     Шаг, на который раздвигаются стратегии в сетке
        /// </summary>
        private decimal _stepPrice;

        /// <summary>
        ///     Новая цена активации.
        /// </summary>
        private decimal _valueForCorrection;

        public SettingsHedgeView(bool isActivatePriceWindow = true, Directions? directions = null)
        {
            InitializeComponent();
            _isActivatePrice = isActivatePriceWindow;
            if (_isActivatePrice) {
                Title = $"Цена активации. {directions.ToString()}";
            } else {
                Title = "Настройки сетки";
                LabelBasis.Text = "Параметр";
                StepGrid.Visibility = Visibility.Collapsed;
            }

            CorrectValueTextBox.Focus();

            FillCommand = new DelegateCommand(Fill, CanFill);
            CancelCommand = new DelegateCommand(Cancel, CanCancel);
        }

        public decimal ValueForCorrection
        {
            get => _valueForCorrection;
            set
            {
                if (_valueForCorrection == value) {
                    return;
                }

                _valueForCorrection = value;
                NotifyPropertyChanged();
            }
        }

        public decimal StepPrice
        {
            get => _stepPrice;
            set
            {
                if (_stepPrice == value) {
                    return;
                }

                _stepPrice = value;
                NotifyPropertyChanged();
            }
        }

        public DelegateCommand FillCommand { private set; get; }

        public DelegateCommand CancelCommand { private set; get; }

        private void Fill(object obj)
        {
            DialogResult = true;
        }

        private bool CanFill(object obj)
        {
            if (_isActivatePrice) {
                return ValueForCorrection != 0 && StepPrice != 0;
            }

            return ValueForCorrection != 0;
        }

        private void Cancel(object obj)
        {
            DialogResult = false;
        }

        private bool CanCancel(object obj)
        {
            return true;
        }

        private void CorrectValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(CorrectValueTextBox.Text.Replace(".", ","), out var value)) {
                ValueForCorrection = value;
            } else {
                ValueForCorrection = 0;
            }
        }

        private void StepValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(StepTextBox.Text.Replace(".", ","), out var value)) {
                StepPrice = value;
            } else {
                StepPrice = 0;
            }
        }
    }
}