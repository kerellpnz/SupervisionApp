using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using BusinessLayer.Repository.Implementations.Entities;
using DataLayer;
using DataLayer.Journals.Materials;
using DataLayer.TechnicalControlPlans.Materials;
using Supervision.Commands;

namespace Supervision.ViewModels.EntityViewModels.Materials
{
    public class StoresControlVM : ViewModelBase
    {
        private readonly DataContext db;
        private readonly StoreControlRepository repo;
        private readonly JournalNumberRepository journalRepo;
        private readonly InspectorRepository inspectorRepo;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<StoresControlTCP> points;
        private IEnumerable<Inspector> inspectors;
        private IList<StoresControlJournal> journal;
        private StoresControlTCP selectedTCPPoint;
        private DateTime lastInspection;
        private DateTime nextInspection;
        private StoresControlJournal operation;

        public StoresControlJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public IList<StoresControlJournal> Journal
        {
            get => journal;
            set
            {
                journal = value;
                RaisePropertyChanged();
            }
        }
        public DateTime LastInspection
        {
            get => lastInspection;
            set
            {
                lastInspection = value;
                RaisePropertyChanged();
            }
        }
        public DateTime NextInspection
        {
            get => nextInspection;
            set
            {
                nextInspection = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<StoresControlTCP> Points
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

        public StoresControlTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }

        public IAsyncCommand LoadItemsCommand { get; private set; }
        public async Task Load()
        {
            try
            {
                IsBusy = true;
                Journal = await Task.Run(() => repo.GetAllAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Points = await Task.Run(() => repo.GetTCPsAsync());
                if (Journal != null)
                {
                    LastInspection = await Task.Run(() => repo.GetLastDateControl());
                    NextInspection = LastInspection.AddMonths(1);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand SaveItemCommand { get; private set; }
        private async Task SaveItem()
        {
            try
            {
                IsBusy = true;
                await Task.Run(() => repo.Update(Journal));                
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand AddOperationCommand { get; private set; }
        public async Task AddJournalOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                await repo.AddAsync(new StoresControlJournal(SelectedTCPPoint));
                SelectedTCPPoint = null;
            }
        }

        public IAsyncCommand RemoveOperationCommand { get; private set; }
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
                        await repo.RemoveAsync(Operation);
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
                if (Journal != null)
                {
                    LastInspection = await Task.Run(() => repo.GetLastDateControl());
                    NextInspection = LastInspection.AddMonths(1);
                    foreach (StoresControlJournal journal in Journal)
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
                        int value = await Task.Run(() => repo.Update(Journal));
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

        public static StoresControlVM LoadVM(DataContext context)
        {
            StoresControlVM vm = new StoresControlVM(context);
            vm.LoadItemsCommand.ExecuteAsync();
            return vm;
        }

        private bool CanExecute()
        {
            return true;
        }


        public StoresControlVM(DataContext context)
        {
            db = context;
            repo = new StoreControlRepository(db);
            journalRepo = new JournalNumberRepository(db);
            inspectorRepo = new InspectorRepository(db);
            LoadItemsCommand = new AsyncCommand(Load);
            SaveItemCommand = new AsyncCommand(SaveItem);
            CloseWindowCommand = new AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new AsyncCommand(RemoveOperation);
        }
    }
}
