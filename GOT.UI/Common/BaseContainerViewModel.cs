using System.ComponentModel;
using System.Windows;
using GOT.SharedKernel;

namespace GOT.UI.Common
{
    public abstract class BaseContainerViewModel<TStrategy> : ViewModel
    {
        private Visibility _infoVisible = Visibility.Visible;

        private string _menuItemHeader = "Скрыть панель информации";

        private TStrategy _selectedStrategy;

        protected BaseContainerViewModel()
        {
            ShowInfoCommand = new DelegateCommand(OnShowInfo);
        }

        public ICollectionView StrategiesCollection { get; set; }

        public TStrategy SelectedStrategy
        {
            get => _selectedStrategy;
            set
            {
                _selectedStrategy = value;
                OnPropertyChanged();
            }
        }

        public string MenuItemHeader
        {
            get => _menuItemHeader;
            set
            {
                _menuItemHeader = value;
                OnPropertyChanged();
            }
        }

        public Visibility InfoVisible
        {
            get => _infoVisible;
            set
            {
                _infoVisible = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand ShowInfoCommand { get; set; }

        private void OnShowInfo(object obj)
        {
            if (InfoVisible == Visibility.Visible) {
                InfoVisible = Visibility.Collapsed;
                MenuItemHeader = "Показать панель информации";
            } else {
                InfoVisible = Visibility.Visible;
                MenuItemHeader = "Скрыть панель информации";
            }
        }

        public virtual void OnDoubleClick()
        {
        }

        public virtual void OnKeyDown()
        {
        }

        protected static bool ShowMessageBox(string header)
        {
            return MessageBox.Show(header, "", MessageBoxButton.OKCancel,
                MessageBoxImage.Question) == MessageBoxResult.OK;
        }
    }
}