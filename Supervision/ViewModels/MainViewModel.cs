using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows;
using DevExpress.Mvvm;
using Supervision.Views.TCPViews;
using DataLayer;
using Supervision.Views.EntityViews.DetailViews;
using Supervision.Views.EntityViews;
using Supervision.ViewModels.EntityViewModels.DetailViewModels;

using DataLayer.TechnicalControlPlans.Detailing;

using Supervision.ViewModels.TCPViewModels;
using DataLayer.TechnicalControlPlans.AssemblyUnits;

using Supervision.Views.EntityViews.MaterialViews;
using Supervision.ViewModels.EntityViewModels.Materials;
using DataLayer.TechnicalControlPlans.Materials;
using Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve;
using Supervision.Views.EntityViews.DetailViews.Valve;
using Supervision.Views.EntityViews.DetailViews.WeldGateValve;
using Supervision.ViewModels.EntityViewModels.DetailViewModels.WeldGateValve;

using DataLayer.Entities.Detailing.SheetGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

using Supervision.Views.EntityViews.AssemblyUnit;
using Supervision.ViewModels.EntityViewModels.AssemblyUnit;
using DataLayer.TechnicalControlPlans;

using DataLayer.TechnicalControlPlans.Materials.AnticorrosiveCoating;
using Supervision.Views.EntityViews.MaterialViews.AnticorrosiveCoating;
using Supervision.ViewModels.EntityViewModels.Materials.AnticorrosiveCoating;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using DataLayer.Entities.Materials;
using DataLayer.Journals.Materials;

using Supervision.Views;
using DataLayer.TechnicalControlPlans.Periodical;
using Supervision.Views.EntityViews.PeriodicalControl;
using DataLayer.Entities.Periodical;
using DataLayer.Journals.Periodical;
using Supervision.ViewModels.EntityViewModels.Periodical;
using Supervision.ViewModels.EntityViewModels.Periodical.Gate;
using Supervision.Views.EntityViews.PeriodicalControl.Gate;
using BusinessLayer.Repository.Implementations.Entities;
using System.Collections.Generic;
using System;

namespace Supervision.ViewModels
{
    public class MainViewModel : BasePropertyChanged
    {
#if DEBUG
        public bool TCPKey = true;
        public bool InspKey = true;
        public bool ServiceKey = true;
#else
        public bool TCPKey = false;
        public bool InspKey = false;
        public bool ServiceKey = false;
#endif
        public static bool HelpMode = false;
        private static DataContext data = new DataContext();
        private readonly NDTPeriodicalRepository repoTO = new NDTPeriodicalRepository(data);
        private readonly WeldingPeriodicalRepository repoWeld = new WeldingPeriodicalRepository(data);
        private readonly StoreControlRepository repoStore = new StoreControlRepository(data);
        private readonly VersionControlRepository versionControl = new VersionControlRepository(data);

        public MainViewModel()
        {
            Page page = new MainMenu();
            page.DataContext = this;
            SlowOpacity(page);
            CheckDate();
        }

