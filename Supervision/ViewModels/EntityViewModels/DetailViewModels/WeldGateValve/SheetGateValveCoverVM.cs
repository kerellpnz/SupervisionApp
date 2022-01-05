using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.Detailing.SheetGateValveDetails;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using Microsoft.EntityFrameworkCore;
using Supervision.Commands;
using Supervision.Views.EntityViews.DetailViews.WeldGateValve;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.WeldGateValve
{
    public class SheetGateValveCoverVM : ViewModelBase
    {
        private readonly DataContext db;
        private readonly SheetGateValveCoverRepository repo;
        private IEnumerable<SheetGateValveCover> allInstances;
        private ICollectionView allInstancesView;
        private SheetGateValveCover selectedItem;

        private string name;
        private string dn;
        private string number = "";
        private string drawing = "";
        private string status = "";
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
                    if (obj is SheetGateValveCover item && item.PID.Number != null)
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
                    if (obj is SheetGateValveCover item && item.DN != null)
                    {
                        return item.DN.ToLower().Contains(Status.ToLower());
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
                    if (obj is SheetGateValveCover item && item.Number != null)
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
                    if (obj is SheetGateValveCover item && item.Drawing != null)
                    {
                        return item.Drawing.ToLower().Contains(Drawing.ToLower());
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
                    if (obj is SheetGateValveCover item && item.Material != null)
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
                    if (obj is SheetGateValveCover item && item.Melt != null)
                    {
                        return item.Melt.ToLower().Contains(Melt.ToLower());
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
                    if (obj is SheetGateValveCover item && item.Status != null)
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

        public SheetGateValveCover SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<SheetGateValveCover> AllInstances
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

        public static SheetGateValveCoverVM LoadVM(DataContext context)
        {
            SheetGateValveCoverVM vm = new SheetGateValveCoverVM(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
        }

        public IAsyncCommand UpdateListCommand { get; private set; }
        private async Task UpdateList()
        {
            try
            {
                IsBusy = true;
                AllInstances = new ObservableCollection<SheetGateValveCover>();
                AllInstances = await Task.Run(() => repo.GetAllAsync());
                AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);
                //if (AllInstances.Count() != 0)
                //{
                //    Name = AllInstances.First().Name;
                //}
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
                SelectedItem = await repo.AddAsync(new SheetGateValveCover());
                var tcpPoints = await repo.GetTCPsAsync();
                var records = new List<SheetGateValveCoverJournal>();
                foreach (var tcp in tcpPoints)
                {
                    var journal = new SheetGateValveCoverJournal(SelectedItem, tcp);
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
                    var copy = await repo.AddAsync(new SheetGateValveCover(temp));
                    var jour = new ObservableCollection<SheetGateValveCoverJournal>();
                    if (temp.SheetGateValveCoverJournals != null)
                    {
                        foreach (var i in temp.SheetGateValveCoverJournals)
                        {
                            var record = new SheetGateValveCoverJournal(copy.Id, i);
                            jour.Add(record);
                        }
                        repo.UpdateJournalRecord(jour);
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
                _ = new WeldGateValveCoverEditView
                {
                    DataContext = SheetGateValveCoverEditVM.LoadVM(SelectedItem.Id, SelectedItem, db)
                };
                if (MainViewModel.HelpMode == true)
                {
                    MessageBox.Show("Поля внутри данного раздела заполняются в соответствии с их описанием, без добавления \"серт.\", " +
                        "\"партия\", \"шт.\", \"№\" и т.п. внутри поля.\n\n" +
                        "Раздел содержит выпадающие списки деталей, участвующих в сборке крышки. " +
                        "После выбора детали из списка, она закрепляется за текущей сборочной единицей, нажатием на кнопку \"Сохранить\" можно выполнить проверку " +
                        "на отсутствие принадлежности выбранной детали к другой сборочной единице.\n" +
                        "Если напротив выпадающего списка есть кнопка \"Добавить\", после выбора детали из списка необходимо ее нажать.\n\n" +

                        "Поковку следует выбирать с отсутствующим в ее обозначении номером или зк детали после ее типа (Пример: \"1234пл./09Г2С/№1/Крышка 2519-20\" - означает, что поковка" +
                        "использована в детали с ЗК 2519-20; \"1234пл./09Г2С/№1/Крышка\" - означает, что поковка нигде не применена).\n" +
                        "Желательно чтобы номер выбранной поковки и номер детали в ЗК совпадали между собой.\n\n" +

                        "Раздел содержит все операции ПТК относящиеся к текущей крышке. " +
                        "Операции разбиты на дополнительные вкладки по своему типу.\n" +
                        "Чтобы полностью закрыть операцию нужно ввести дату, фамилию, статус, выбрать журнал ТН, написать номер " +
                        "замечания, если необходимо.\n" +
                        "Если внутри данного раздела не оказалось требуемых операций ПТК доступных для заполнения, выберете ее из выпадающего списка " +
                        "\"Выбор пункта\" и нажмите кнопку \"Добавить операцию\". Операция должна отобразиться во вкладке согласно своему типу. Если после этого необходимая операция ПТК по-прежнему не отобразилась на экране " +
                        "обратитесь к администратору.\n\n" +
                        "Текст набранный в поле \"Примечание\" внутри каждой операции отобразится в журнале при печати ежедневного отчета, " +
                        "текст в поле \"Примечание\" внизу данного раздела, останется внутри текущей детали и будет являться дополнительной информацией к ней.\n\n" +

                        "После заполнения нужных полей и закрытия операций, необходимо нажать кнопку \"Сохранить\".\n\n" +
                        "ВНИМАНИЕ: Выход из раздела, являющегося сборкой, обязательно осуществлять только посредством кнопки \"Закрыть\", так как программа должна выполнить необходимые операции для " +
                        "безопасного закрытия данной сборки.", "Раздел \"Редактирование Крышки\"");
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

        public SheetGateValveCoverVM(DataContext context)
        {
            db = context;
            repo = new SheetGateValveCoverRepository(db);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem, CanExecute);
            CopySelectedItemCommand = new AsyncCommand(CopySelectedItem, CanExecute);
            EditSelectedItemCommand = new Command(o => EditSelectedItem());
            CloseWindowCommand = new Command(o => CloseWindow(o));
        }
    }
}
