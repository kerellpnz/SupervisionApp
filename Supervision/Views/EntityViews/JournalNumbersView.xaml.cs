using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;

namespace Supervision.Views.EntityViews
{
    /// <summary>
    /// Логика взаимодействия для JournalNumbers.xaml
    /// </summary>
    public partial class JournalNumbersView: Window
    {

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);


        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;

        const uint SC_CLOSE = 0xF060;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Disable close button
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
            }
        }

        public JournalNumbersView()
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