        private async void CheckDate()
        {
            try
            {                 
                IList<NDTControl> TO = await Task.Run(() => repoTO.GetAllAsync());
                IList <WeldingProcedures> Welding = await Task.Run(() => repoWeld.GetAllAsync());
                DateTime LastInspection = await Task.Run(() => repoStore.GetLastDateControl());
                IList<VersionControl> version = await Task.Run(() => versionControl.GetAllAsync());

                if (version[0].ControlValue != 25)
                {
                    MessageBoxResult result = MessageBox.Show("�� ����������� ���������� ������ ���������. ��� ����������� ������ ���������� ������� ����� ������ �� ����� ����", "��������!", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    if (result == MessageBoxResult.OK)
                    {
                        Application.Current.Shutdown();
                    }
                }
                else if (version[1].ControlValue == 1)
                {
                    MessageBox.Show(version[1].AdministratorMessage, "��������� �� ��������������", MessageBoxButton.OK, MessageBoxImage.Warning);
                }


                
                if (TO[0].NextControl != null)
                {
                    if (TO[0].NextControl < DateTime.Now)
                    {
                        MessageBox.Show("������ ���� ��������� ��", "��������!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                foreach (WeldingProcedures weldType in Welding)
                {
                    if (weldType.NextControl != null)
                    {
                        if (weldType.NextControl < DateTime.Now)
                        {
                            MessageBox.Show("���������� ��������� ������ ������", "��������!", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                    }
                }

                if (LastInspection != null)
                {
                    if (LastInspection.AddMonths(1) < DateTime.Now)
                    {
                        MessageBox.Show("������ ���� �������� �������������", "��������!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }                
            }
            catch
            {
                MessageBox.Show("���� ������ ����������. ��������� ����������� � ����.", "������ �������� ��", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Page currentPage;
        public Page CurrentPage
        { 
            get => currentPage;
            set 
            { 
                currentPage = value;
                RaisePropertyChanged();
            } 
        }

        private double frameOpacity;
        public double FrameOpacity
        { 
            get => frameOpacity;
            set 
            { 
                frameOpacity = value; 
                RaisePropertyChanged(); 
            } 
        }        

        public ICommand HelpModeActivator
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    MessageBoxResult result = MessageBox.Show("��� �������� ������ ������ ����� �������� ������� ������� ��������� ����� ��������� ���� � ������� ����������� � ����.", "������������ ����� ������?", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        HelpMode = true;
                        MessageBox.Show("1.������ \"������� �������� ����������\"\n\n" +
                            "\"���������� ��\" - ����, ���������� � ���� ���������� �� � ������� ����������. (��������, ��������� ������, ������ ���)\n\n" +
                            "\"���������� ��������� �������\" - ���� ���������, � ��.\n\n" +
                            "\"���������� ������\" - ����������, ����������� � ������ ������ � ���������� �� ������������� ������� (��������, O-Ring/�)\n\n" +
                            "\"����\", \"����/�����\" - ���� ��������������.\n\n" +
                            "\"�������\" - ���� ������.\n\n" +
                            "\"�������/�������\" - ���������� ���� ��������� ������� � �������, ����� ������� �������, ���� � ������� ���������, ��� �������� � ����������� ���� ������ ������� \"�������� ��������\"\n\n" +
                            "2.������ \"���\" - �������� � ���� ���� �������� �������� ����������, ����������� � �������� ��������� ���.\n\n" +
                            "3.������ \"������\" - ���� �������� �������� ��������� ���������� � ���� �������� ���\n\n" +
                            "4.������ \"������������� ��������\" - �������� ���� �������������� ��������, ������� ����� ��������� ������������ " +
                            "� ����������� ������ ������������ ���. ������� ������� ������, �� � �������������.\n\n" +
                            "5. ������ \"�������� ��������\" - �������� � ���� ���� ������� � ��������� ������ ��.\n\n" +
                            "����� ������� ������ � ����������, ���������� �������� ���� ��� � ������� \"��������\"\n\n" +
                            "��� ����� ��������� ������� �� ������� �� �������� ��������� � ���� �� ���.", "��������� \"SuperVision\"");
                    }
                    else
                    {
                        HelpMode = false;
                    }
                });
                
            }
        }

        public ICommand SpecificationOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var w = new SpecificationView();
                    w.DataContext = SpecificationVM.LoadSpecificationVM(new DataContext());
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("1.���� ������.\n" +
                            "� ������� ����� ���� ��������� ��� ���� ������: �� ������ ������������ � �� ������ PID. " +
                            "��������� ������ ������������ ����� ������������ � ������� ����. ����������� ������ �� PID ����� �������� �����, " +
                            "��������� � ����� ���� PID, ���� ����� ������������, �� ��� ������� ������ \"�������\", ��������� ������ \"�������������� PID\" �� ���� ���������� � ���� �����������.\n\n" +
                            "2.������������.\n" +
                            "���������� ������������ �������������� ����� ������� ������ \"�������� ������������\". ����� ������� ������ ��������� ������ ������. " +
                            "���� ������� �� �����, �������������� ��������� ����. ������ ������� ������� ������������ ����������� � ������������ � ��������/�������� �� ���������. " +
                            "������ \"������� ������������\" ������ ������������ � ��� ������������� �� PID�.\n\n" +
                            "3.PID�.\n" +
                            "��� ������� �� ������ ������������, � ������ ������� \"PID\" ����������� ��� ������������� ���� ������������ PID�. " +
                            "������ \"�������� PID\" ������� ������ \"�������������� PID\" � ������� ����� ������� PID � �������� ������������. " +
                            "������ \"������� PID\" ������ ��������� ������� PID � �������� ������������. " +
                            "��� ������� ������� ����� ������� ���� �� ������ PID ��������� ������ \"�������������� PID\" ��� ��������� ������.", "������ \"������������\"");
                    }
                });
            }
        }
        public ICommand JournalReportOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new JournalReportView
                    {
                        DataContext = JournalReportVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ������������ ������ ��� ���������� ������ ���� ������� ���� � ������� �������� �� ����������� ������.\n\n" +
                            "������ \"������������ �����\" ������� ���������� �� ������� ���� ���������� �� �����, �������� ������� �������� �� ���� ������ ������������� �� ����� \\�ranzit.\n\n" +
                            "������ \"������������ ����� (������� 2)\" ������� ���������� �� ������� ���� ���������� �� �����, �������� ������� �������� �� ���� ������, �������������� ������������� �� ������� ����.\n\n" +
                            "������ ������� ����� �������� ������� � ������ ���������� �������� ����. ������ ������� ��������������, ����� � ����� ������������ �������� ��������� ������������.\n\n" +
                            "������ \"������� Excel\" ��������� �������������� ����� � �������������� � ������ ����� Excel.", "������ \"���������� �����\"");
                    }
                });
            }
        }

        public ICommand DailyWorkReportOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new DailyWorkReportView
                    {
                        DataContext = DailyWorkReportVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ������������ ����������� ������ ���������� ������ ���� ������� ���� � ������� �������� �� ����������� ������.\n\n" +
                            "������ \"������������ �����\" ������� ���������� �� ������� ���� ���������� �� �����, �������� ������� �������� �� ���� ������ ������������� �� ����� \\�ranzit.\n\n" +
                            "������ \"������������ ����� (������� 2)\" ������� ���������� �� ������� ���� ���������� �� �����, �������� ������� �������� �� ���� ������, �������������� ������������� �� ������� ����.\n\n" +
                            "������ ������� ����� �������� ������� � ������ ���������� �������� ����. ������ ������� ��������������, ����� � ����� ������������ �������� ��������� ������������.\n\n" +
                            "������ \"������� Excel\" ��������� �������������� ����� � �������������� � ������ ����� Excel.", "������ \"���������� �����\"");
                    }
                });
            }
        }

        public ICommand CustomerOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var w = new CustomerView();
                    w.DataContext = CustomerVM.LoadCustomerVM(new DataContext());
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("� ���� ������� ��������� ��������� �� \"����\". ������ �������� ��������� �������������. " +
                            "��������� ��� �������������� ���� \"��������\" � ������� \"�������������� PID\"", "������ \"���������\"");
                    }
                });
            }
        }
        public ICommand InspectorOpen
        {
            get
            {
                //return new DelegateCommand(() =>
                //{
                //    var w = new InspectorView();
                //    w.DataContext = InspectorVM.LoadVM(new DataContext());
                //    if (MainViewModel.HelpMode == true)
                //    {
                //        MessageBox.Show("������ ���������� ������������� ������ ���������. ������� �� �������� ��������, ����� ������������ ���������� ������. " +
                //            "������ ������ ��������� � ���� \"�������\" ������ ��������.\n\n" +
                //            "������ \"��������\" ������� ������� ��� ���������� ������ � ������.", "������ \"��������\"");
                //    }
                //});

                
                        return new DelegateCommand(() =>
                        {
                            if (!InspKey)
                            {
                                PasswordWindow passwordWindow = new PasswordWindow(this);
                                passwordWindow.Show();
                            }
                            else
                            {
                                var w = new InspectorView();
                                w.DataContext = InspectorVM.LoadVM(new DataContext());
                                if (MainViewModel.HelpMode == true)
                                {
                                    MessageBox.Show("������ ���������� ������������� ������ ���������. ������� �� �������� ��������, ����� ������������ ���������� ������. " +
                                        "������ ������ ��������� � ���� \"�������\" ������ ��������.\n\n" +
                                        "������ \"��������\" ������� ������� ��� ���������� ������ � ������.", "������ \"��������\"");
                                }
                            }

                        });
            }
        }
        public ICommand ProductTypeOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var w = new ProductTypeView();
                    w.DataContext = ProductTypeViewModel.LoadVM(new DataContext());                    
                });
            }
        }
        public ICommand JournalNumbersOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var w = new JournalNumbersView();
                    w.DataContext = JournalNumbersViewModel.LoadVM(new DataContext());
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("������ ���������� ������ �������� ������������ �������. " +
                            "������ ������ ��������� � ���� \"������\" ������ ��������.\n\n" +
                            "������ \"��������\" ������� ������� ��� ���������� ������ � ������.\n" +
                            "��� ����, ����� �������� ������ �������� ������������ � �������� ��������, ���������� ��������� ������� � ���� \"������ ������\".", "������ \"������� ��\"");
                    }
                });
            }
        }

        public ICommand ServiceWindowOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (!ServiceKey)
                    {
                        PasswordWindow passwordWindow = new PasswordWindow(this);
                        passwordWindow.Show();
                    }
                    else
                    {
                        var w = new ServiceWindow();
                        w.DataContext = ServiceCommandsVM.LoadVM();
                    }
                });
            }
        }

        #region Materials
        public ICommand SheetMaterialOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new SheetMaterialView
                    {
                        DataContext = SheetMaterialVM.LoadSheetMaterialVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ ����� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������ ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� �����.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� �����, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"����\"");
                    }
                });
            }
        }
        public ICommand PipeMaterialOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new PipeMaterialView
                    {
                        DataContext = PipeMaterialVM.LoadPipeMaterialVM(new DataContext())
                    };
                });
            }
        }
        public ICommand ForgingMaterialOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new ForgingMaterialView
                    {
                        DataContext = ForgingMaterialVM.LoadVM(new DataContext())
                    };
                    MessageBox.Show("�� ��������� ���������� ��������, ��������� � ������������ ��������� �������� �������� ������� � ����, ������ ������ ������ � �������� ������ ��� ���������� ��� ��������� �������/�������.\n" +
                        "������� � ������� ������ �������� ��������������� � ����������� ������� ������ \"�������� ��������\": ������� ����� � \"������\", ������� ������ � \"������ �����������\", ������� ������� � \"�������\" � ��.\n" +
                        "����� �������� ������� �� ����� �������� (������ ��� ������) �������������� �� ������, � ���������� ��������� � ��� � ���������� ��.\n" +
                        "����� �������� ������� ����� ��� �����������/�������� �� �������������� ������ ������� �� ������, �����������, ����. ������, � ���������� ����������� ��.");
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� �������/������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��������/�������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������ \"����������\" ��������� ������ ��������� ���������� ����� ��������� �������/�������, ������������� ��������� ���������, ��������� ��� ���������� ������, ������� ����������� �������� ���, � ������� ����� � ���� � ���� ����� �����.", "������ \"�������/�������\"");
                    }
                });
            }
        }
        public ICommand RolledMaterialOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new RolledMaterialView
                    {
                        DataContext = RolledMaterialVM.LoadRolledMaterialVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ �����/����� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������/������ ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� �� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"����/�����\"");
                    }
                });
            }
        }

        public ICommand AbrasiveMaterialOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new BaseAnticorrosiveCoatingView
                    {
                        DataContext = AbrasiveMaterialVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ��� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� ���.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� ���, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"���\"");
                    }
                });
            }
        }
        public ICommand UndercoatOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new BaseAnticorrosiveCoatingView
                    {
                        DataContext = UndercoatVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ��� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� ���.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� ���, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"���\"");
                    }
                });
            }
        }
        public ICommand AbovegroundCoatingOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new BaseAnticorrosiveCoatingView
                    {
                        DataContext = AbovegroundCoatingVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ��� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� ���.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� ���, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"���\"");
                    }
                });
            }
        }
        public ICommand UndergroundCoatingOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new BaseAnticorrosiveCoatingView
                    {
                        DataContext = UndergroundCoatingVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ��� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� ���.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� ���, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"���\"");
                    }
                });
            }
        }
        public ICommand WeldingMaterialOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new WeldingMaterialView
                    {
                        DataContext = WeldingMaterialVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ��������� ���������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ �� ��������� ���������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� �� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"��������� ���������\"");
                    }
                });
            }
        }
        public ICommand ControlWeldOpen 
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new ControlWeldView
                    {
                        DataContext = ControlWeldVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ ��� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� ���.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� ���, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"���\"");
                    }
                });
            }
        }
        public ICommand StoresControlOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new StoresControlView
                    {
                        DataContext = StoresControlVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("������ ������ ������������ ����� ����� ������ �������� �������������.\n" +
                            "��� ���������� ����� ������ �������� ����� �� ����������� ������ " +
                        "\"����� ������\" � ������� ������ \"�������� ��������\". � ���� \"���� ��������� ��������\" ����������� ��������� ���� �������� �������������.", "������ \"�������� �������������\"");
                    }
                });
            }
        }
        public ICommand NDTControlOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new PeriodicalControlView
                    {
                        DataContext = NDTPeriodicalControlVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("� ���� ������� ����������� ���� ��������� ��������� �� ������������ �������. " +
                            "� ���������� �����/��� ������ ������ �� ����. �������� �������� �� ���������� ������, � ������� � ���, ����������� " +
                            "��������������� � ����� ������.\n\n" +
                            "��� �������� ��������� �������� ��� ���� �� ������ �������� ��", "������ \"�������� ��\"");
                    }
                });
            }
        }
        public ICommand WeldingProceduresOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new PeriodicalControlView
                    {
                        DataContext = WeldingPeriodicalControlVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("� ���� ������� ����������� ���� ��������� ��������� ������� ������ ������������ �������. " +
                            "� ���������� �����/��� ������ ������ �� ����. �������� �������� ������� ������ ���������� ��������� ������, � ������� � ���, ����������� " +
                            "��������������� � ����� ��������� ��������.\n\n" +
                            "��� �������� ��������� �������� ��� ���� �� ������ ������� ���� ������.", "������ \"������ ������\"");
                    }
                });
            }
        }
        public ICommand FactoryInspectionOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new FactoryInspectionView
                    {
                        DataContext = FactoryInspectionVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("������ ������ ������������ ����� ����� ������ �������� �������������� ������������ � �������� ������ ��������� ������.\n" +
                            "��� ���������� ����� ������ �������� ����� �� ����������� ������ " +
                        "\"����� ������\" � ������� ������ \"�������� ��������\".", "������ \"�������� �������������� ������������\"");
                    }
                });
            }
        }
        public ICommand DegreasingChemicalCompositionOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new GatePeriodicalView
                    {
                        DataContext = DegreasingChemicalCompositionVM.LoadVM(new DataContext())
                    };
                });
            }
        }
        public ICommand CoatingChemicalCompositionOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new GatePeriodicalView
                    {
                        DataContext = CoatingChemicalCompositionVM.LoadVM(new DataContext())
                    };
                });
            }
        }
        public ICommand CoatingPlasticityOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new GatePeriodicalView
                    {
                        DataContext = CoatingPlasticityVM.LoadVM(new DataContext())
                    };
                });
            }
        }
        public ICommand CoatingProtectivePropertiesOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new GatePeriodicalView
                    {
                        DataContext = CoatingProtectivePropertiesVM.LoadVM(new DataContext())
                    };
                });
            }
        }
        public ICommand CoatingPorosityOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new CoatingPorosityView
                    {
                        DataContext = CoatingPorosityVM.LoadVM(new DataContext())
                    };
                });
            }
        }
        public ICommand FrontalSaddleSealingOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new FrontalSaddleSealingView
                    {
                        DataContext = FrontalSaddleSealingVM.LoadFrontalSaddleSealingVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ���������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ����������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� ����������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"���������� ������\"");
                    }
                });
            }
        }
        public ICommand AssemblyUnitSealingOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new AssemblyUnitSealingView
                    {
                        DataContext = AssemblyUnitSealingVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ���������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ����������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� ����������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"���������� ��\"");
                    }                    
                });
            }
        }

        public ICommand MainFlangeSealingOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new MainFlangeSealingView
                    {
                        DataContext = MainFlangeSealingVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ���������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ����������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ���������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ���������� ����������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"���������� ��������� �������\"");
                    }
                });
            }
        }
