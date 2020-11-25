using System.Windows.Controls;
using System.Windows.Input;
using GOT.UI.ViewModels.Option;

namespace GOT.UI.Views.Options
{
    public partial class OptionSecondLevelView : UserControl
    {
        public OptionSecondLevelView()
        {
            InitializeComponent();
        }

        private void PreviewKeyDownSetter(object sender, KeyEventArgs e)
        {
            ((OptionSecondLevelViewModel) DataContext).OnKeyDown();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((OptionSecondLevelViewModel) DataContext).OnDoubleClick();
        }
    }
}