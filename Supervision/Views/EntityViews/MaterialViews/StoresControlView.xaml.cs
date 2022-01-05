using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Supervision.Views.EntityViews.MaterialViews
{
    public partial class StoresControlView : Window
    {
        public StoresControlView()
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
