using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
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
    public partial class WeldGateValveCoverEditView : Window
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

        public WeldGateValveCoverEditView()
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

        private CollectionView AssemblyUnitSealsItems;
        private void AssemblyUnitSealsBox_KeyUp(object sender, KeyEventArgs e)
        {
            AssemblyUnitSealsItems = (CollectionView)CollectionViewSource.GetDefaultView(AssemblyUnitSealsBox.ItemsSource);
            AssemblyUnitSealsItems.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(AssemblyUnitSealsBox.Text))
                {
                    AssemblyUnitSealsBox.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((AssemblyUnitSealing)o).FullName != null)
                    {
                        if (((AssemblyUnitSealing)o).FullName.Contains(AssemblyUnitSealsBox.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            AssemblyUnitSealsItems.Refresh();
        }

        private CollectionView SleevesItems;
        private void SleevesBox_KeyUp(object sender, KeyEventArgs e)
        {
            SleevesItems = (CollectionView)CollectionViewSource.GetDefaultView(SleevesBox.ItemsSource);
            SleevesItems.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(SleevesBox.Text))
                {
                    SleevesBox.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((CoverSleeve)o).FullName != null)
                    {
                        if (((CoverSleeve)o).FullName.Contains(SleevesBox.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            SleevesItems.Refresh();
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

        private CollectionView SpindlesItems;
        private void SpindlesBox_KeyUp(object sender, KeyEventArgs e)
        {
            SpindlesItems = (CollectionView)CollectionViewSource.GetDefaultView(SpindlesBox.ItemsSource);
            SpindlesItems.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(SpindlesBox.Text))
                {
                    SpindlesBox.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((Spindle)o).FullName != null)
                    {
                        if (((Spindle)o).FullName.Contains(SpindlesBox.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            SpindlesItems.Refresh();
        }

        private CollectionView ColumnsItems;
        private void ColumnsBox_KeyUp(object sender, KeyEventArgs e)
        {
            ColumnsItems = (CollectionView)CollectionViewSource.GetDefaultView(ColumnsBox.ItemsSource);
            ColumnsItems.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(ColumnsBox.Text))
                {
                    ColumnsBox.SelectedItem = null;
                    return true;
                }
                else
                {
                    if (((Column)o).FullName != null)
                    {
                        if (((Column)o).FullName.Contains(ColumnsBox.Text)) return true;
                        else return false;
                    }
                    else return false;
                }
            });
            ColumnsItems.Refresh();
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
            if (itemsViewOriginal != null) itemsViewOriginal.Filter = null;
            if (FlangesItems != null) FlangesItems.Filter = null;
            if (BottomsItems != null) BottomsItems.Filter = null;
            if (MetalsItems != null) MetalsItems.Filter = null;
            if (AssemblyUnitSealsItems != null) AssemblyUnitSealsItems.Filter = null;
            if (SleevesItems != null) SleevesItems.Filter = null;
            if (Sleeves008Items != null) Sleeves008Items.Filter = null;
            if (SpindlesItems != null) SpindlesItems.Filter = null;
            if (ColumnsItems != null) ColumnsItems.Filter = null;
        }
    }
}
