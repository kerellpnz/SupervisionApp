using DataLayer;
using System.Collections.Generic;
using System.Windows;
using DataLayer.TechnicalControlPlans.Materials;
using DataLayer.Journals.Materials;
using DataLayer.Entities.Materials;
using BusinessLayer.Repository.Implementations.Entities.Material;
using BusinessLayer.Repository.Implementations.Entities;
using System.Threading.Tasks;
using Supervision.Commands;
using System.Text.RegularExpressions;

namespace Supervision.ViewModels.EntityViewModels.Materials
{
    public class ForgingMaterialEditVM : ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<string> materials;
        private IEnumerable<string> firstSizes;
        private IEnumerable<string> drawings;
        //private IEnumerable<string> thirdSizes;
        //private IEnumerable<string> targets;
        private IEnumerable<ForgingMaterialTCP> points;
        private IEnumerable<Inspector> inspectors;
        //private IEnumerable<ForgingMaterialJournal> journal;
        //private readonly BaseTable parentEntity;
        private readonly ForgingMaterialRepository repo;
        private readonly InspectorRepository inspectorRepo;
        private readonly JournalNumberRepository journalRepo;
        private ForgingMaterialTCP selectedTCPPoint;
        private ForgingMaterial selectedItem;
        private ForgingMaterialJournal operation;
        private IEnumerable<ForgingMaterial> forgingMaterial;

        public IEnumerable<string> Drawings
        {
            get => drawings;
            set
            {
                drawings = value;
                RaisePropertyChanged();
            }
        }


        public IEnumerable<ForgingMaterial> ForgingMaterials
        {
            get => forgingMaterial;
            set
            {
                forgingMaterial = value;
                RaisePropertyChanged();
            }
        }

        public ForgingMaterial SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public ForgingMaterialJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<ForgingMaterialTCP> Points
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

        public IEnumerable<string> FirstSizes
        {
            get => firstSizes;
            set
            {
                firstSizes = value;
                RaisePropertyChanged();
            }
        }

        //public IEnumerable<string> Targets
        //{
        //    get => targets;
        //    set
        //    {
        //        targets = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public IEnumerable<string> JournalNumbers
        {
            get => journalNumbers;
            set
            {
                journalNumbers = value;
                RaisePropertyChanged();
            }
        }

