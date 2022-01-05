using DataLayer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using Supervision.Commands;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using Supervision.Views.EntityViews.DetailViews.WeldGateValve;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels
{
    public class CoverSleeveVM : ViewModelBase
    {
        private readonly DataContext db;
        private IList<CoverSleeve> allInstances;
        private ICollectionView allInstancesView;
        private CoverSleeve selectedItem;
        private readonly CoverSleeveRepository repo;

        private string name;
        private string number = "";
        private string drawing = "";
        private string status = "";
        private string certificate = "";
        private string material = "";
        private string melt = "";
        private string zk = "";
        private string dn = "";

        #region Filter

        public string ZK
        {
            get => zk;
            set
            {
                zk = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is CoverSleeve item && item.ZK != null)
                    {
                        return item.ZK.ToLower().Contains(ZK.ToLower());
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
                    if (obj is CoverSleeve item && item.DN != null)
                    {
                        return item.DN.ToLower().Contains(DN.ToLower());
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
                    if (obj is CoverSleeve item && item.Number != null)
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
                    if (obj is CoverSleeve item && item.Drawing != null)
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
                    if (obj is CoverSleeve item && item.Status != null)
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
                    if (obj is CoverSleeve item && item.MetalMaterial.Material != null)
                    {
                        return item.MetalMaterial.Material.ToLower().Contains(Material.ToLower());
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
                    if (obj is CoverSleeve item && item.MetalMaterial.Melt != null)
                    {
                        return item.MetalMaterial.Melt.ToLower().Contains(Melt.ToLower());
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
                    if (obj is CoverSleeve item && item.Certificate != null)
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

        public CoverSleeve SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public IList<CoverSleeve> AllInstances
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

        public static CoverSleeveVM LoadVM(DataContext context)
        {
            CoverSleeveVM vm = new CoverSleeveVM(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
        }

        public IAsyncCommand UpdateListCommand { get; private set; }
        private async Task UpdateList()
        {
            try
            {
                IsBusy = true;
                AllInstances = new ObservableCollection<CoverSleeve>();
                AllInstances = await Task.Run(() => repo.GetAllAsync());
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
                SelectedItem = await repo.AddAsync(new CoverSleeve());
                var tcpPoints = await repo.GetTCPsAsync();
                var records = new List<CoverSleeveJournal>();
                foreach (var tcp in tcpPoints)
                {
                    var journal = new CoverSleeveJournal(SelectedItem, tcp);
                    if (journal != null)
                        records.Add(journal);
                }
                await repo.AddJournalRecordAsync(records);
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
                    var temp = await repo.GetByIdIncludeAsync(SelectedItem.Id);
                    var copy = await repo.AddAsync(new CoverSleeve(temp));
                    var jour = new ObservableCollection<CoverSleeveJournal>();
                    if ( temp.CoverSleeveJournals != null)
                    {
                        foreach (var i in temp.CoverSleeveJournals)
                        {
                            var record = new CoverSleeveJournal(copy.Id, i);
                            jour.Add(record);
                        }
                        int value = await Task.Run(() => repo.UpdateJournalRecord(jour));
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
                    
                    if (System.Int32.TryParse(Input, out int x))
                    {
                        // you know that the parsing attempt
                        // was successful
                        if (x > 20)
                        {
                            MessageBox.Show("Установлено ограничение не больше 20 шт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            var temp = await repo.GetByIdIncludeAsync(SelectedItem.Id);
                            CoverSleeve detail = new CoverSleeve(temp);
                            if (System.Int32.TryParse(detail.Number, out int numb))
                            {
                                for (int y = 0; y < x; y++)
                                {
                                    numb += 1;
                                    detail.Number = numb.ToString();
                                    var copy = await repo.AddAsync(new CoverSleeve(detail));
                                    var jour = new ObservableCollection<CoverSleeveJournal>();
                                    if (temp.CoverSleeveJournals != null)
                                    {
                                        foreach (var i in temp.CoverSleeveJournals)
                                        {
                                            var record = new CoverSleeveJournal(copy.Id, i);
                                            jour.Add(record);
                                        }
                                        int value = await Task.Run(() => repo.UpdateJournalRecord(jour));
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
                _ = new CoverSleeveEditView
                {
                    DataContext = CoverSleeveEditVM.LoadVM(SelectedItem.Id, SelectedItem, db)
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

                        "После заполнения нужных полей и закрытия операций, необходимо нажать кнопку \"Сохранить\".", "Раздел \"Редактирование Втулки(016)\"");
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

        public CoverSleeveVM(DataContext context)
        {
            db = context;
            repo = new CoverSleeveRepository(db);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem, CanExecute);
            CopySelectedItemCommand = new AsyncCommand(CopySelectedItem, CanExecute);
            GroupCopySelectedItemCommand = new AsyncCommand(GroupCopySelectedItem, CanExecute);
            EditSelectedItemCommand = new Command(o => EditSelectedItem());
            CloseWindowCommand = new Command(o => CloseWindow(o));
        }
    }
}
