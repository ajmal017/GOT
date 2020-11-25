using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GOT.Logic.Connectors;
using GOT.Logic.Enums;
using GOT.Logic.Models.Instruments;
using GOT.UI.Common;

namespace GOT.UI.Views.Adding.Instruments
{
    public partial class AddOptionInstrumentView : FrameWindow
    {
        private readonly Future _baseInstrument;
        private readonly IConnector _connector;

        private string _instrumentSearchPattern;
        private bool _isBusy;

        private DateTime? _selectedExpiryDate;

        private Option _selectedInstrument;

        private OptionTypes _selectedOptionType;

        public AddOptionInstrumentView(IConnector connector, Future baseInstrument)
        {
            InitializeComponent();
            _connector = connector;
            _baseInstrument = baseInstrument;
            CodeBaseInstrumentTextBox.Text = baseInstrument.Code + " " + baseInstrument.Exchange;

            OptionsCollection = CollectionViewSource.GetDefaultView(Options.OrderBy(s => s.Strike));
            OptionsCollection.Filter += ViewSource_Filter;

            LoadInstrumentsCommand = new DelegateCommand(OnLoadInstrument, CanLoadInstrument);
            SelectCommand = new DelegateCommand(OnSelect, CanSelect);
            CancelCommand = new DelegateCommand(OnCancel, CanCancel);
        }

        public ObservableCollection<DateTime> ExpiryDates { get; set; } = new ObservableCollection<DateTime>();

        public ObservableCollection<Option> Options { get; } = new ObservableCollection<Option>();

        public ICollectionView OptionsCollection { get; set; }

        public IEnumerable<OptionTypes> OptionTypes => Enum.GetValues(typeof(OptionTypes)).Cast<OptionTypes>();

        public Option SelectedInstrument
        {
            get => _selectedInstrument;
            set
            {
                _selectedInstrument = value;
                NotifyPropertyChanged();
            }
        }

        public DateTime? SelectedExpiryDate
        {
            get => _selectedExpiryDate;
            set
            {
                if (_selectedExpiryDate == value) {
                    return;
                }

                _selectedExpiryDate = value;
                NotifyPropertyChanged();
                OptionsCollection.Refresh();
            }
        }

        public string InstrumentSearchPattern
        {
            get => _instrumentSearchPattern;
            set
            {
                _instrumentSearchPattern = value;
                NotifyPropertyChanged();
                OptionsCollection.Refresh();
            }
        }

        public OptionTypes SelectedOptionType
        {
            get => _selectedOptionType;
            set
            {
                if (_selectedOptionType == value) {
                    return;
                }

                _selectedOptionType = value;
                NotifyPropertyChanged();
                OptionsCollection.Refresh();
            }
        }

        public DelegateCommand LoadInstrumentsCommand { get; private set; }

        public DelegateCommand SelectCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        private async void OnLoadInstrument(object obj)
        {
            try {
                Options.Clear();
                Spinner.Visibility = Visibility.Visible;
                ErrorTextBlock.Visibility = Visibility.Collapsed;
                _isBusy = true;

                await RequestInstruments();
                SetExpiryDates();
                SelectedExpiryDate = ExpiryDates.FirstOrDefault();
            }
            catch {
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            finally {
                _isBusy = false;
                Spinner.Visibility = Visibility.Collapsed;
                OptionsCollection.Refresh();
            }
        }

        private async Task RequestInstruments()
        {
            var options = await _connector.GetOptionsAsync(_baseInstrument.Code, _baseInstrument.Exchange);

            foreach (var option in options) {
                Options.Add(option);
            }
        }

        private bool CanLoadInstrument(object obj)
        {
            return !_isBusy;
        }

        private void SetExpiryDates()
        {
            var dateTimes = Options.Select(s => s.ExpirationDate)
                                   .Distinct().OrderBy(s => s.Date);
            foreach (var dateTime in dateTimes) ExpiryDates.Add(dateTime);
        }

        private bool ViewSource_Filter(object sender)
        {
            if (sender is Option option) {
                if (SelectedExpiryDate == null && string.IsNullOrEmpty(InstrumentSearchPattern)) {
                    return option.OptionType == SelectedOptionType;
                }

                if (string.IsNullOrEmpty(InstrumentSearchPattern)) {
                    return option.ExpirationDate == SelectedExpiryDate
                           && option.OptionType == SelectedOptionType;
                }

                if (InstrumentSearchPattern.Length > 2) {
                    return option.FullName.IndexOf(InstrumentSearchPattern, StringComparison.OrdinalIgnoreCase) != -1
                           && option.OptionType == SelectedOptionType;
                }
            }

            return false;
        }

        private void OnSelect(object obj)
        {
            DialogResult = true;
        }

        private bool CanSelect(object obj)
        {
            return SelectedInstrument != null;
        }

        private void OnCancel(object obj)
        {
            DialogResult = false;
        }

        private bool CanCancel(object obj)
        {
            return true;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (SelectedInstrument != null) {
                DialogResult = true;
            }
        }
    }
}