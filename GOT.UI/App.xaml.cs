using System;
using System.Windows;
using GOT.Logic;
using GOT.SharedKernel;
using GOT.UI.ViewModels;

namespace GOT.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IGotContext _context;
        private IGotLogger _logger;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            FolderBuilder.CreateAllDirectories();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            _logger = new GotLogger();
            _context = new GotContext(_logger);

            MainWindow = new MainWindow {DataContext = new ShellViewModel(_context)};
            MainWindow.Show();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = (Exception) args.ExceptionObject;
            _context.SendNotification(ex.Message + "\\" + ex.StackTrace, 4);
            _context.Shutdown();
        }
    }
}