        public ForgingMaterialTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }


        public static ForgingMaterialEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            ForgingMaterialEditVM vm = new ForgingMaterialEditVM(entity, context);
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
                ForgingMaterials = await Task.Run(() => repo.GetAllAsync());
                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));
                Materials = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Material));
                FirstSizes = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.FirstSize));
                Drawings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());                
                Points = await Task.Run(() => repo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand SaveItemCommand { get; private set; }
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

        public Supervision.Commands.IAsyncCommand AddOperationCommand { get; private set; }
        public async Task AddJournalOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.ForgingMaterialJournals.Add(new ForgingMaterialJournal(SelectedItem, SelectedTCPPoint));
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
                        SelectedItem.ForgingMaterialJournals.Remove(Operation);
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

                if (SelectedItem.Number == null || SelectedItem.Number == "")
                {
                    MessageBox.Show("Не оставляйте номер пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if (!SelectedItem.Number.ToLower().Contains("удалить"))
                {
                    if (SelectedItem.Certificate != null && SelectedItem.Number != null && SelectedItem.Drawing != null)
                    {
                        int count = 0;
                        foreach (ForgingMaterial entity in ForgingMaterials)
                        {
                            if (SelectedItem.Melt.Equals(entity.Melt) && SelectedItem.Certificate.Equals(entity.Certificate) && SelectedItem.Number.Equals(entity.Number) && SelectedItem.Drawing.Equals(entity.Drawing))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Поковка/отливка под такими сертификатными данными уже существует! Данные сохранены не будут, в поле \"Номер\" напишите \"удалить\"", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }
                    if (SelectedItem.Target == null || SelectedItem.Target == "")
                    {
                        MessageBox.Show("Выберите назначение поковки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Melt == null || SelectedItem.Melt == "")
                    {
                        MessageBox.Show("Не введена плавка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Material == null || SelectedItem.Material == "")
                    {
                        MessageBox.Show("Не введен материал!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Drawing == null || SelectedItem.Drawing == "")
                    {
                        MessageBox.Show("Не введен чертеж! Если этих данных нет в сертификате, ставьте \"-\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Certificate == null || SelectedItem.Certificate == "")
                    {
                        MessageBox.Show("Не введен сертификат!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                }

                if (SelectedItem.ForgingMaterialJournals != null)
                {
                    bool flag = true;
                    foreach (ForgingMaterialJournal journal in SelectedItem.ForgingMaterialJournals)
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

                    if (flag)
                    {
                        SelectedItem.Status = "Cоотв.";
                    }
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

        public ForgingMaterialEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            //parentEntity = entity;
            repo = new ForgingMaterialRepository(db);
            inspectorRepo = new InspectorRepository(db);
            //materialRepo = new MetalMaterialRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand(SaveItem);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new Supervision.Commands.AsyncCommand(RemoveOperation);
            //EditMaterialCommand = new Supervision.Commands.Command(o => EditMaterial());
        }


        //public ForgingMaterialJournal Operation
        //{
        //    get => operation;
        //    set
        //    {
        //        operation = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public ForgingMaterial SelectedItem
        //{
        //    get => selectedItem;
        //    set
        //    {
        //        selectedItem = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public IEnumerable<ForgingMaterialJournal> Journal
        //{
        //    get => journal;
        //    set
        //    {
        //        journal = value;
        //        RaisePropertyChanged();
        //    }
        //}
        //public IEnumerable<MetalMaterialTCP> Points
        //{
        //    get => points;
        //    set
        //    {
        //        points = value;
        //        RaisePropertyChanged();
        //    }
        //}
        //public IEnumerable<Inspector> Inspectors
        //{
        //    get => inspectors;
        //    set
        //    {
        //        inspectors = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public IEnumerable<string> Materials
        //{
        //    get => materials;
        //    set
        //    {
        //        materials = value;
        //        RaisePropertyChanged();
        //    }
        //}
        //public IEnumerable<string> JournalNumbers
        //{
        //    get => journalNumbers;
        //    set
        //    {
        //        journalNumbers = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public IEnumerable<string> FirstSizes
        //{
        //    get => firstSizes;
        //    set
        //    {
        //        firstSizes = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public IEnumerable<string> SecondSizes
        //{
        //    get => secondSizes;
        //    set
        //    {
        //        secondSizes = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public IEnumerable<string> ThirdSizes
        //{
        //    get => thirdSizes;
        //    set
        //    {
        //        thirdSizes = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public MetalMaterialTCP SelectedTCPPoint
        //{
        //    get => selectedTCPPoint;
        //    set
        //    {
        //        selectedTCPPoint = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public IAsyncCommand<int> LoadItemCommand { get; private set; }
        //public async Task Load(int id)
        //{
        //    try
        //    {
        //        IsBusy = true;
        //        SelectedItem = await Task.Run(() => materialRepo.GetByIdIncludeAsync(id));
        //        Materials = await Task.Run(() => materialRepo.GetPropertyValuesDistinctAsync(i => i.Material));
        //        Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
        //        Points = await Task.Run(() => materialRepo.GetTCPsAsync());
        //        JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
        //        FirstSizes = await Task.Run(() => materialRepo.GetPropertyValuesDistinctAsync(i => i.FirstSize));
        //        SecondSizes = await Task.Run(() => materialRepo.GetPropertyValuesDistinctAsync(i => i.SecondSize));
        //        ThirdSizes = await Task.Run(() => materialRepo.GetPropertyValuesDistinctAsync(i => i.ThirdSize));
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}

        //public IAsyncCommand SaveItemCommand { get; private set; }
        //private async Task SaveItem()
        //{
        //    try
        //    {
        //        bool flag = true;
        //        foreach (ForgingMaterialJournal journal in SelectedItem.ForgingMaterialJournals)
        //        {
        //            if (journal.Status != null)
        //            {
        //                if (journal.Date == null)
        //                {
        //                    MessageBox.Show("Не выбрана дата", "Ошибка");
        //                }
        //                if (journal.Status == "Не соответствует")
        //                {
        //                    SelectedItem.Status = "НЕ СООТВ.";
        //                    flag = false;
        //                }
        //            }
        //        }

        //        if (flag)
        //        {
        //            SelectedItem.Status = "Cоотв.";
        //        }

        //        IsBusy = true;                
        //        await Task.Run(() => materialRepo.Update(SelectedItem));                
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}

        //public IAsyncCommand AddOperationCommand { get; private set; }
        //public async Task AddJournalOperation()
        //{
        //    if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
        //    else
        //    {
        //        SelectedItem.ForgingMaterialJournals.Add(new ForgingMaterialJournal(SelectedItem, SelectedTCPPoint));
        //        await SaveItemCommand.ExecuteAsync();
        //        SelectedTCPPoint = null;
        //    }
        //}

        //public IAsyncCommand RemoveOperationCommand { get; private set; }
        //private async Task RemoveOperation()
        //{
        //    try
        //    {
        //        IsBusy = true;
        //        if (Operation != null)
        //        {
        //            MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
        //            if (result == MessageBoxResult.Yes)
        //            {
        //                SelectedItem.ForgingMaterialJournals.Remove(Operation);
        //                await SaveItemCommand.ExecuteAsync();
        //            }
        //        }
        //        else MessageBox.Show("Выберите операцию!", "Ошибка");
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }

        //}

        //protected override void CloseWindow(object obj)
        //{
        //    if (materialRepo.HasChanges(SelectedItem) || materialRepo.HasChanges(SelectedItem.ForgingMaterialJournals))
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

        //public static ForgingMaterialEditVM LoadForgingMaterialEditVM(int id, BaseTable entity, DataContext context)
        //{
        //    ForgingMaterialEditVM vm = new ForgingMaterialEditVM(entity, context);
        //    vm.LoadItemCommand.ExecuteAsync(id);
        //    return vm;
        //}

        //private bool CanExecute()
        //{
        //    return true;
        //}

        //public ForgingMaterialEditVM(BaseTable entity, DataContext context)
        //{
        //    db = context;
        //    parentEntity = entity;
        //    materialRepo = new ForgingMaterialRepository(db);
        //    inspectorRepo = new InspectorRepository(db);
        //    journalRepo = new JournalNumberRepository(db);
        //    LoadItemCommand = new AsyncCommand<int>(Load);
        //    SaveItemCommand = new AsyncCommand(SaveItem);
        //    CloseWindowCommand = new Command(o => CloseWindow(o));
        //    AddOperationCommand = new AsyncCommand(AddJournalOperation);
        //    RemoveOperationCommand = new AsyncCommand(RemoveOperation);
        //}
    }
}
