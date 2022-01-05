using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Supervision.Views.EntityViews.DetailViews.Valve
{
    /// <summary>
    /// Логика взаимодействия для ShutterReverse.xaml
    /// </summary>
    public partial class SaddleView : Window
    {
        public SaddleView()
        {
            InitializeComponent();
            Show();
            ((INotifyCollectionChanged)mainDataGrid.Items).CollectionChanged += AddedNewItem;
        }

        private void AddedNewItem(object sender, EventArgs e)
        {
            if (mainDataGrid.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(mainDataGrid, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }
    }
}
