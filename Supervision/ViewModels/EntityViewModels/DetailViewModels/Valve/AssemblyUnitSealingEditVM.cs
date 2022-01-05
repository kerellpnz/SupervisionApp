using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using Supervision.Commands;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve
{
    public class AssemblyUnitSealingEditVM : BasePropertyChanged
    {
        private readonly DataContext db;
        private readonly AssemblyUnitSealingRepository sealRepo;
        private readonly InspectorRepository inspectorRepo;
        private readonly JournalNumberRepository journalRepo;

        private IEnumerable<string> journalNumbers;
        private IEnumerable<string> materials;
        private IEnumerable<string> drawings;
        private IEnumerable<AssemblyUnitSealingTCP> points;
        private IEnumerable<Inspector> inspectors;
        private readonly BaseTable parentEntity;
        private AssemblyUnitSealing selectedItem;
        private AssemblyUnitSealingTCP selectedTCPPoint;
        private IEnumerable<string> names;
        private IEnumerable<AssemblyUnitSealing> assemblyUnitSeals;

        public IEnumerable<AssemblyUnitSealing> AssemblyUnitSeals
        {
            get => assemblyUnitSeals;
            set
            {
                assemblyUnitSeals = value;
                RaisePropertyChanged();
            }
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            protected set
            {
                isBusy = value;
                RaisePropertyChanged();
            }
        }

        private AssemblyUnitSealingJournal operation;
        public AssemblyUnitSealingJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }
        public AssemblyUnitSealing SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<AssemblyUnitSealingTCP> Points
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

        public IEnumerable<string> Names
        {
            get => names;
            set
            {
                names = value;
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

        public AssemblyUnitSealingTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }



        public static AssemblyUnitSealingEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            AssemblyUnitSealingEditVM vm = new AssemblyUnitSealingEditVM(entity, context);
            vm.LoadItemCommand.ExecuteAsync(id);
            return vm;
        }

        public IAsyncCommand<int> LoadItemCommand { get; private set; }
        public async Task Load(int id)
        {
            try
            {
                IsBusy = true;
                AssemblyUnitSeals = await Task.Run(() => sealRepo.GetAllAsync());
                SelectedItem = await Task.Run(() => sealRepo.GetByIdIncludeAsync(id));
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Materials = await Task.Run(() => sealRepo.GetPropertyValuesDistinctAsync(i => i.Material));
                Drawings = await Task.Run(() => sealRepo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Names = await Task.Run(() => sealRepo.GetPropertyValuesDistinctAsync(i => i.Name));
                Points = await Task.Run(() => sealRepo.GetTCPsAsync());
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
                await Task.Run(() => sealRepo.Update(SelectedItem));
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand<object> CloseWindowCommand { get; private set; }
        protected async Task CloseWindow(object obj)
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

                if (SelectedItem.Certificate == null || SelectedItem.Certificate == "")
                {
                    MessageBox.Show("Не оставляйте сертификат пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if (!SelectedItem.Certificate.ToLower().Contains("удалить"))
                {
                    if (SelectedItem.Certificate != null && SelectedItem.Batch != null && SelectedItem.Name != null)
                    {
                        int count = 0;
                        foreach (AssemblyUnitSealing entity in AssemblyUnitSeals)
                        {
                            if (SelectedItem.Certificate.Equals(entity.Certificate) && SelectedItem.Batch.Equals(entity.Batch) && SelectedItem.Name.Equals(entity.Name))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Уплотнения с такими сертификатными данными уже существуют! Действия сохранены не будут! В поле \"Сертификат\" напишите \"удалить\". Добавьте принятое количество к имеющимся уплотнениям. Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }
                    if (SelectedItem.Batch == null || SelectedItem.Batch == "")
                    {
                        MessageBox.Show("Не введена партия! Если этих данных нет в сертификате, ставьте \"-\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Name == null || SelectedItem.Name == "")
                    {
                        MessageBox.Show("Не введено наименование уплотнения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Amount == 0)
                    {
                        MessageBox.Show("Не введено количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                }

                if (SelectedItem.AssemblyUnitSealingJournals != null)
                {
                    foreach (AssemblyUnitSealingJournal journal in SelectedItem.AssemblyUnitSealingJournals)
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
                        if (SelectedItem.Amount > 0)
                            SelectedItem.AmountRemaining = SelectedItem.Amount - SelectedItem.BaseValveSCoverWithSeals.Count;
                        int value = await Task.Run(() => sealRepo.Update(SelectedItem));
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

        public IAsyncCommand AddOperationCommand { get; private set; }
        public async Task AddJournalOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.AssemblyUnitSealingJournals.Add(new AssemblyUnitSealingJournal()
                {
                    DetailDrawing = SelectedItem.Drawing,
                    DetailNumber = SelectedItem.Number,
                    DetailName = SelectedItem.Name,
                    DetailId = SelectedItem.Id,
                    Point = SelectedTCPPoint.Point,
                    Description = SelectedTCPPoint.Description,
                    PointId = SelectedTCPPoint.Id,
                });
                await SaveItemCommand.ExecuteAsync();
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
                        SelectedItem.AssemblyUnitSealingJournals.Remove(Operation);
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
        

        //public ICommand CloseWindowCommand { get; private set; }
        //private void CloseWindow(object obj)
        //{
        //    if (sealRepo.HasChanges(SelectedItem) || sealRepo.HasChanges(SelectedItem.AssemblyUnitSealingJournals))
        //    {
        //        MessageBoxResult result = MessageBox.Show("Закрыть без сохранения изменений?", "Выход", MessageBoxButton.YesNo);

        //        if (result == MessageBoxResult.Yes)
        //        {
        //            Window w = obj as Window;
        //            w?.Close();
        //        }
        //    }
        //    else
        //    {
        //        Window w = obj as Window;
        //        w?.Close();
        //    }
        //}

        private bool CanExecute()
        {
            return true;
        }

        public AssemblyUnitSealingEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            sealRepo = new AssemblyUnitSealingRepository(context);
            inspectorRepo = new InspectorRepository(context);
            journalRepo = new JournalNumberRepository(context);
            LoadItemCommand = new AsyncCommand<int>(Load);
            SaveItemCommand = new AsyncCommand(Save);
            CloseWindowCommand = new AsyncCommand<object>(CloseWindow);
            //CloseWindowCommand = new Command(o => CloseWindow(o));
            AddOperationCommand = new AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new AsyncCommand(RemoveOperation);
        }
    }
}
