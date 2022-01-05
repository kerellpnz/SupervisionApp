using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace Supervision.Views.EntityViews.DetailViews.WeldGateValve
{
    public partial class WeldGateValveCaseEditView : Window
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

        //private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        //{
        //    //// Have to do this in the unusual case where the border of the cell gets selected.
        //    //// and causes a crash 'EditItem is not allowed'
        //    e.Cancel = true;
        //}

        public WeldGateValveCaseEditView()
        {
            InitializeComponent();
            Show();
            Closing += this.OnWindowClosing;
        }

        private CollectionView itemsViewOriginal;
        private void PIDBox_KeyUp(object sender, KeyEventArgs e)
        {
            itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(PIDBox.ItemsSource);
            itemsViewOriginal.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(PIDBox.Text))
                {
                    PIDBox.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((DataLayer.PID)o).Number != null)
                    {
                        if (((DataLayer.PID)o).Number.Contains(PIDBox.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            itemsViewOriginal.Refresh();
        }

        private CollectionView FlangesItems;
        private void FlangesBox_KeyUp(object sender, KeyEventArgs e)
        {
            FlangesItems = (CollectionView)CollectionViewSource.GetDefaultView(FlangesBox.ItemsSource);
            FlangesItems.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(FlangesBox.Text))
                {
                    FlangesBox.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((CoverFlange)o).FullName != null)
                    {
                        if (((CoverFlange)o).FullName.Contains(FlangesBox.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            FlangesItems.Refresh();
        }

        private CollectionView BottomsItems;
        private void BottomsBox_KeyUp(object sender, KeyEventArgs e)
        {
            BottomsItems = (CollectionView)CollectionViewSource.GetDefaultView(BottomsBox.ItemsSource);
            BottomsItems.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(BottomsBox.Text))
                {
                    BottomsBox.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((CaseBottom)o).FullName != null)
                    {
                        if (((CaseBottom)o).FullName.Contains(BottomsBox.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            BottomsItems.Refresh();
        }

        private CollectionView Sleeves008Items;
        private void Sleeves008Box_KeyUp(object sender, KeyEventArgs e)
        {
            Sleeves008Items = (CollectionView)CollectionViewSource.GetDefaultView(Sleeves008Box.ItemsSource);
            Sleeves008Items.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(Sleeves008Box.Text))
                {
                    Sleeves008Box.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((CoverSleeve008)o).FullName != null)
                    {
                        if (((CoverSleeve008)o).FullName.Contains(Sleeves008Box.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            Sleeves008Items.Refresh();
        }

        private CollectionView Rings043Items;
        private void Rings043Box_KeyUp(object sender, KeyEventArgs e)
        {
            Rings043Items = (CollectionView)CollectionViewSource.GetDefaultView(Rings043Box.ItemsSource);
            Rings043Items.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(Rings043Box.Text))
                {
                    Rings043Box.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((Ring043)o).FullName != null)
                    {
                        if (((Ring043)o).FullName.Contains(Rings043Box.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            Rings043Items.Refresh();
        }

        //private CollectionView Rings047Items;
        //private void Rings047Box_KeyUp(object sender, KeyEventArgs e)
        //{
        //    Rings047Items = (CollectionView)CollectionViewSource.GetDefaultView(Rings047Box.ItemsSource);
        //    Rings047Items.Filter = ((o) =>
        //    {
        //        if (String.IsNullOrEmpty(Rings047Box.Text))
        //        {
        //            Rings047Box.SelectedItem = null;
        //            return true;
        //        }
        //        else
        //        {
        //            if (((Ring047)o).FullName != null)
        //            {
        //                if (((Ring047)o).FullName.Contains(Rings047Box.Text)) return true;
        //                else return false;
        //            }
        //            else return false;
        //        }
        //    });
        //    Rings047Items.Refresh();
        //}

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
            if (itemsViewOriginal != null) itemsViewOriginal.Filter = null;
            if (FlangesItems != null) FlangesItems.Filter = null;
            if (BottomsItems != null) BottomsItems.Filter = null;
            if (Sleeves008Items != null) Sleeves008Items.Filter = null;
            if (Rings043Items != null) Rings043Items.Filter = null;
            //if (Rings047Items != null) Rings047Items.Filter = null;
        }
    }
}