#endregion

#region Assembly

        public ICommand SheetGateValveOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new SheetGateValveView
                    {
                        DataContext = SheetGateValveVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� �������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.", "������ \"��������\"");
                    }
                });
            }
        }
        
#endregion

#region Details
#region Sleeve
       
        
        public ICommand CoverSleeveOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new CoverSleeveView
                    {
                        DataContext = CoverSleeveVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������(016) ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � �������(016) ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"������(016)\"");
                    }
                });
            }
        }

        public ICommand CoverSleeve008Open
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new CoverSleeve008View
                    {
                        DataContext = CoverSleeve008VM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������(008) ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � �������(008) ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"������(008)\"");
                    }
                });
            }
        }

        public ICommand Ring047Open
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new Ring047View
                    {
                        DataContext = Ring047VM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ ������(047) ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � �������(047) ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"������(047)\"");
                    }
                });
            }
        }

        public ICommand Ring043Open
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new Ring043View
                    {
                        DataContext = Ring043VM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ ������(043) ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � �������(043) ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"������(043)\"");
                    }
                });
            }
        }

        public ICommand RunningSleeveOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new RunningSleeveView
                    {
                        DataContext = RunningSleeveVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������� ������ ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������� ������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"������ �������\"");
                    }
                });
            }
        }

