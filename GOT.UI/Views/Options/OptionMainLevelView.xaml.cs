using System.Windows.Controls;
using System.Windows.Input;
using GOT.UI.ViewModels.Option;

namespace GOT.UI.Views.Options
{
    public partial class OptionMainLevelView : UserControl
    {
        public OptionMainLevelView()
        {
            InitializeComponent();
        }

        private void PreviewKeyDownSetter(object sender, KeyEventArgs e)
        {
            ((OptionMainLevelViewModel) DataContext).OnKeyDown();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((OptionMainLevelViewModel) DataContext).OnDoubleClick();
        }
    }
}