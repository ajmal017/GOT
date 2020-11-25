using System.ComponentModel;
using System.Reflection;
using System.Windows;
using GOT.Logic;
using GOT.SharedKernel;
using GOT.UI.Properties;

namespace GOT.UI.ViewModels
{
    /// <summary>
    ///     Модель представления главного окна
    /// </summary>
    public class ShellViewModel : ViewModel
    {
        private readonly IGotContext _context;

        public ShellViewModel(IGotContext context)
        {
            _context = context;
            MainViewModel = new MainViewModel(_context);
            TopToolBar = new TopToolBarViewModel(_context);
            StatusPanel = new StatusPanelViewModel(_context);
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show(" Вы уверены что хотите выйти? ", " Выход ", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.No) {
                _context.SendNotification("Shutdown.");
                Settings.Default.Save();
                _context.Shutdown();
            } else {
                e.Cancel = true;
            }
        }

        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_context.Config.IsAutoConnect) {
                _context.Connect();
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var message = $"Version: [{version}]. All strategies loaded. Ready to start!";
            _context.SendNotification(message);
        }

        #region Properties

        private TopToolBarViewModel _topToolBar;

        public TopToolBarViewModel TopToolBar
        {
            get => _topToolBar;
            set
            {
                if (_topToolBar == value) {
                    return;
                }

                _topToolBar = value;
                OnPropertyChanged();
            }
        }

        private MainViewModel _mainViewModel;

        public MainViewModel MainViewModel
        {
            get => _mainViewModel;
            set
            {
                if (_mainViewModel == value) {
                    return;
                }

                _mainViewModel = value;
                OnPropertyChanged();
            }
        }

        private StatusPanelViewModel _statusPanel;

        public StatusPanelViewModel StatusPanel
        {
            get => _statusPanel;
            set
            {
                if (_statusPanel == value) {
                    return;
                }

                _statusPanel = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}