using System.Windows.Controls;
using System.Windows.Input;
using GOT.UI.ViewModels.Option;

namespace GOT.UI.Views.Options
{
    public partial class OptionFirstLevelView : UserControl
    {
        public OptionFirstLevelView()
        {
            InitializeComponent();
        }

        private void PreviewKeyDownSetter(object sender, KeyEventArgs e)
        {
            ((OptionFirstLevelViewModel) DataContext).OnKeyDown();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((OptionFirstLevelViewModel) DataContext).OnDoubleClick();
        }
    }
}