using DataLayer;
using System.Collections.Generic;
using System.Windows;
using DataLayer.TechnicalControlPlans;
using DataLayer.Journals;
using System.Threading.Tasks;
using BusinessLayer.Repository.Implementations.Entities;
using System.Windows.Input;
using Supervision.Commands;
using System.Diagnostics;
using DataLayer.Entities.AssemblyUnits;
using Supervision.Views.EntityViews.AssemblyUnit;
using Supervision.ViewModels.EntityViewModels.AssemblyUnit;
using DataLayer.Entities.Detailing;
using System.Text.RegularExpressions;

namespace Supervision.ViewModels
{
    public class PIDEditVM : ViewModelBase
    {
        private readonly DataContext db;
        private readonly PIDRepository pIDRepo;
        private readonly JournalNumberRepository journalRepo;
        //private readonly ProductTypeRepository productTypeRepo;
        private readonly InspectorRepository inspectorRepo;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<PIDTCP> points;
        private IEnumerable<Inspector> inspectors;
        private IEnumerable<PIDJournal> journal;
        private readonly BaseTable parentEntity;
        private PIDTCP selectedTCPPoint;
        private IEnumerable<string> descriptions;
        private IEnumerable<string> consignees;

        private PID selectedItem;
        //private IList<ProductType> productTypes;
        private IEnumerable<string> designations;
        private BaseAssemblyUnit unit;
        public BaseAssemblyUnit Unit
        {
            get => unit;
            set
            {
                unit = value;
                RaisePropertyChanged();
            }
        }

