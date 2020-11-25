using System.Windows.Controls;
using System.Windows.Input;
using GOT.UI.ViewModels.Hedge;

namespace GOT.UI.Views.Hedges
{
    public partial class FirstLevelView : UserControl
    {
        public FirstLevelView()
        {
            InitializeComponent();
            MouseLeave += (sender, args) =>
            {
                GridItems.CommitEdit();
                GridItems.CancelEdit();
            };
        }

        private void PreviewKeyDownSetter(object sender, KeyEventArgs e)
        {
            ((FirstLevelViewModel) DataContext).OnKeyDown();
        }
    }
}