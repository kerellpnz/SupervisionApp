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
                    MessageBoxResult result = MessageBox.Show("Вы используете устаревшую версию программы. Для продолжения работы необходимо скачать новую версию из папки СМТО", "ВНИМАНИЕ!", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    if (result == MessageBoxResult.OK)
                    {
                        Application.Current.Shutdown();
                    }
                }
                else if (version[1].ControlValue == 1)
                {
                    MessageBox.Show(version[1].AdministratorMessage, "Сообщение от Администратора", MessageBoxButton.OK, MessageBoxImage.Warning);
                }


                
                if (TO[0].NextControl != null)
                {
                    if (TO[0].NextControl < DateTime.Now)
                    {
                        MessageBox.Show("Настал срок предъявки ТО", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                foreach (WeldingProcedures weldType in Welding)
                {
                    if (weldType.NextControl != null)
                    {
                        if (weldType.NextControl < DateTime.Now)
                        {
                            MessageBox.Show("Необходимо проверить режимы сварки", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                    }
                }

                if (LastInspection != null)
                {
                    if (LastInspection.AddMonths(1) < DateTime.Now)
                    {
                        MessageBox.Show("Настал срок проверки складирования", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }                
            }
            catch
            {
                MessageBox.Show("База данных недоступна. Проверьте подключение к сети.", "Ошибка загрузки БД", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBoxResult result = MessageBox.Show("При активном режиме помощи после открытия каждого раздела программы будет всплывать окно с краткой инструкцией к нему.", "Активировать режим помощи?", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        HelpMode = true;
                        MessageBox.Show("1.Раздел \"Входной контроль материалов\"\n\n" +
                            "\"Уплотнения ЗШ\" - база, включающая в себя уплотнения ЗШ в штучном количестве. (Графлекс, поршневые кольца, прочие РТИ)\n\n" +
                            "\"Уплотнения основного разъема\" - шнур резиновый, в кг.\n\n" +
                            "\"Уплотнения обоймы\" - уплотнения, участвующие в сборке обоймы и отвечающие за герметичность затвора (Полиамид, O-Ring/А)\n\n" +
                            "\"Лист\", \"Круг/Труба\" - база металлопроката.\n\n" +
                            "\"Пружины\" - база пружин.\n\n" +
                            "\"Поковки/Отливки\" - совместная база различных поковок и отливок, кроме поковок фланцев, днищ и отливок крестовин, они вносятся в одноименные базы внутри раздела \"Задвижка шиберная\"\n\n" +
                            "2.Раздел \"АКП\" - включает в себя базы входного контроля материалов, участвующих в процессе нанесения АКП.\n\n" +
                            "3.Раздел \"Сварка\" - база входного контроля сварочных материалов и база контроля КСС\n\n" +
                            "4.Раздел \"Периодический контроль\" - включает базы периодического контроля, которые будут оповещать пользователя " +
                            "о наступлении сроков предъявления тех. надзору режимов сварки, ТО и складирования.\n\n" +
                            "5. Раздел \"Задвижка шиберная\" - содержит в себе базы деталей и сборочных единиц ЗШ.\n\n" +
                            "Перед началом работы с программой, необходимо добавить свое ФИО в разделе \"Персонал\"\n\n" +
                            "Для более подробной справки по каждому из разделов перейдите в один из них.", "Программа \"SuperVision\"");
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
                        MessageBox.Show("1.Меню поиска.\n" +
                            "В верхней части окна приведены два поля поиска: по номеру спецификации и по номеру PID. " +
                            "Результат поиска спецификации будет отображаться в таблице ниже. Результатом поиска по PID будет являться номер, " +
                            "найденный в общей базе PID, если номер присутствует, то при нажатии кнопки \"Открыть\", откроется раздел \"Редактирование PID\" со всей занесенной в него информацией.\n\n" +
                            "2.Спецификации.\n" +
                            "Добавление спецификации осуществляется путем нажатия кнопки \"Добавить спецификацию\". Внизу таблицы должна появиться пустая строка. " +
                            "Если строчку не видно, воспользуйтесь колесиком мыши. Каждый столбец таблицы спецификации заполняется в соответствии с бумажным/цифровым ее носителем. " +
                            "Кнопка \"Удалить спецификацию\" удалит спецификацию и все принадлежащие ей PIDы.\n\n" +
                            "3.PIDы.\n" +
                            "При нажатии на строку спецификации, в нижней таблице \"PID\" отобразятся все принадлежащие этой спецификации PIDы. " +
                            "Кнопка \"Добавить PID\" откроет раздел \"Редактирование PID\" и добавит новую строчку PID к активной спецификации. " +
                            "Кнопка \"Удалить PID\" удалит выбранную строчку PID у активной спецификации. " +
                            "При двойном нажатии левой кнопкой мыши на строку PID откроется раздел \"Редактирование PID\" для выбранной строки.", "Раздел \"Спецификация\"");
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
                        MessageBox.Show("Для формирования отчета ЖТН необходимо ввести либо выбрать дату и фамилию инженера из выпадающего списка.\n\n" +
                            "Кнопка \"Сформировать отчет\" выведет информацию по заданым выше параметрам на экран, совершая выборку операций из базы данных расположенной на диске \\Тranzit.\n\n" +
                            "Кнопка \"Сформировать отчет (вариант 2)\" выведет информацию по заданым выше параметрам на экран, совершая выборку операций из базы данных, предварительно скопированной на рабочий стол.\n\n" +
                            "Первый вариант может работать быстрее в момент наименьшей загрузки базы. Второй вариант предпочтителен, когда с базой одновременно работают несколько пользвателей.\n\n" +
                            "Кнопка \"Открыть Excel\" скопирует сформированный отчет в подготовленную к печати форму Excel.", "Раздел \"Ежедневный отчет\"");
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
                        MessageBox.Show("Для формирования ежедневного отчета необходимо ввести либо выбрать дату и фамилию инженера из выпадающего списка.\n\n" +
                            "Кнопка \"Сформировать отчет\" выведет информацию по заданым выше параметрам на экран, совершая выборку операций из базы данных расположенной на диске \\Тranzit.\n\n" +
                            "Кнопка \"Сформировать отчет (вариант 2)\" выведет информацию по заданым выше параметрам на экран, совершая выборку операций из базы данных, предварительно скопированной на рабочий стол.\n\n" +
                            "Первый вариант может работать быстрее в момент наименьшей загрузки базы. Второй вариант предпочтителен, когда с базой одновременно работают несколько пользвателей.\n\n" +
                            "Кнопка \"Открыть Excel\" скопирует сформированный отчет в подготовленную к печати форму Excel.", "Раздел \"Ежедневный отчет\"");
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
                        MessageBox.Show("В этом разделе приведены заказчики АО \"ПТПА\". Список является полностью редактируемым. " +
                            "Необходим для автозаполнения поля \"Заказчик\" в разделе \"Редактирование PID\"", "Раздел \"Заказчики\"");
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
                //        MessageBox.Show("Раздел содержащий редактируемый список инженеров. Нажимая на названия столбцов, можно осуществлять сортировку списка. " +
                //            "Данный список выводится в поле \"Инженер\" таблиц операций.\n\n" +
                //            "Кнопка \"Добавить\" создаст готовую для заполнения строку в списке.", "Раздел \"Персонал\"");
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
                                    MessageBox.Show("Раздел содержащий редактируемый список инженеров. Нажимая на названия столбцов, можно осуществлять сортировку списка. " +
                                        "Данный список выводится в поле \"Инженер\" таблиц операций.\n\n" +
                                        "Кнопка \"Добавить\" создаст готовую для заполнения строку в списке.", "Раздел \"Персонал\"");
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
                        MessageBox.Show("Раздел содержащий список журналов технического надзора. " +
                            "Данный список выводится в поле \"Журнал\" таблиц операций.\n\n" +
                            "Кнопка \"Добавить\" создаст готовую для заполнения строку в списке.\n" +
                            "Для того, чтобы закрытый журнал перестал отображаться в таблицах операций, необходимо выставить галочку в поле \"Журнал закрыт\".", "Раздел \"Журналы ТН\"");
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
                        MessageBox.Show("Для добавления нового листа нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с листом или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного листа.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного листа, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"Лист\"");
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
                    MessageBox.Show("Во избежание дальнейшей путаницы, связанной с некорректным внесением входного контроля фланцев и днищ, данный раздел закрыт и доступен только для применения уже внесенных поковок/отливок.\n" +
                        "Поковки и отливки теперь вносятся непосредственно в одноименные разделы внутри \"ЗАДВИЖКА ШИБЕРНАЯ\": поковки обойм в \"Обоймы\", поковки втулок в \"Втулка центральная\", отливки катушек в \"Катушки\" и тд.\n" +
                        "Поиск принятых поковок во время стыковок (сборок под сварку) осуществляется по плавке, с дальнейшим переходом в них и присвоения ЗК.\n" +
                        "Поиск принятых поковок обойм для запрессовки/контроля ТО осуществляется внутри раздела по плавке, сертификату, черт. номеру, с дальнейшим присвоением ЗК.");
                    if (MainViewModel.HelpMode == true)
                    {
                        MessageBox.Show("Для добавления новой поковки/отливки нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с поковкой/отливкой или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Кнопка \"Копировать\" предложит ВВЕСТИ требуемое количество копий выбранной поковки/отливки, автоматически продолжит нумерацию, скопирует всю информацию внутри, включая заполненные операции ПТК, и добавит копии в базу в виде новых строк.", "Раздел \"Поковка/Отливка\"");
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
                        MessageBox.Show("Для добавления нового круга/трубы нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с кругом/трубой или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит ее в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"Круг/Труба\"");
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
                        MessageBox.Show("Для добавления новой партии АКП нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с АКП или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного АКП.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного АКП, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"АКП\"");
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
                        MessageBox.Show("Для добавления новой партии АКП нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с АКП или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного АКП.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного АКП, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"АКП\"");
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
                        MessageBox.Show("Для добавления новой партии АКП нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с АКП или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного АКП.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного АКП, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"АКП\"");
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
                        MessageBox.Show("Для добавления новой партии АКП нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с АКП или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного АКП.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного АКП, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"АКП\"");
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
                        MessageBox.Show("Для добавления новой партии сварочных материалов нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке со сварочным материалом или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит ее в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"Сварочные материалы\"");
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
                        MessageBox.Show("Для добавления нового КСС нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с КСС или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного КСС.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного КСС, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"КСС\"");
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
                        MessageBox.Show("Данный раздел представляет собой общий журнал проверок складирования.\n" +
                            "Для добавления новой записи выберете пункт из выпадающего списка " +
                        "\"Выбор пункта\" и нажмите кнопку \"Добавить операцию\". В поле \"Дата последней проверки\" установится последняя дата контроля складирования.", "Раздел \"Контроль складирования\"");
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
                        MessageBox.Show("В этом разделе фиксируется дата последней предъявки ТО техническому надзору. " +
                            "В ежедневный отчет/ЖТН данная запись не идет. Операция контроля ТО конкретной детали, с записью в ЖТН, закрывается " +
                            "непосредственно в самой детали.\n\n" +
                            "Для внесения изменений щелкните два раза по строке контроля ТО", "Раздел \"Контроль ТО\"");
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
                        MessageBox.Show("В этом разделе фиксируются даты последней предъявки режимов сварки техническому надзору. " +
                            "В ежедневный отчет/ЖТН данные записи не идут. Операции контроля режимов сварки конкретных сборочных единиц, с записью в ЖТН, закрываются " +
                            "непосредственно в самих сборочных единицах.\n\n" +
                            "Для внесения изменений щелкните два раза по строке нужного вида сварки.", "Раздел \"Режимы сварки\"");
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
                        MessageBox.Show("Данный раздел представляет собой общий журнал контроля разрешительной документации и проверок знаний персонала завода.\n" +
                            "Для добавления новой записи выберете пункт из выпадающего списка " +
                        "\"Выбор пункта\" и нажмите кнопку \"Добавить операцию\".", "Раздел \"Контроль разрешительной документации\"");
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
                        MessageBox.Show("Для добавления новой партии уплотнений нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с уплотнением или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного уплотнения.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного уплотнения, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"Уплотнения обоймы\"");
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
                        MessageBox.Show("Для добавления новой партии уплотнений нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с уплотнением или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного уплотнения.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного уплотнения, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"Уплотнения ЗШ\"");
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
                        MessageBox.Show("Для добавления новой партии уплотнений нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с уплотнением или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранного уплотнения.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранного уплотнения, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"Уплотнения основного разъема\"");
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
                        MessageBox.Show("Для добавления новой задвижки нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с задвижкой или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.", "Раздел \"Задвижки\"");
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
                        MessageBox.Show("Для добавления новой втулки(016) нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с втулкой(016) или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Втулки(016)\"");
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
                        MessageBox.Show("Для добавления новой втулки(008) нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с втулкой(008) или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Втулки(008)\"");
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
                        MessageBox.Show("Для добавления нового кольца(047) нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с кольцом(047) или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Кольцо(047)\"");
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
                        MessageBox.Show("Для добавления нового кольца(043) нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с кольцом(043) или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Кольцо(043)\"");
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
                        MessageBox.Show("Для добавления новой ходовой втулки нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с ходовой втулкой или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Втулки ходовые\"");
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
                        MessageBox.Show("Для добавления новой партии гаек нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с партией гаек или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n", "Раздел \"Гайки\"");
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
                        MessageBox.Show("Для добавления новой катушки нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с катушкой или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Катушки\"");
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
                        MessageBox.Show("Для добавления нового корпуса/крестовины нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с корпусом/крестовиной или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует информацию внутри выбранной позиции, заполненные операции ПТК, за исключением собранных деталей, и добавит ее в виде новой строки " +
                        "в базе доступной для редактирования.\n\n", "Раздел \"Корпус\"");
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
                        MessageBox.Show("Для добавления нового днища нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с днищем или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n", "Раздел \"Днища\"");
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
                        MessageBox.Show("Для добавления новой крышки нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с крышкой или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует информацию внутри выбранной позиции, заполненные операции ПТК, за исключением собранных деталей, и добавит ее в виде новой строки " +
                        "в базе доступной для редактирования.\n\n", "Раздел \"Корпус\"");
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
                        MessageBox.Show("Для добавления нового фланца нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с фланцем или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n", "Раздел \"Фланцы\"");
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
                        MessageBox.Show("Для добавления новой партии пружин нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с пружинами или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной партии пружин.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.", "Раздел \"Пружины\"");
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
                        MessageBox.Show("Для добавления новой обоймы нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с задвижкой или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Обоймы\"");
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
                        MessageBox.Show("Для добавления нового шибера нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с шибером или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Шибера\"");
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
                        MessageBox.Show("Для добавления новой партии шпилек нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с партией шпилек или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n", "Раздел \"Шпильки\"");
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
                        MessageBox.Show("Для добавления нового шпинделя нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с шпинделем или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n" +
                        "Кнопка \"Копировать ЗК\" потребует ввести количество деталей, которое необходимо добавить к текущему ЗК, " +
                        "продолжит нумерацию автоматически относительно выбранной детали и скопирует всю информацию внутри выбранной позиции, " +
                        "включая заполненные операции ПТК, в каждую новую строку.", "Раздел \"Шпиндели\"");
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
                        MessageBox.Show("Для добавления новой стойки нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке со стойкой или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n", "Раздел \"Стойка\"");
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
                        MessageBox.Show("Для добавления новой партии штифтов нажмите кнопку \"Добавить\".\n\n" +
                        "Двойной щелчок по строке с партией штифтов или выбор строки + кнопка \"Изменить\" - " +
                        "откроет раздел редактирования выбранной позиции.\n\n" +
                        "Нажимая на название столбцов (\"Номер\", \"Статус\" и т.д.) можно осуществлять сортировку таблицы.\n" +
                        "Рядом с каждым столбцом находится кнопка фильтра, с помощью которой можно осуществлять фильтрацию таблицы по внесенным параметрам.\n\n" +
                        "Кнопка \"Копировать\" единично скопирует всю информацию внутри выбранной позиции, включая заполненные операции ПТК, и добавит его в виде новой строки " +
                        "в базе доступной для редактирования.\n\n", "Раздел \"Штифты\"");
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
                            MessageBox.Show("\"Задвижки\" - раздел содержащий перечень собранных или готовых к сборке задвижек, с присвоенным заводским номером. " +
                                "В данном разделе можно произвести сборку ЗШ, отметить ПСИ ЗШ, АКП ЗШ, отгрузку ЗШ.\n\n" +
                                "\"Корпус\" - раздел содержащий перечень корпусов/крестовин. В данный раздел вносятся литые крестовины после входного контроля, " +
                                "вносятся корпуса, изготавливаемые из поковок. В разделе производится сборка/сварка корпусов, контроль геометрии, толщины, промывки, документации и т.д.\n\n" +
                                "\"Крышка\" - раздел содержащий перечень собранных или готовых к сборке крышек. " +
                                "В разделе производится сборка/сварка крышек, контроль геометрии, толщины, документации и т.д.\n\n" +
                                "Базы отдельных деталей ЗШ (Фланец, Днище, Шпиндель и т.д.) - разделы содержащие перечни конкретных деталей ЗШ, со своими ПТК.", "Раздел \"Задвижка шиберная\"");
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