#endregion
        public ICommand ScrewNutOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new ScrewNutView
                    {
                        DataContext = ScrewNutVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ���� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������� ���� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n", "������ \"�����\"");
                    }
                });
            }
        }
        
        public ICommand NozzleOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new NozzleView
                    {
                        DataContext = NozzleVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � �������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"�������\"");
                    }
                });
            }
        }

        

        
        public ICommand SheetGateValveCaseOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new WeldGateValveCaseView
                    {
                        DataContext = SheetGateValveCaseVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ �������/���������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��������/����������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ���������� ������ ��������� �������, ����������� �������� ���, �� ����������� ��������� �������, � ������� �� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n", "������ \"������\"");
                    }
                });
            }
        }
        
#region CaseDetail
        public ICommand CaseBottomOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new CaseBottomView
                    {
                        DataContext = CaseBottomVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ ����� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������ ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n", "������ \"�����\"");
                    }
                });
            }
        }
              
       
        
        
#endregion
#endregion
#region Cover
        
        public ICommand SheetGateValveCoverOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new WeldGateValveCoverView
                    {
                        DataContext = SheetGateValveCoverVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ���������� ������ ��������� �������, ����������� �������� ���, �� ����������� ��������� �������, � ������� �� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n", "������ \"������\"");
                    }
                });
            }
        }
        
