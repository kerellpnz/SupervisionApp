using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Journals.Detailing;
using Supervision.Commands;
using Supervision.Views.EntityViews.DetailViews.Valve;
using DataLayer.Entities.Materials;
using BusinessLayer.Repository.Implementations.Entities.Material;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve
{
    public class SaddleVM : ViewModelBase
    {
        private readonly DataContext db;
        private IList<Saddle> allInstances;
        private ICollectionView allInstancesView;
        private Saddle selectedItem;
        private readonly SaddleRepository saddleRepo;
        //private IList<MetalMaterial> materials;

        //private readonly MetalMaterialRepository materialRepo;

        private string name;
        private string dn = "";
        private string pn = "";
        private string zk = "";
        private string number = "";
        private string drawing = "";
        private string status = "";
        private string certificate = "";
        private string material = "";
        private string melt = "";
        private string pid = "";

        #region Filter

        public string PID
        {
            get => pid;
            set
            {
                pid = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.PID.Number != null)
                    {
                        return item.PID.Number.ToLower().Contains(PID.ToLower());
                    }
                    else return true;
                };
            }
        }

        public string DN
        {
            get => dn;
            set
            {
                dn = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.DN != null)
                    {
                        return item.DN.ToLower().Contains(DN.ToLower());
                    }
                    else return true;
                };
            }
        }

        public string PN
        {
            get => pn;
            set
            {
                pn = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.PN != null)
                    {
                        return item.PN.ToLower().Contains(PN.ToLower());
                    }
                    else return true;
                };
            }
        }

        public string ZK
        {
            get => zk;
            set
            {
                zk = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.ZK != null)
                    {
                        return item.ZK.ToLower().Contains(ZK.ToLower());
                    }
                    else return true;
                };
            }
        }

        public string Number
        {
            get => number;
            set
            {
                number = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.Number != null)
                    {
                        return item.Number.ToLower().Contains(Number.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Drawing
        {
            get => drawing;
            set
            {
                drawing = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.Drawing != null)
                    {
                        return item.Drawing.ToLower().Contains(Drawing.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Status
        {
            get => status;
            set
            {
                status = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.Status != null)
                    {
                        return item.Status.ToLower().Contains(Status.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Material
        {
            get => material;
            set
            {
                material = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.Material != null)
                    {
                        return item.Material.ToLower().Contains(Material.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Melt
        {
            get => melt;
            set
            {
                melt = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.Melt != null)
                    {
                        return item.Melt.ToLower().Contains(Melt.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Certificate
        {
            get => certificate;
            set
            {
                certificate = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Saddle item && item.Certificate != null)
                    {
                        return item.Certificate.ToLower().Contains(Certificate.ToLower());
                    }
                    else return true;
                };
            }
        }
        #endregion

        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        public Saddle SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public IList<Saddle> AllInstances
        {
            get => allInstances;
            set
            {
                allInstances = value;
                RaisePropertyChanged();
            }
        }
        public ICollectionView AllInstancesView
        {
            get => allInstancesView;
            set
            {
                allInstancesView = value;
                RaisePropertyChanged();
            }
        }

        //public IList<MetalMaterial> Materials
        //{
        //    get => materials;
        //    set
        //    {
        //        materials = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public static SaddleVM LoadSaddleVM(DataContext context)
        {
            SaddleVM vm = new SaddleVM(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
        }

        public IAsyncCommand UpdateListCommand { get; private set; }
        private async Task UpdateList()
        {
            try
            {
                IsBusy = true;
                //Materials = await Task.Run(() => materialRepo.GetAllAsync());
                AllInstances = new ObservableCollection<Saddle>();
                AllInstances = await Task.Run(() => saddleRepo.GetAllAsync());
                AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand AddNewItemCommand { get; private set; }
        private async Task AddNewItem()
        {
            try
            {
                IsBusy = true;

                //Saddle EmptyItem = new Saddle();
                //EmptyItem.MetalMaterial = Materials[0];
                //EmptyItem.MetalMaterialId = 1;

                SelectedItem = await saddleRepo.AddAsync(new Saddle());
                var tcpPoints = await saddleRepo.GetTCPsAsync();
                var records = new List<SaddleJournal>();
                foreach (var tcp in tcpPoints)
                {
                    var journal = new SaddleJournal(SelectedItem, tcp);
                    if (journal != null)
                        records.Add(journal);
                }
                await saddleRepo.AddJournalRecordAsync(records);
                EditSelectedItem();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand CopySelectedItemCommand { get; private set; }
        private async Task CopySelectedItem()
        {
            if (SelectedItem != null)
            {
                try
                {
                    IsBusy = true;
                    var temp = await saddleRepo.GetByIdIncludeAsync(SelectedItem.Id);
                    var copy = await saddleRepo.AddAsync(new Saddle(temp));
                    var jour = new ObservableCollection<SaddleJournal>();
                    if (temp.SaddleJournals != null)
                    {
                        foreach (var i in temp.SaddleJournals)
                        {
                            var record = new SaddleJournal(copy.Id, i);
                            jour.Add(record);
                        }
                        int value = await Task.Run(() => saddleRepo.UpdateJournalRecord(jour));
                        if (value == 0)
                        {
                            MessageBox.Show("При копировании произошла ошибка сервера. Проверьте целостность данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }                    
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public IAsyncCommand GroupCopySelectedItemCommand { get; private set; }
        private async Task GroupCopySelectedItem()
        {
            if (SelectedItem != null)
            {
                try
                {
                    IsBusy = true;
                    string Input = Microsoft.VisualBasic.Interaction.InputBox("Введите количество деталей, которое необходимо добавить к текущему ЗК, нумерация продолжится автоматически относительно выбранной детали:");
                    int x = 0;
                    if (System.Int32.TryParse(Input, out x))
                    {
                        // you know that the parsing attempt
                        // was successful
                        if (x > 20)
                        {
                            MessageBox.Show("Установлено ограничение: не больше 20 шт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            var temp = await saddleRepo.GetByIdIncludeAsync(SelectedItem.Id);
                            Saddle detail = new Saddle(temp);
                            int numb = 0;
                            if (System.Int32.TryParse(detail.Number, out numb))
                            {
                                for (int y = 0; y < x; y++)
                                {
                                    numb += 1;
                                    detail.Number = numb.ToString();
                                    var copy = await saddleRepo.AddAsync(new Saddle(detail));
                                    var jour = new ObservableCollection<SaddleJournal>();
                                    if (temp.SaddleJournals != null)
                                    {
                                        foreach (var i in temp.SaddleJournals)
                                        {
                                            var record = new SaddleJournal(copy.Id, i);
                                            jour.Add(record);
                                        }
                                        int value = await Task.Run(() => saddleRepo.UpdateJournalRecord(jour));
                                        if (value == 0)
                                        {
                                            MessageBox.Show("При копировании произошла ошибка сервера. Проверьте целостность данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }                                        
                                }
                                detail = null;
                            }
                            else
                            {
                                MessageBox.Show("В поле Номер установлено некорректное значение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введено некорректное значение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public IAsyncCommand RemoveSelectedItemCommand { get; private set; }

        public ICommand EditSelectedItemCommand { get; private set; }
        private void EditSelectedItem()
        {
            if (SelectedItem != null)
            {
                _ = new SaddleEditView
                {
                    DataContext = SaddleEditVM.LoadSaddleEditVM(SelectedItem.Id, SelectedItem, db)
                };
                if (MainViewModel.HelpMode == true)
                {
                    MessageBox.Show("Поля внутри данного раздела заполняются в соответствии с их описанием, без добавления \"серт.\", " +
                        "\"партия\", \"шт.\", \"№\" и т.п. внутри поля.\n\n" +
                        "Раздел содержит списки материалов - \"Прокат\", \"Поковка\". Чтобы быстро найти необходимый материал, " +
                        "в поле \"Прокат\" или \"Поковка\" достаточно начать набирать его плавку. Если материал не находится, скорее всего он отсутствует в базе входного контроля.\n" +
                        "После выбора материала, для переноса данных (плавка, материал, сертификат) в деталь, необходимо нажать кнопку \"Сохранить\".\n\n" +
                        
                        "Поковку следует выбирать с отсутствующим в ее обозначении номером или зк детали после ее типа (Пример: \"1234пл./09Г2С/№1/Втулка 2519-20\" - означает, что поковка" +
                        "использована в детали с ЗК 2519-20; \"1234пл./09Г2С/№2/Втулка\" - означает, что поковка нигде не применена).\n" +
                        "Желательно чтобы номер выбранной поковки и номер детали в ЗК совпадали между собой.\n\n" +
                        "Вкладка \"Операции\" содержит все операции ПТК относящиеся к текущей детали. " +
                        "Чтобы полностью закрыть операцию нужно ввести дату, фамилию, статус, выбрать журнал ТН, написать номер " +
                        "замечания, если необходимо.\n" +
                        "Если внутри данного раздела не оказалось требуемых операций ПТК доступных для заполнения, выберете ее из выпадающего списка " +
                        "\"Выбор пункта\" и нажмите кнопку \"Добавить операцию\". Если после этого необходимая операция ПТК по-прежнему не отобразилась на экране " +
                        "обратитесь к администратору.\n\n" +
                        "Текст набранный в поле \"Примечание\" внутри каждой операции отобразится в журнале при печати ежедневного отчета, " +
                        "текст в поле \"Примечание\" внизу данного раздела, останется внутри текущей детали и будет являться дополнительной информацией к ней.\n\n" +

                        "После заполнения нужных полей и закрытия операций, необходимо нажать кнопку \"Сохранить\".", "Раздел \"Редактирование обоймы\"");
                }
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        protected override void CloseWindow(object obj)
        {
            Window w = obj as Window;
            w?.Close();
        }

        private bool CanExecute()
        {
            return true;
        }

        public SaddleVM(DataContext context)
        {
            db = context;
            saddleRepo = new SaddleRepository(db);
            //materialRepo = new MetalMaterialRepository(db);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem, CanExecute);
            CopySelectedItemCommand = new AsyncCommand(CopySelectedItem, CanExecute);
            GroupCopySelectedItemCommand = new AsyncCommand(GroupCopySelectedItem, CanExecute);
            EditSelectedItemCommand = new Command(o => EditSelectedItem());
            CloseWindowCommand = new Command(o => CloseWindow(o));
        }
    }
}
