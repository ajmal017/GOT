using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GOT.UI.Properties;
using GOT.UI.ViewModels;

namespace GOT.UI.Views.Main
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((MainViewModel) DataContext).OnDoubleClick();
        }

        private void PreviewKeyDownSetter(object sender, KeyEventArgs e)
        {
            ((MainViewModel) DataContext).OnKeyDown();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var mainColumnWeight = Settings.Default.MainColumnWeights;
            if (!mainColumnWeight.Any()) {
                return;
            }

            for (var i = 0; i < mainColumnWeight.Count; i++)
                GridItemsCollections.Columns[i].Width = mainColumnWeight[i];
        }

        private void OnHeaderSizeChange(object sender, SizeChangedEventArgs e)
        {
            if (!GridItemsCollections.IsLoaded) {
                return;
            }

            var columnsWeight = new List<double>();
            foreach (var column in GridItemsCollections.Columns) columnsWeight.Add(column.Width.Value);

            Settings.Default.MainColumnWeights = columnsWeight;
        }
    }
}