#region CoverDetial
        public ICommand CoverFlangeOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new CoverFlangeView
                    {
                        DataContext = CoverFlangeVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ ������ ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n", "������ \"������\"");
                    }
                });
            }
        }
        
#endregion
#endregion
        
        public ICommand SpringOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new SpringView
                    {
                        DataContext = SpringVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ������ ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� ������ ������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.", "������ \"�������\"");
                    }
                });
            }
        }
        public ICommand SaddleOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var w = new SaddleView();
                    w.DataContext = SaddleVM.LoadSaddleVM(new DataContext());
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"������\"");
                    }
                });
            }
        }
        public ICommand CounterFlangeOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new CounterFlangeView
                    {
                        DataContext = CounterFlangeVM.LoadVM(new DataContext())
                    };
                });
            }
        }
        public ICommand GateOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new GateView
                    {
                        DataContext = GateVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ ������ ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"������\"");
                    }
                });
            }
        }
        public ICommand ScrewStudOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new ScrewStudView
                    {
                        DataContext = ScrewStudVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ������ ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������� ������ ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n", "������ \"�������\"");
                    }
                });
            }
        }
        public ICommand SpindleOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new SpindleView
                    {
                        DataContext = SpindleVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ������ �������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ��������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n" +
                        "������ \"���������� ��\" ��������� ������ ���������� �������, ������� ���������� �������� � �������� ��, " +
                        "��������� ��������� ������������� ������������ ��������� ������ � ��������� ��� ���������� ������ ��������� �������, " +
                        "������� ����������� �������� ���, � ������ ����� ������.", "������ \"��������\"");
                    }
                });
            }
        }

        public ICommand ColumnOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new ColumnView
                    {
                        DataContext = ColumnVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ �� ������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n", "������ \"������\"");
                    }
                });
            }
        }
        public ICommand ShearPinOpen
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _ = new ShearPinView
                    {
                        DataContext = ShearPinVM.LoadVM(new DataContext())
                    };
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("��� ���������� ����� ������ ������� ������� ������ \"��������\".\n\n" +
                        "������� ������ �� ������ � ������� ������� ��� ����� ������ + ������ \"��������\" - " +
                        "������� ������ �������������� ��������� �������.\n\n" +
                        "������� �� �������� �������� (\"�����\", \"������\" � �.�.) ����� ������������ ���������� �������.\n" +
                        "����� � ������ �������� ��������� ������ �������, � ������� ������� ����� ������������ ���������� ������� �� ��������� ����������.\n\n" +
                        "������ \"����������\" �������� ��������� ��� ���������� ������ ��������� �������, ������� ����������� �������� ���, � ������� ��� � ���� ����� ������ " +
                        "� ���� ��������� ��� ��������������.\n\n", "������ \"������\"");
                    }
                });
            }
        }
        

