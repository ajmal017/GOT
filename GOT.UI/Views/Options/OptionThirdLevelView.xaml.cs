using System.Windows.Controls;
using System.Windows.Input;
using GOT.UI.ViewModels.Option;

namespace GOT.UI.Views.Options
{
    public partial class OptionThirdLevelView : UserControl
    {
        public OptionThirdLevelView()
        {
            InitializeComponent();
        }
        
        private void PreviewKeyDownSetter(object sender, KeyEventArgs e)
        {
            ((OptionThirdLevelViewModel) DataContext).OnKeyDown();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((OptionThirdLevelViewModel) DataContext).OnDoubleClick();
        }
    }
}