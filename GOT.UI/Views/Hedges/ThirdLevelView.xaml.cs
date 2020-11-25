using System.Windows.Controls;
using System.Windows.Input;
using GOT.UI.ViewModels.Hedge;

namespace GOT.UI.Views.Hedges
{
    public partial class ThirdLevelView : UserControl
    {
        public ThirdLevelView()
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
            ((ThirdLevelViewModel) DataContext).OnKeyDown();
        }
    }
}