#region TCP
        private void LoadTCP<T>() where T : BaseTCP, new()
        {
            _ = new TCPView()
            {
                DataContext = TCPViewModel<T>.LoadVM<T>(new DataContext())
            };
        }

        private ICommand inputControlTCPPageOpen;
        public ICommand InputControlTCPPageOpen
        {
            get
            {
                return inputControlTCPPageOpen ??
                    (
                        inputControlTCPPageOpen = new DelegateCommand(() =>
                        {
                            Page page = new InputControlTCPPage();
                            page.DataContext = this;
                            SlowOpacity(page);
                        })
                    );
            }
        }

        private ICommand periodicalControlTCPPageOpen;
        public ICommand PeriodicalControlTCPPageOpen
        {
            get
            {
                return periodicalControlTCPPageOpen ??
                    (
                        periodicalControlTCPPageOpen = new DelegateCommand(() =>
                        {
                            Page page = new PeriodicalControlTCPPage();
                            page.DataContext = this;
                            SlowOpacity(page);
                        })
                    );
            }
        }

        private ICommand tcpPageOpen;
        public ICommand TCPPageOpen
        {
            get
            {
                return tcpPageOpen ??
                    (
                        tcpPageOpen = new DelegateCommand(() =>
                        {
                            if (!TCPKey)
                            {
                                PasswordWindow passwordWindow = new PasswordWindow(this);
                                passwordWindow.Show();                                
                            }
                            else
                            {
                                Page page = new TCPPage();
                                page.DataContext = this;
                                SlowOpacity(page);
                            }                                                 
                        })
                    );                
            }
        }

        public ICommand PIDTCPOpen
        {
            
            get
            {
                return new DelegateCommand(() => LoadTCP<PIDTCP>());
            }
        }
        public ICommand CoatingTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CoatingTCP>());
        }
        
        public ICommand SheetGateValveTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<SheetGateValveTCP>());
        }
        
        public ICommand GateTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<GateTCP>());
        }
