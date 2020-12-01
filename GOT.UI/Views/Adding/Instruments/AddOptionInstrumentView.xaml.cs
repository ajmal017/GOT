using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private DateTime? _selectedExpiryDate;
        private readonly Future _baseInstrument;
        private Option _selectedInstrument;

        private OptionTypes _selectedOptionType;

        public AddOptionInstrumentView(IConnector connector, Future baseInstrument)
        {
            InitializeComponent();

            _baseInstrument = baseInstrument;
            CodeBaseInstrumentTextBox.Text = baseInstrument.Code + " " + baseInstrument.Exchange;

            Options = new ObservableCollection<Option>(connector.GetOptions(baseInstrument)
                                                                .OrderBy(s => s.Strike));

            var dateTimes = Options.Select(s => s.ExpirationDate)
                                   .Distinct()
                                   .OrderBy(s => s.Date);

            foreach (var dateTime in dateTimes)
                ExpiryDates.Add(dateTime);

            OptionsCollection.Filter += ViewSource_Filter;
            SelectedExpiryDate = ExpiryDates.FirstOrDefault();

            SelectCommand = new DelegateCommand(OnSelect, CanSelect);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        public ObservableCollection<DateTime> ExpiryDates { get; set; } = new ObservableCollection<DateTime>();

        private ObservableCollection<Option> Options { get; }
        public ICollectionView OptionsCollection => CollectionViewSource.GetDefaultView(Options);

        public IEnumerable<OptionTypes> OptionTypeList => Enum.GetValues(typeof(OptionTypes)).Cast<OptionTypes>();

        public Option SelectedInstrument
        {
            get => _selectedInstrument;
            set
            {
                _selectedInstrument = value;
                _selectedInstrument.OptionType = SelectedOptionType;
                _selectedInstrument.Symbol = _baseInstrument.Code;
                _selectedInstrument.Currency = _baseInstrument.Currency;
                _selectedInstrument.Exchange = _baseInstrument.Exchange;
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

        public OptionTypes SelectedOptionType
        {
            get => _selectedOptionType;
            set
            {
                if (_selectedOptionType == value) {
                    return;
                }

                _selectedOptionType = value;
                if (SelectedInstrument != null) {
                    SelectedInstrument.OptionType = value;
                }

                NotifyPropertyChanged();
            }
        }

        private bool ViewSource_Filter(object sender)
        {
            if (sender is Option option) {
                return option.ExpirationDate == SelectedExpiryDate;
            }

            return false;
        }

        public DelegateCommand SelectCommand { get; private set; }

        private void OnSelect(object obj)
        {
            DialogResult = true;
        }

        private bool CanSelect(object obj)
        {
            return SelectedInstrument != null && SelectedOptionType != OptionTypes.None;
        }

        public DelegateCommand CancelCommand { get; private set; }

        private void OnCancel(object obj)
        {
            DialogResult = false;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (SelectedInstrument != null && SelectedOptionType != OptionTypes.None) {
                DialogResult = true;
            }
        }
    }
}