using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GOT.UI.Properties;
using GOT.UI.ViewModels.Holders;

namespace GOT.UI.Views.Hedges
{
    public partial class HedgeContainerView : Window
    {
        private readonly Button[] _buttonsView;

        public HedgeContainerView()
        {
            InitializeComponent();
            _buttonsView = new[] {MainLevelButton, FirstLevelButton, SecondLevelButton, ThirdLevelButton};
        }

        private void PreviewKeyDownSetter(object sender, KeyEventArgs e)
        {
            ((HedgeHolderViewModel) DataContext).OnKeyDown();
        }

        private void LevelOnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            SetButtonsBackground(button?.Name);
        }

        private void SetButtonsBackground(string viewName)
        {
            foreach (var button in _buttonsView)
                if (button.Name.Contains(viewName)) {
                    var bc = new BrushConverter();
                    button.Background = (Brush) bc.ConvertFrom("#0595c6");
                } else {
                    button.Background = Brushes.Transparent;
                }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            BindingOperations.ClearAllBindings(TitlesGrid);
            base.OnClosing(e);
        }
    }
}