#region Details
#region Sleeve
        
        public ICommand RunningSleeveTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<RunningSleeveTCP>());
        }
        public ICommand ColumnTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<ColumnTCP>());
        }
#endregion
        public ICommand ScrewNutTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<ScrewNutTCP>());
        }
        
        public ICommand NozzleTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<NozzleTCP>());
        }
#region Case
        
        public ICommand SheetGateValveCaseTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<SheetGateValveCaseTCP>());
        }
        
       
#region Details
        public ICommand CaseBottomTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CaseBottomTCP>());
        }
       
        
        
        
#endregion
#endregion
#region Cover
        
        public ICommand SheetGateValveCoverTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<SheetGateValveCoverTCP>());
        }
       
#region Details
        public ICommand CoverFlangeTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CoverFlangeTCP>());
        }
        
        public ICommand CoverSleeveTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CoverSleeveTCP>());
        }
        public ICommand Ring043TCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<Ring043TCP>());
        }
        public ICommand Ring047TCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<Ring047TCP>());
        }
        public ICommand CoverSleeve008TCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CoverSleeve008TCP>());
        }
#endregion
#endregion
       
        public ICommand SpringTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<SpringTCP>());
        }
        public ICommand SaddleTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<SaddleTCP>());
        }
        public ICommand CounterFlangeTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CounterFlangeTCP>());
        }
        public ICommand ScrewStudTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<ScrewStudTCP>());
        }
        public ICommand SpindleTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<SpindleTCP>());
        }
        public ICommand ShearPinTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<ShearPinTCP>());
        }
#endregion
#region Materials
        public ICommand AnticorrosiveCoatingTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<AnticorrosiveCoatingTCP>());
        }
        public ICommand MetalMaterialTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<MetalMaterialTCP>());
        }
        public ICommand ForgingMaterialTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<ForgingMaterialTCP>());
        }
        
        public ICommand WeldingMaterialTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<WeldingMaterialTCP>());
        }
        public ICommand FrontalSaddleSealingTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<FrontalSaddleSealingTCP>());
        }
        public ICommand AssemblyUnitSealingTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<AssemblyUnitSealingTCP>());
        }

        public ICommand MainFlangeSealingTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<MainFlangeSealingTCP>());
        }
