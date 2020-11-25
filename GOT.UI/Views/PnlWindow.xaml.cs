namespace GOT.UI.Views
{
    public partial class PnlWindow
    {
        public PnlWindow(object item)
        {
            InitializeComponent();
            if (item != null) {
                DataContext = item;
            }
        }
    }
}