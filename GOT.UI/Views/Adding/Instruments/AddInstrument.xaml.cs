using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GOT.Logic.Connectors;
using GOT.Logic.Models.Instruments;
using GOT.UI.Common;


namespace GOT.UI.Views.Adding.Instruments
{
    public partial class AddInstrument : FrameWindow
    {
        private readonly IConnector _connector;
        private string _codeSearchPattern;

        private string _instrumentSearchPattern;

        private Future _selectedInstrument;

        public AddInstrument(IConnector connector)
        {
            InitializeComponent();

            _connector = connector;

            var codes = _connector.GetInstrumentCodes();
            CodesCollection = CollectionViewSource.GetDefaultView(codes);
            CodesCollection.Filter += CodeFilter;

            Instruments = new ObservableCollection<Future>();
            InstrumentsCollection = CollectionViewSource.GetDefaultView(Instruments);
            InstrumentsCollection.Filter += InstrumentFilter;

            SelectCodeCommand = new DelegateCommand(OnSelectCode, CanSelectCode);
            SelectCommand = new DelegateCommand(OnSelect, CanSelect);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        public Future SelectedInstrument
        {
            get => _selectedInstrument;
            set
            {
                _selectedInstrument = value;
                NotifyPropertyChanged();
            }
        }

        public string CodeSearchPattern
        {
            get => _codeSearchPattern;
            set
            {
                _codeSearchPattern = value;
                NotifyPropertyChanged();
                CodesCollection.Refresh();
            }
        }

        public string InstrumentSearchPattern
        {
            get => _instrumentSearchPattern;
            set
            {
                _instrumentSearchPattern = value;
                NotifyPropertyChanged();
                InstrumentsCollection.Refresh();
            }
        }

        /// <summary>
        ///     Список кодов инструментов
        /// </summary>
        public ICollectionView CodesCollection { get; private set; }

        /// <summary>
        ///     Доступные фьючерс-инструменты
        /// </summary>
        private ObservableCollection<Future> Instruments { get; }

        /// <summary>
        ///     Коллекция инструментов, подготовленная для фильтрации <see cref="Instruments" />
        /// </summary>
        public ICollectionView InstrumentsCollection { get; private set; }

        public DelegateCommand SelectCodeCommand { get; private set; }

        public DelegateCommand SelectCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        private bool CodeFilter(object obj)
        {
            if (obj is string code) {
                return CodeSearchPattern == null ||
                       code.IndexOf(CodeSearchPattern, StringComparison.OrdinalIgnoreCase) != -1;
            }

            return false;
        }

        private bool InstrumentFilter(object obj)
        {
            if (obj is Instrument instrument) {
                return InstrumentSearchPattern == null
                       || instrument.Exchange.IndexOf(InstrumentSearchPattern, StringComparison.OrdinalIgnoreCase) != -1
                       || instrument.Code.IndexOf(InstrumentSearchPattern, StringComparison.OrdinalIgnoreCase) != -1
                       || instrument.FullName.IndexOf(InstrumentSearchPattern, StringComparison.OrdinalIgnoreCase) != -1
                       || instrument.Currency.IndexOf(InstrumentSearchPattern, StringComparison.OrdinalIgnoreCase) !=
                       -1;
            }

            return false;
        }

        private async void OnSelectCode(object obj)
        {
            try {
                Instruments.Clear();
                var code = CodeListView.SelectedItem as string;
                var futures = await _connector.GetFuturesAsync(code);
                foreach (var inst in futures) Instruments.Add(inst);
            }
            catch {
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            finally {
                InstrumentsCollection.Refresh();
            }
        }

        private bool CanSelectCode(object obj)
        {
            return CodeListView.SelectedItem != null;
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
            SelectedInstrument = null;
            DialogResult = false;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (SelectedInstrument != null) {
                DialogResult = true;
            }
        }

        private void OnCodeDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnSelectCode(sender);
        }
    }
}