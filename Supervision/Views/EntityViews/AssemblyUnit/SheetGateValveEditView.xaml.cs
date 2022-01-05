using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace Supervision.Views.EntityViews.AssemblyUnit
{
    public partial class SheetGateValveEditView : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);


        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;

        const uint SC_CLOSE = 0xF060;


        public SheetGateValveEditView()
        {            
            
            InitializeComponent();
            Show();
            Closing += this.OnWindowClosing;
        }

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

        private CollectionView PIDItems;
        private CollectionView CasesItems;
        private CollectionView SaddlesItems;
        private CollectionView CoversItems;
        private CollectionView GatesItems;
        private CollectionView ShearPinsItems;
        private CollectionView ScrewNutsItems;
        private CollectionView ScrewStudsItems;
        private CollectionView MainFlangeSealsItems;
        private CollectionView NozzlesItems;
        private CollectionView SpringsItems;
        private CollectionView BaseAnticorrosiveCoatingsItems;
        //private CollectionView DrawingsItems;

        //private void DrawingsBox_KeyUp(object sender, KeyEventArgs e)
        //{
        //    DrawingsItems = (CollectionView)CollectionViewSource.GetDefaultView(DrawingsBox.ItemsSource);
        //    DrawingsItems.Filter = ((o) =>
        //    {
        //        if (String.IsNullOrEmpty(DrawingsBox.Text)) return true;
        //        else
        //        {
        //            if (((string)o).Contains(DrawingsBox.Text)) return true;
        //            else return false;
        //        }
        //    });
        //    DrawingsItems.Refresh();
        //}

        private void PIDBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (PIDBox.ItemsSource != null)
            {
                PIDItems = (CollectionView)CollectionViewSource.GetDefaultView(PIDBox.ItemsSource);
                PIDItems.Filter = ((o) =>
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
                PIDItems.Refresh();
            }
        }

        private void CasesBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (CasesBox.ItemsSource != null)
            {
                CasesItems = (CollectionView)CollectionViewSource.GetDefaultView(CasesBox.ItemsSource);
                CasesItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(CasesBox.Text))
                    {
                        CasesBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((WeldGateValveCase)o).FullName != null)
                        {
                            if (((WeldGateValveCase)o).FullName.Contains(CasesBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                CasesItems.Refresh();
            }
        }

        private void SaddlesBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(SaddlesBox.ItemsSource != null)
            {
                SaddlesItems = (CollectionView)CollectionViewSource.GetDefaultView(SaddlesBox.ItemsSource);
                SaddlesItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(SaddlesBox.Text))
                    {
                        SaddlesBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((Saddle)o).FullName != null)
                        {
                            if (((Saddle)o).FullName.Contains(SaddlesBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                SaddlesItems.Refresh();
            }
        }

        private void CoversBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (CoversBox.ItemsSource != null)
            {
                CoversItems = (CollectionView)CollectionViewSource.GetDefaultView(CoversBox.ItemsSource);
                CoversItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(CoversBox.Text))
                    {
                        CoversBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((WeldGateValveCover)o).FullName != null)
                        {
                            if (((WeldGateValveCover)o).FullName.Contains(CoversBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                CoversItems.Refresh();
            }    
        }

        private void GatesBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (GatesBox.ItemsSource != null)
            {
                GatesItems = (CollectionView)CollectionViewSource.GetDefaultView(GatesBox.ItemsSource);
                GatesItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(GatesBox.Text))
                    {
                        GatesBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((Gate)o).FullName != null)
                        {
                            if (((Gate)o).FullName.Contains(GatesBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                GatesItems.Refresh();
            }  
        }

        private void ShearPinsBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (ShearPinsBox.ItemsSource != null)
            {
                ShearPinsItems = (CollectionView)CollectionViewSource.GetDefaultView(ShearPinsBox.ItemsSource);
                ShearPinsItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(ShearPinsBox.Text))
                    {
                        ShearPinsBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((ShearPin)o).FullName != null)
                        {
                            if (((ShearPin)o).FullName.Contains(ShearPinsBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                ShearPinsItems.Refresh();
            }  
        }

        private void ScrewNutsBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (ScrewNutsBox.ItemsSource != null)
            {
                ScrewNutsItems = (CollectionView)CollectionViewSource.GetDefaultView(ScrewNutsBox.ItemsSource);
                ScrewNutsItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(ScrewNutsBox.Text))
                    {
                        ScrewNutsBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((ScrewNut)o).FullName != null)
                        {
                            if (((ScrewNut)o).FullName.Contains(ScrewNutsBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                ScrewNutsItems.Refresh();
            }   
        }

        private void ScrewStudsBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (ScrewStudsBox.ItemsSource != null)
            {
                ScrewStudsItems = (CollectionView)CollectionViewSource.GetDefaultView(ScrewStudsBox.ItemsSource);
                ScrewStudsItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(ScrewStudsBox.Text))
                    {
                        ScrewStudsBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((ScrewStud)o).FullName != null)
                        {
                            if (((ScrewStud)o).FullName.Contains(ScrewStudsBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                ScrewStudsItems.Refresh();
            }   
        }

        private void SpringsBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (SpringsBox.ItemsSource != null)
            {
                SpringsItems = (CollectionView)CollectionViewSource.GetDefaultView(SpringsBox.ItemsSource);
                SpringsItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(SpringsBox.Text))
                    {
                        SpringsBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((Spring)o).FullName != null)
                        {
                            if (((Spring)o).FullName.Contains(SpringsBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                SpringsItems.Refresh();
            }  
        }

        private void NozzlesBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (NozzlesBox.ItemsSource != null)
            {
                NozzlesItems = (CollectionView)CollectionViewSource.GetDefaultView(NozzlesBox.ItemsSource);
                NozzlesItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(NozzlesBox.Text))
                    {
                        NozzlesBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((Nozzle)o).FullName != null)
                        {
                            if (((Nozzle)o).FullName.Contains(NozzlesBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                NozzlesItems.Refresh();
            }  
        }

        private void MainFlangeSealsBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (MainFlangeSealsBox.ItemsSource != null)
            {
                MainFlangeSealsItems = (CollectionView)CollectionViewSource.GetDefaultView(MainFlangeSealsBox.ItemsSource);
                MainFlangeSealsItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(MainFlangeSealsBox.Text))
                    {
                        MainFlangeSealsBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((MainFlangeSealing)o).FullName != null)
                        {
                            if (((MainFlangeSealing)o).FullName.Contains(MainFlangeSealsBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                MainFlangeSealsItems.Refresh();
            }  
        }

        private void BaseAnticorrosiveCoatingsBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (BaseAnticorrosiveCoatingsBox.ItemsSource != null)
            {
                BaseAnticorrosiveCoatingsItems = (CollectionView)CollectionViewSource.GetDefaultView(BaseAnticorrosiveCoatingsBox.ItemsSource);
                BaseAnticorrosiveCoatingsItems.Filter = ((o) =>
                {
                    if (String.IsNullOrEmpty(BaseAnticorrosiveCoatingsBox.Text))
                    {
                        BaseAnticorrosiveCoatingsBox.SelectedItem = null;
                        return true;
                    }
                    else
                    {
                        if (((BaseAnticorrosiveCoating)o).FullName != null)
                        {
                            if (((BaseAnticorrosiveCoating)o).FullName.Contains(BaseAnticorrosiveCoatingsBox.Text)) return true;
                            else return false;
                        }
                        else return false;
                    }
                });
                BaseAnticorrosiveCoatingsItems.Refresh();
            }  
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
            if (PIDItems != null) PIDItems.Filter = null;
            if (CasesItems != null) CasesItems.Filter = null;
            if (SaddlesItems != null) SaddlesItems.Filter = null;
            if (CoversItems != null) CoversItems.Filter = null;
            if (GatesItems != null) GatesItems.Filter = null;
            if (ShearPinsItems != null) ShearPinsItems.Filter = null;
            if (ScrewNutsItems != null) ScrewNutsItems.Filter = null;
            if (ScrewStudsItems != null) ScrewStudsItems.Filter = null;
            if (SpringsItems != null) SpringsItems.Filter = null;
            if (NozzlesItems != null) NozzlesItems.Filter = null;
            if (MainFlangeSealsItems != null) MainFlangeSealsItems.Filter = null;
            if (BaseAnticorrosiveCoatingsItems != null) BaseAnticorrosiveCoatingsItems.Filter = null;
            //if (DrawingsItems != null) DrawingsItems.Filter = null;
        }
    }
}
