﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using BusinessLayer.Repository.Implementations.Entities;
using BusinessLayer.Repository.Implementations.Entities.Material;
using DataLayer;
using DataLayer.Entities.Materials;
using DataLayer.Journals.Materials;
using DataLayer.TechnicalControlPlans.Materials;
using Supervision.Commands;

namespace Supervision.ViewModels.EntityViewModels.Materials
{
    public class ControlWeldEditVM : ViewModelBase
    {
        private readonly DataContext db;
        private readonly ControlWeldRepository repo;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<string> materials;
        private IEnumerable<string> weldingMethods;
        private IEnumerable<string> welders;
        private IEnumerable<string> stamps;
        private IEnumerable<ControlWeldTCP> points;
        private IEnumerable<Inspector> inspectors;
        private IEnumerable<ControlWeldJournal> journal;
        private readonly BaseTable parentEntity;
        private ControlWeld selectedItem;
        private ControlWeldTCP selectedTCPPoint;
        private ControlWeldJournal operation;

        public ControlWeldJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public ControlWeld SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<ControlWeldJournal> Journal
        {
            get => journal;
            set
            {
                journal = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<ControlWeldTCP> Points
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
 
        public IEnumerable<string> Materials
        {
            get => materials;
            set
            {
                materials = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<string> WeldingMethods
        {
            get => weldingMethods;
            set
            {
                weldingMethods = value;
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
        public IEnumerable<string> Welders
        {
            get => welders;
            set
            {
                welders = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<string> Stamps
        {
            get => stamps;
            set
            {
                stamps = value;
                RaisePropertyChanged();
            }
        }

        public ControlWeldTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }

        private readonly InspectorRepository inspectorRepo;
        private readonly JournalNumberRepository journalRepo;

        public IAsyncCommand<int> LoadItemCommand { get; private set; }
        public async Task Load(int id)
        {
            try
            {
                IsBusy = true;
                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));
                Materials = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.FirstMaterial));
                WeldingMethods = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.WeldingMethod));
                Welders = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Welder));
                Stamps = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Stamp));
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Points = await Task.Run(() => repo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
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
                await Task.Run(() => repo.Update(SelectedItem));
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
                SelectedItem.ControlWeldJournals.Add(new ControlWeldJournal(SelectedItem, SelectedTCPPoint));
                await SaveItemCommand.ExecuteAsync();
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
                        SelectedItem.ControlWeldJournals.Remove(Operation);
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
                bool flag = true;
                if (SelectedItem.ControlWeldJournals != null)
                {
                    foreach (ControlWeldJournal journal in SelectedItem.ControlWeldJournals)
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
                                SelectedItem.Status = "Не соотв.";
                                flag = false;
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


                if (flag)
                {
                    SelectedItem.Status = "Cоотв.";
                }

                if (check)
                {
                    try
                    {
                        IsBusy = true;
                        int value = await Task.Run(() => repo.Update(SelectedItem));
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

        public static ControlWeldEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            ControlWeldEditVM vm = new ControlWeldEditVM(entity, context);
            vm.LoadItemCommand.ExecuteAsync(id);
            return vm;
        }

        private bool CanExecute()
        {
            return true;
        }

        public ControlWeldEditVM(BaseTable entity, DataContext context)
        {
            parentEntity = entity;
            db = context;
            repo = new ControlWeldRepository(db);
            inspectorRepo = new InspectorRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new AsyncCommand<int>(Load);
            SaveItemCommand = new AsyncCommand(SaveItem);
            CloseWindowCommand = new AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new AsyncCommand(RemoveOperation);
        }
    }
}
