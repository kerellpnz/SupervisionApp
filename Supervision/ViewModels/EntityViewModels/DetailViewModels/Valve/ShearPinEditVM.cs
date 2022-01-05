using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using System.Collections.Generic;
using System.Windows;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using BusinessLayer.Repository.Implementations.Entities;
using System.Threading.Tasks;
using Supervision.Commands;
using DataLayer.Entities.AssemblyUnits;
using System.Text.RegularExpressions;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels
{
    public class ShearPinEditVM : ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<string> drawings;
        private IEnumerable<ShearPinTCP> points;
        private IList<Inspector> inspectors;
        private readonly BaseTable parentEntity;
        private ShearPinJournal operation;
        private ShearPin selectedItem;
        private ShearPinTCP selectedTCPPoint;
        private readonly ShearPinRepository repo;
        private readonly InspectorRepository inspectorRepo;
        private readonly JournalNumberRepository journalRepo;
        private IEnumerable<string> materials;
        private IEnumerable<string> diameters;
        private IEnumerable<string> tensileStrengths;
        private IEnumerable<ShearPin> shearPins;

        public IEnumerable<ShearPin> ShearPins
        {
            get => shearPins;
            set
            {
                shearPins = value;
                RaisePropertyChanged();
            }
        }

        public ShearPin SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public ShearPinJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<ShearPinTCP> Points
        {
            get => points;
            set
            {
                points = value;
                RaisePropertyChanged();
            }
        }
        public IList<Inspector> Inspectors
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

        public IEnumerable<string> Diameters
        {
            get => diameters;
            set
            {
                diameters = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> TensileStrengths
        {
            get => tensileStrengths;
            set
            {
                tensileStrengths = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> Drawings
        {
            get => drawings;
            set
            {
                drawings = value;
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

        public ShearPinTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }


        public static ShearPinEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            ShearPinEditVM vm = new ShearPinEditVM(entity, context);
            vm.LoadItemCommand.ExecuteAsync(id);
            return vm;
        }

        private bool CanExecute()
        {
            return true;
        }

        public Commands.IAsyncCommand<int> LoadItemCommand { get; private set; }
        public async Task Load(int id)
        {
            try
            {
                IsBusy = true;
                ShearPins = await Task.Run(() => repo.GetAllAsync());
                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Drawings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Materials = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Material));
                TensileStrengths = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.TensileStrength));
                Diameters = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Diameter));
                Points = await Task.Run(() => repo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand SaveItemCommand { get; private set; }
        private async Task Save()
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

        public Supervision.Commands.IAsyncCommand AddOperationCommand { get; private set; }
        public async Task AddJournalOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.ShearPinJournals.Add(new ShearPinJournal(SelectedItem, SelectedTCPPoint));
                await SaveItemCommand.ExecuteAsync();
                SelectedTCPPoint = null;
            }
        }

        public Commands.IAsyncCommand RemoveOperationCommand { get; private set; }
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
                        SelectedItem.ShearPinJournals.Remove(Operation);
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

                if (SelectedItem.Number == null || SelectedItem.Number == "")
                {
                    MessageBox.Show("Не оставляйте ЗК пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if (!SelectedItem.Number.ToLower().Contains("удалить"))
                {
                    if (SelectedItem.Number != null && SelectedItem.Pull != null)
                    {
                        int count = 0;
                        foreach (ShearPin entity in ShearPins)
                        {
                            if (SelectedItem.Number.Equals(entity.Number) && SelectedItem.Pull.Equals(entity.Pull))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Штифты под таким ЗК и тягой уже существуют! Действия сохранены не будут! В поле \"ЗК\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }
                    if (SelectedItem.DN == null || SelectedItem.DN == "")
                    {
                        MessageBox.Show("Не выбран диаметр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Melt == null || SelectedItem.Melt == "")
                    {
                        MessageBox.Show("Не введена плавка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Drawing == null || SelectedItem.Drawing == "")
                    {
                        MessageBox.Show("Не введен чертеж!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Material == null || SelectedItem.Material == "")
                    {
                        MessageBox.Show("Не введен материал!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Certificate == null || SelectedItem.Certificate == "")
                    {
                        MessageBox.Show("Не введен сертификат!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Amount == 0)
                    {
                        MessageBox.Show("Не введено количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Diameter == null || SelectedItem.Diameter == "")
                    {
                        MessageBox.Show("Не введен диаметр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.TensileStrength == null || SelectedItem.TensileStrength == "")
                    {
                        MessageBox.Show("Не введен предел прочности!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Pull == null || SelectedItem.Pull == "")
                    {
                        MessageBox.Show("Не введена тяга!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                }


                if (SelectedItem.ShearPinJournals != null)
                {
                    foreach (ShearPinJournal journal in SelectedItem.ShearPinJournals)
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
                                SelectedItem.Status = "НЕ СООТВ.";
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

                        if (SelectedItem != null)
                        {
                            if (SelectedItem.AmountRemaining == null && SelectedItem.Amount > 0)
                                SelectedItem.AmountRemaining = SelectedItem.Amount;
                            else
                            {
                                //SelectedItem.AmountRemaining = await repo.GetAmountRemaining(SelectedItem);
                                int amountUsed = 0;
                                foreach (BaseValveWithShearPin i in SelectedItem.BaseValveWithShearPins)
                                {
                                    amountUsed += i.ShearPinAmount;
                                }
                                SelectedItem.AmountRemaining = SelectedItem.Amount - amountUsed;
                            }
                        }

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

        public ShearPinEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            repo = new ShearPinRepository(db);
            inspectorRepo = new InspectorRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand(Save);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new Supervision.Commands.AsyncCommand(RemoveOperation);
        }
    }
}
