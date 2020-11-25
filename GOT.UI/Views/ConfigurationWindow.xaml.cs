using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GOT.SharedKernel;
using GOT.SharedKernel.Enums;

namespace GOT.UI.Views
{
    public partial class ConfigurationWindow : FrameWindow
    {
        private const string SELECTED_BUTTON_BACKGROUND = "#0595c6";
        private const string REPOS_ADDRESS = "https://github.com/Polaroid15/BFF/releases";

        public ConfigurationWindow(IConfiguration configuration)
        {
            InitializeComponent();

            if (configuration != null) {
                Configuration = configuration;
                ConnectorTypeComboBox.SelectedValue = configuration.ConnectorType.ToString();
                DataTypeComboBox.SelectedValue = DataTypes[configuration.DataType - 1];
            } else {
                Configuration = new Configuration();
                ConnectorTypeComboBox.SelectedValue = ConnectorTypes.First();
                DataTypeComboBox.SelectedValue = DataType.Delayed;
            }
        }

        public List<string> ConnectorTypes => Enum.GetNames(typeof(ConnectorTypes)).ToList();
        public List<string> DataTypes => Enum.GetNames(typeof(DataType)).ToList();

        public string AssemblyVersion => $"Version: {Assembly.GetExecutingAssembly().GetName().Version}";

        public IConfiguration Configuration { get; }

        private void ShowConnectorLayout()
        {
            var bc = new BrushConverter();
            var brush = (Brush) bc.ConvertFrom(SELECTED_BUTTON_BACKGROUND);
            ConnectorButton.Background = brush;
            NotificationButton.Background = Brushes.Transparent;
            ConnectorCardGrid.Visibility = Visibility.Visible;
            NotificationCardGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowNotificationLayout()
        {
            var bc = new BrushConverter();
            var brush = (Brush) bc.ConvertFrom(SELECTED_BUTTON_BACKGROUND);
            NotificationButton.Background = brush;
            ConnectorButton.Background = Brushes.Transparent;
            NotificationCardGrid.Visibility = Visibility.Visible;
            ConnectorCardGrid.Visibility = Visibility.Collapsed;
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            try {
                var connectorType = ConnectorTypeComboBox.SelectedItem.ToString();
                var dataType = DataTypeComboBox.SelectedItem.ToString();
                Configuration.ConnectorType = (ConnectorTypes) Enum.Parse(typeof(ConnectorTypes), connectorType);
                Configuration.Email = EmailTextBox.Text;
                Configuration.IbHost = IBHostTextBox.Text;
                Configuration.IbPort = int.Parse(IBPortTextBox.Text);
                Configuration.DataType = (int) Enum.Parse(typeof(DataType), dataType);
                Configuration.IbClientId = int.Parse(IBIdTextBox.Text);
                Configuration.TelegramId = long.Parse(TelegramIdTextBox.Text);
                Configuration.TelegramProxy = TelegramProxyTextBox.Text;
                Configuration.TelegramHost = int.Parse(TelegramHostTextBox.Text);
                if (AutoConnectCheckBox.IsChecked != null) {
                    Configuration.IsAutoConnect = (bool) AutoConnectCheckBox.IsChecked;
                }

                DialogResult = true;
            }
            catch {
                MessageBox.Show("Указаны некорректные данные, попробуйте еще раз.");
                DialogResult = false;
            }
        }

        private void NavigateButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn) {
                if (btn.Name.Equals("NotificationButton")) {
                    ShowNotificationLayout();
                } else if (btn.Name.Equals("ConnectorButton")) {
                    ShowConnectorLayout();
                }
            }
        }

        private void OpenRepoOnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(REPOS_ADDRESS);
        }
    }

    internal enum DataType
    {
        Live = 1,
        Frozen = 2,
        Delayed = 3,
        DelayedFrozen = 4
    }
}