#endregion
        public ICommand ControlWeldTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<ControlWeldTCP>());
        }
        public ICommand FactoryInspectionTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<FactoryInspectionTCP>());
        }
        public ICommand StoresControlTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<StoresControlTCP>());
        }
        public ICommand NDTTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<NDTControlTCP>());
        }
        public ICommand WeldingProceduresTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<WeldingProceduresTCP>());
        }
        public ICommand DegreasingChemicalCompositionTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<DegreasingChemicalCompositionTCP>());
        }
        public ICommand CoatingChemicalCompositionTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CoatingChemicalCompositionTCP>());
        }
        public ICommand CoatingPlasticityTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CoatingPlasticityTCP>());
        }
        public ICommand CoatingProtectivePropertiesTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CoatingProtectivePropertiesTCP>());
        }
        public ICommand CoatingPorosityTCPOpen
        {
            get => new DelegateCommand(() => LoadTCP<CoatingPorosityTCP>());
        }
#endregion

        public ICommand AppExit
        {
            get
            {
                return new DelegateCommand<CancelEventArgs>((args) => 
                    {
                        Application.Current.Shutdown();
                    });
            }
        }

        public async void SlowOpacity(Page page)
        {
            await Task.Factory.StartNew(() =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.25)
                {
                    FrameOpacity = i;
                    Thread.Sleep(50);
                }
                CurrentPage = page;
                for (double i = 0.0; i < 1.1; i += 0.25)
                {
                    FrameOpacity = i;
                    Thread.Sleep(50);
                }
            }).ConfigureAwait(false);
        }

        private void OpenWindow(Window w, BasePropertyChanged vm)
        {
            w.DataContext = vm;
            w.Show();
        }

        private ICommand mainMenuOpen;

        public ICommand MainMenuOpen
        {
            get
            {
                return mainMenuOpen ??
                (
                    mainMenuOpen = new DelegateCommand(() =>
                    {
                        Page page = new MainMenu();
                        page.DataContext = this;
                        SlowOpacity(page);
                    })
                );
            }
        }

        

        private ICommand sheetGateValveMenuOpen;
        public ICommand SheetGateValveMenuOpen
        {
            get
            {
                return sheetGateValveMenuOpen ??
                (
                    sheetGateValveMenuOpen = new DelegateCommand(() =>
                    {
                        Page page = new SheetGateValveMenu();
                        page.DataContext = this;
                        SlowOpacity(page);
                        if (MainViewModel.HelpMode == true)
                        {
                            MessageBox.Show("\"��������\" - ������ ���������� �������� ��������� ��� ������� � ������ ��������, � ����������� ��������� �������. " +
                                "� ������ ������� ����� ���������� ������ ��, �������� ��� ��, ��� ��, �������� ��.\n\n" +
                                "\"������\" - ������ ���������� �������� ��������/���������. � ������ ������ �������� ����� ���������� ����� �������� ��������, " +
                                "�������� �������, ��������������� �� �������. � ������� ������������ ������/������ ��������, �������� ���������, �������, ��������, ������������ � �.�.\n\n" +
                                "\"������\" - ������ ���������� �������� ��������� ��� ������� � ������ ������. " +
                                "� ������� ������������ ������/������ ������, �������� ���������, �������, ������������ � �.�.\n\n" +
                                "���� ��������� ������� �� (������, �����, �������� � �.�.) - ������� ���������� ������� ���������� ������� ��, �� ������ ���.", "������ \"�������� ��������\"");
                        }
                    })
                );
            }
        }

        private ICommand ballValveMenuOpen;
        public ICommand BallValveMenuOpen
        {
            get
            {
                return ballValveMenuOpen ??
                (
                    ballValveMenuOpen = new DelegateCommand(() =>
                    {
                        Page page = new BallValveMenu();
                        page.DataContext = this;
                        SlowOpacity(page);
                    })
                );
            }
        }       
                

        private ICommand dailyReportOpen;
        public ICommand DailyReportOpen
        {
            get
            {
                return dailyReportOpen ??
                (
                    dailyReportOpen = new DelegateCommand(() => OpenWindow(new DailyReportView(), new DailyReportVM(new DataContext())))
                );
            }
        }

        private ICommand fOMReportOpen;
        public ICommand FOMReportOpen
        {
            get
            {
                return fOMReportOpen ??
                (
                    fOMReportOpen = new DelegateCommand(() => OpenWindow(new FOMReportView(), new FOMReportVM(new DataContext())))
                );
            }
        }
    }
}