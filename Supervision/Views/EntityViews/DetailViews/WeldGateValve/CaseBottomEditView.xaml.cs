using DataLayer.Entities.Materials;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace Supervision.Views.EntityViews.DetailViews.WeldGateValve
{
    public partial class CaseBottomEditView : Window
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

        public CaseBottomEditView()
        {
            InitializeComponent();
            Show();
            Closing += this.OnWindowClosing;
        }

        
        private CollectionView MetalsItems;
        

        private void MetalsBox_KeyUp(object sender, KeyEventArgs e)
        {
            MetalsItems = (CollectionView)CollectionViewSource.GetDefaultView(MetalsBox.ItemsSource);
            MetalsItems.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(MetalsBox.Text))
                {
                    MetalsBox.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((MetalMaterial)o).FullName != null)
                    {
                        if (((MetalMaterial)o).FullName.Contains(MetalsBox.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            MetalsItems.Refresh();
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
            
            if (MetalsItems != null) MetalsItems.Filter = null;
        }
    }
}
