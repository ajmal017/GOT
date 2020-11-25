using System.Collections.Generic;
using System.Windows.Input;
using GOT.Logic.Strategies;

namespace GOT.UI.Views
{

    public sealed partial class StrategyMessageBox
    {
        public IEnumerable<MainStrategy> Strategies { get; set; }

        public StrategyMessageBox(IList<MainStrategy> strategies)
        {
            Strategies = strategies;
            InitializeComponent();
        }

        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            var singleInfoWindow = new PnlWindow(ItemGrid.SelectedItem);
            singleInfoWindow.Show();
        }
    }
}