        public PID SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<string> Consignees
        {
            get => consignees;
            set
            {
                consignees = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<string> Descriptions
        {
            get => descriptions;
            set
            {
                descriptions = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<PIDJournal> Journal
        {
            get => journal;
            set
            {
                journal = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<PIDTCP> Points
        {
            get => points;
            set
            {
                points = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<Inspector> Inspectors
        {
            get => inspectors;
            set
            {
                inspectors = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> JournalNumbers
        {
            get => journalNumbers;
            set
            {
                journalNumbers = value;
                RaisePropertyChanged();
            }
        }

        public PIDTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }
        //public IList<ProductType> ProductTypes
        //{
        //    get => productTypes;
        //    set
        //    {
        //        productTypes = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public IEnumerable<string> Designations
        {
            get => designations;
            set
            {
                designations = value;
                RaisePropertyChanged();
            }
        }

        private PIDJournal operation;
        public PIDJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public Supervision.Commands.IAsyncCommand SaveItemCommand { get; private set; }
        private async Task Save()
        {
            try
            {
                IsBusy = true;
                await Task.Run(() => pIDRepo.Update(SelectedItem));
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand AddOperationCommand { get; private set; }
        public async Task AddJournalOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.PIDJournals.Add(new PIDJournal(SelectedItem, SelectedTCPPoint));
                await SaveItemCommand.ExecuteAsync();
                SelectedTCPPoint = null;
            }
        }

        public ICommand OpenFileStorageCommand { get; private set; }
        private void OpenFile()
        {
            //if (SelectedItem.Specification.FilePath != null)
            //{
            //    Process.Start(SelectedItem.Specification.FilePath);
            //}
            //else MessageBox.Show("Файл не привязан", "Ошибка");
        }

        

        public ICommand EditSelectedAssemblyUnitCommand { get; private set; }
        private void EditSelectedAssemblyUnit()
        {
            if (Unit != null)
            {
                
                if (Unit is SheetGateValve)
                {
                    _ = new SheetGateValveEditView
                    {
                        DataContext = SheetGateValveEditVM.LoadVM(Unit.Id, SelectedItem, db)
                    };
                }
                
            }
        }
        
        public Supervision.Commands.IAsyncCommand RemoveOperationCommand { get; private set; }
        private async Task RemoveOperation()
        {
            try
            {
                IsBusy = true;
                if (Operation != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        SelectedItem.PIDJournals.Remove(Operation);
                        await SaveItemCommand.ExecuteAsync();
                    }
                }
                else MessageBox.Show("Выберите операцию!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }


        public new IAsyncCommand<object> CloseWindowCommand { get; private set; }
        protected new async Task CloseWindow(object obj)
        {
            if (IsBusy)
            {
                MessageBoxResult result = MessageBox.Show("Процесс сохранения уже запущен, теперь все в \"руках\" сервера. Попробовать отправить запрос на сохранение повторно? (Возможен вылет программы и не сохранение результата)", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await SaveItemCommand.ExecuteAsync();
                }
            }
            else
            {
                bool check = true;
                if (SelectedItem.PIDJournals != null)
                {
                    foreach (PIDJournal journal in SelectedItem.PIDJournals)
                    {
                        if (journal.InspectorId != null)
                        {
                            if (journal.Date == null)
                            {
                                check = false;
                                journal.Status = null;
                                MessageBox.Show("Не выбрана дата!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                            if (journal.JournalNumber == null)
                            {
                                check = false;
                                journal.Status = null;
                                MessageBox.Show("Не выбран номер журнала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                        }
                        if (journal.Date != null)
                        {
                            if (journal.RemarkIssued != null && journal.RemarkIssued != "")
                            {
                                if (!Regex.IsMatch(journal.RemarkIssued, @"^\d+$"))
                                {
                                    check = false;
                                    MessageBox.Show("Введите только номер замечания! Без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            if (journal.RemarkClosed != null && journal.RemarkClosed != "")
                            {
                                if (!Regex.IsMatch(journal.RemarkClosed, @"^\d+$"))
                                {
                                    check = false;
                                    MessageBox.Show("Введите только номер замечания! Без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            if (journal.RemarkIssued != null && journal.RemarkIssued != "" && (journal.RemarkClosed == null || journal.RemarkClosed == ""))
                            {
                                journal.Status = "Не соответствует";
                                journal.DateOfRemark = journal.Date;
                                if (journal.Inspector != null)
                                {
                                    journal.RemarkInspector = journal.Inspector.Name;
                                }
                            }
                            else
                            {
                                journal.Status = "Cоответствует";
                            }

                        }
                    }
                }

                if (check)
                {
                    try
                    {
                        IsBusy = true;
                        pIDRepo.SetShippedProductAsync(SelectedItem);
                        int value = await Task.Run(() => pIDRepo.Update(SelectedItem));
                        if (value != 0)
                        {
                            Window w = obj as Window;
                            w?.Close();
                        }
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
            }                
        }

        public Supervision.Commands.IAsyncCommand<int> LoadItemCommand { get; private set; }
        public async Task Load(int id)
        {
            try
            {
                IsBusy = true;
                SelectedItem = await Task.Run(() => pIDRepo.GetByIdIncludeAsync(id));
                Consignees = await Task.Run(() => pIDRepo.GetPropertyValuesDistinctAsync(i => i.Consignee));
                Descriptions = await Task.Run(() => pIDRepo.GetPropertyValuesDistinctAsync(i => i.Description));
                Designations = await Task.Run(() => pIDRepo.GetPropertyValuesDistinctAsync(i => i.Designation));
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Points = await Task.Run(() => pIDRepo.GetTCPsAsync());
                //ProductTypes = await Task.Run(() => productTypeRepo.GetAllAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public static PIDEditVM LoadPIDEditVM(int id, BaseTable entity, DataContext context)
        {
            PIDEditVM vm = new PIDEditVM(entity, context);
            vm.LoadItemCommand.ExecuteAsync(id);
            return vm;
        }

        private bool CanExecute()
        {
            return true;
        }

        public PIDEditVM(BaseTable entity, DataContext context)
        {
            parentEntity = entity;
            db = context;
            pIDRepo = new PIDRepository(db);
            journalRepo = new JournalNumberRepository(db);
            //productTypeRepo = new ProductTypeRepository(db);
            inspectorRepo = new InspectorRepository(db);
            LoadItemCommand = new Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Commands.AsyncCommand(Save);
            CloseWindowCommand = new Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Commands.AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new Commands.AsyncCommand(RemoveOperation);
            //OpenFileCommand = new Command(_ => OpenFile());
            EditSelectedAssemblyUnitCommand = new Command(_ => EditSelectedAssemblyUnit());
            //SelectedItem.AmountShipped = SelectedItem.BaseAssemblyUnits.Where(i => i.Status == "Отгружен").Count();





            //SelectedItem = db.PIDs.Include(i => i.BaseAssemblyUnits).Include(i => i.Specification).ThenInclude(i => i.Customer).SingleOrDefault(i => i.Id == id);
            //Designations = db.PIDs.Select(i => i.Designation).Distinct().OrderBy(i => i).ToList();
            //Journal = db.Set<PIDJournal>().Where(i => i.DetailId == SelectedItem.Id).OrderBy(x => x.PointId).ToList();
            //JournalNumbers = db.JournalNumbers.Where(i => i.IsClosed == false).Select(i => i.Number).Distinct().ToList();
            //ProductTypes = db.ProductTypes.OrderBy(i => i.Name).ToList();
            //Inspectors = db.Inspectors.OrderBy(i => i.Name).ToList();
            //Points = db.PIDTCPs.Include(i => i.OperationType).ToList();
            //Descriptions = db.PIDs.Select(s => s.Description).Distinct().ToList();
        }
    }
}
