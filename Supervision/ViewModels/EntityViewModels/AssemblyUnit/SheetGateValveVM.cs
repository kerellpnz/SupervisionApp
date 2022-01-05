using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Journals.AssemblyUnits;
using Supervision.Commands;
using Supervision.Views.EntityViews.AssemblyUnit;

namespace Supervision.ViewModels.EntityViewModels.AssemblyUnit
{
    public class SheetGateValveVM : ViewModelBase
    {
        private readonly DataContext db;
        private readonly SheetGateValveRepository repo;
        private IEnumerable<SheetGateValve> allInstances;
        private ICollectionView allInstancesView;
        private SheetGateValve selectedItem;

        private string name;
        private string number = "";
        private string drawing = "";
        private string status = "";
        private string pid = "";
        //private bool isWindowOpen = true;

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
                    if (obj is SheetGateValve item && item.PID.Number != null)
                    {
                        return item.PID.Number.ToLower().Contains(PID.ToLower());
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
                    if (obj is SheetGateValve item && item.Number != null)
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
                    if (obj is SheetGateValve item && item.Drawing != null)
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
                    if (obj is SheetGateValve item && item.Status != null)
                    {
                        return item.Status.ToLower().Contains(Status.ToLower());
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

        public SheetGateValve SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<SheetGateValve> AllInstances
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

        public static SheetGateValveVM LoadVM(DataContext context)
        {
            SheetGateValveVM vm = new SheetGateValveVM(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
        }

        public IAsyncCommand UpdateListCommand { get; private set; }
        private async Task UpdateList()
        {
            try
            {
                IsBusy = true;                
                AllInstances = new ObservableCollection<SheetGateValve>();
                AllInstances = await Task.Run(() => repo.GetAllAsync());
                AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);
                //Task task = Task.Run(async () =>
                //{
                //    while (isWindowOpen)
                //    {
                //        Thread.Sleep(5000);
                //        IsBusy = true;
                //        await repo.LoadAsync();
                //        AllInstances = repo.GetObservableCollection();
                //        AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);
                //        IsBusy = false;
                //    }
                //});
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
                SelectedItem = await repo.AddAsync(new SheetGateValve());
                var tcpPoints = await repo.GetTCPsAsync();
                var coatTcp = await repo.GetCoatingTCPsAsync();
                var records = new List<SheetGateValveJournal>();
                var coatRecords = new List<CoatingJournal>();
                foreach (var tcp in tcpPoints)
                {
                    var journal = new SheetGateValveJournal(SelectedItem, tcp);
                    if (journal != null)
                        records.Add(journal);
                }
                foreach (var tcp in coatTcp)
                {
                    var journal = new CoatingJournal(SelectedItem, tcp);
                    if (journal != null)
                        coatRecords.Add(journal);
                }
                await repo.AddJournalRecordAsync(records);
                await repo.AddCoatJournalRecordAsync(coatRecords);
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
                    var copy = await repo.AddAsync(new SheetGateValve(temp));
                    var jour = new ObservableCollection<SheetGateValveJournal>();
                    foreach (var i in temp.SheetGateValveJournals)
                    {
                        var record = new SheetGateValveJournal(copy.Id, i);
                        jour.Add(record);
                    }
                    repo.UpdateJournalRecord(jour);
                    var coatJour = new ObservableCollection<CoatingJournal>();
                    foreach (var i in temp.CoatingJournals)
                    {
                        var record = new CoatingJournal(copy.Id, i);
                        coatJour.Add(record);
                    }
                    repo.UpdateCoatJournalRecord(coatJour);
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
                _ = new SheetGateValveEditView
                {
                    DataContext = SheetGateValveEditVM.LoadVM(SelectedItem.Id, SelectedItem, db)
                };
                if (MainViewModel.HelpMode == true)
                {
                    MessageBox.Show("Поля внутри данного раздела заполняются в соответствии с их описанием, поле \"Номер\" заполняется без добавления \"№\", \"О\" внутри поля.\n\n" +
                        "Вкладка \"Комплектация\" содержит выпадающие списки деталей, участвующих в сборке ЗШ. " +
                        "После выбора детали из списка, она закрепляется за текущей ЗШ, нажатием на кнопку \"Сохранить\" можно выполнить проверку " +
                        "на отсутствие принадлежности выбранной детали к другой сборочной единице.\n" +
                        "Если напротив выпадающего списка есть кнопка \"Добавить\", после выбора детали из списка необходимо ее нажать.\n" +
                        "Вкладка \"Комплектация\" содержит дополнительные вкладки, которые являются продолжением сборки ЗШ, а также выбором материалов АКП.\n\n" +
                        "Вкладка \"Операции\" содержит все операции ПТК относящиеся к текущей ЗШ. " +
                        "Операции разбиты на дополнительные вкладки по своему типу.\n" +
                        "Чтобы полностью закрыть операцию нужно ввести дату, фамилию, статус, выбрать журнал ТН, написать номер " +
                        "замечания, если необходимо.\n" +
                        "Если внутри данного раздела не оказалось требуемых операций ПТК доступных для заполнения, выберете ее из выпадающего списка " +
                        "\"Выбор пункта\" и нажмите кнопку \"Добавить операцию\". Операция должна отобразиться во вкладке согласно своему типу. Если после этого необходимая операция ПТК по-прежнему не отобразилась на экране " +
                        "обратитесь к администратору.\n\n" +
                        "Текст набранный в поле \"Примечание\" внутри каждой операции отобразится в журнале при печати ежедневного отчета, " +
                        "текст в поле \"Примечание\" внизу данного раздела, останется внутри текущей детали и будет являться дополнительной информацией к ней."
                        
                        , "Раздел \"Редактирование ЗШ\"");
                }
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        protected override void CloseWindow(object obj)
        {
            //isWindowOpen = false;
            Window w = obj as Window;
            w?.Close();
        }

        private bool CanExecute()
        {
            return true;
        }

        public SheetGateValveVM(DataContext context)
        {
            db = context;
            repo = new SheetGateValveRepository(db);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem, CanExecute);
            CopySelectedItemCommand = new AsyncCommand(CopySelectedItem, CanExecute);
            EditSelectedItemCommand = new Command(o => EditSelectedItem());
            CloseWindowCommand = new Command(o => CloseWindow(o));
        }
    }
}
