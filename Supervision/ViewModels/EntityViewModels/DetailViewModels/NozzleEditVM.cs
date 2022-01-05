using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DataLayer.Entities.Materials;
using Supervision.ViewModels.EntityViewModels.Materials;
using Supervision.Views.EntityViews.MaterialViews;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using BusinessLayer.Repository.Implementations.Entities;
using BusinessLayer.Repository.Implementations.Entities.Material;
using System.Threading.Tasks;
using System.Linq;
using Supervision.Views.EntityViews;
using System.Text.RegularExpressions;
using Supervision.Commands;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels
{
    public class NozzleEditVM : ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IList<MetalMaterial> materials;
        private IList<ForgingMaterial> forgingMaterials;
        private IEnumerable<string> drawings;
        private IEnumerable<string> tensileStrengths;
        private IEnumerable<string> groovings;
        private IEnumerable<NozzleTCP> points;
        private IList<Inspector> inspectors;
        private IEnumerable<NozzleJournal> inputControlJournal;
        private IEnumerable<NozzleJournal> mechanicalJournal;
        private readonly BaseTable parentEntity;
        private NozzleJournal operation;
        private Nozzle selectedItem;
        private NozzleTCP selectedTCPPoint;
        private readonly NozzleRepository repo;
        private readonly InspectorRepository inspectorRepo;
        private readonly MetalMaterialRepository materialRepo;
        private readonly ForgingMaterialRepository forgingRepo;
        private readonly JournalNumberRepository journalRepo;
        private IEnumerable<PID> pIDs;
        private readonly PIDRepository pIDRepo;
        private IEnumerable<Nozzle> nozzles;        


        public IEnumerable<Nozzle> Nozzles
        {
            get => nozzles;
            set
            {
                nozzles = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<PID> PIDs
        {
            get => pIDs;
            set
            {
                pIDs = value;
                RaisePropertyChanged();
            }
        }

        public ICommand EditPIDCommand { get; private set; }
        private void EditPID()
        {
            if (SelectedItem.PID != null)
            {
                _ = new PIDEditView
                {
                    DataContext = PIDEditVM.LoadPIDEditVM(SelectedItem.PID.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("PID не выбран", "Ошибка");
        }


        public Nozzle SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public NozzleJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<NozzleJournal> InputControlJournal
        {
            get => inputControlJournal;
            set
            {
                inputControlJournal = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<NozzleJournal> MechanicalJournal
        {
            get => mechanicalJournal;
            set
            {
                mechanicalJournal = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<NozzleTCP> Points
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
        
        public IList<MetalMaterial> Materials
        {
            get => materials;
            set
            {
                materials = value;
                RaisePropertyChanged();
            }
        }

        public IList<ForgingMaterial> ForgingMaterials
        {
            get => forgingMaterials;
            set
            {
                forgingMaterials = value;
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

        public IEnumerable<string> TensileStrengths
        {
            get => tensileStrengths;
            set
            {
                tensileStrengths = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> Groovings
        {
            get => groovings;
            set
            {
                groovings = value;
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

        public NozzleTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }


        public static NozzleEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            NozzleEditVM vm = new NozzleEditVM(entity, context);
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
                PIDs = await Task.Run(() => pIDRepo.GetAllAsync());
                Nozzles = await Task.Run(() => repo.GetAllAsyncForCompare());
                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));

                await Task.Run(() => materialRepo.Load());
                Materials = materialRepo.SortList();

                await Task.Run(() => forgingRepo.Load());
                ForgingMaterials = forgingRepo.GetByDetail("Катушка");

                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Drawings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                TensileStrengths = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.TensileStrength));
                Groovings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Grooving));
                Points = await Task.Run(() => repo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
                InputControlJournal = SelectedItem.NozzleJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId);
                MechanicalJournal = SelectedItem.NozzleJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId);
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

        public ICommand AddOperationCommand { get; private set; }
        public void AddJournalOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.NozzleJournals.Add(new NozzleJournal(SelectedItem, SelectedTCPPoint));
                InputControlJournal = SelectedItem.NozzleJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                MechanicalJournal = SelectedItem.NozzleJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
                SelectedTCPPoint = null;
            }
        }

        public ICommand RemoveOperationCommand { get; private set; }
        private void RemoveOperation()
        {
            try
            {
                IsBusy = true;
                if (Operation != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        SelectedItem.NozzleJournals.Remove(Operation);
                        InputControlJournal = SelectedItem.NozzleJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                        MechanicalJournal = SelectedItem.NozzleJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
                        Operation = null;
                    }
                }
                else MessageBox.Show("Выберите операцию!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }

        }

        public ICommand EditForgingMaterialCommand { get; private set; }
        private void EditForgingMaterial()
        {
            if (SelectedItem.ForgingMaterial != null)
            {

                _ = new ForgingMaterialEditView
                {
                    DataContext = ForgingMaterialEditVM.LoadVM(SelectedItem.ForgingMaterial.Id, SelectedItem, db)
                };
            }
        }

        public ICommand EditMaterialCommand { get; private set; }
        private void EditMaterial()
        {
            if (SelectedItem.MetalMaterial != null)
            {                

                if (SelectedItem.MetalMaterial is SheetMaterial)
                {
                    _ = new SheetMaterialEditView
                    {
                        DataContext = SheetMaterialEditVM.LoadSheetMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
                    };
                }                
                else if (SelectedItem.MetalMaterial is RolledMaterial)
                {
                    _ = new RolledMaterialEditView
                    {
                        DataContext = RolledMaterialEditVM.LoadRolledMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
                    };
                }
            }
            else MessageBox.Show("Для просмотра привяжите материал", "Ошибка");
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
                bool assemblyFlag = false;

                if (SelectedItem.ForgingMaterialId != null && SelectedItem.MetalMaterialId != null)
                {
                    MessageBox.Show("Невозможно одновременно привязать два материала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    SelectedItem.ForgingMaterial = null;
                    SelectedItem.MetalMaterial = null;
                    check = false;
                }
                else if (SelectedItem.ForgingMaterialId != null)
                {
                    if (await Task.Run(() => forgingRepo.IsAssembliedAsync(SelectedItem)))
                    {
                        SelectedItem.ForgingMaterial = null;
                        check = false;
                    }
                    else
                    {
                        SelectedItem.Material = SelectedItem.ForgingMaterial.Material;
                        SelectedItem.Melt = SelectedItem.ForgingMaterial.Melt;
                        SelectedItem.Certificate = SelectedItem.ForgingMaterial.Certificate;
                        if (SelectedItem.ForgingMaterial.Status == "НЕ СООТВ.")
                        {
                            SelectedItem.Status = "НЕ СООТВ.";
                            flag = false;
                            MessageBox.Show("Выбранная поковка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей катушке.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else if (SelectedItem.MetalMaterialId != null)
                {
                    SelectedItem.Material = SelectedItem.MetalMaterial.Material;
                    SelectedItem.Melt = SelectedItem.MetalMaterial.Melt;
                    SelectedItem.Certificate = SelectedItem.MetalMaterial.Certificate;
                    if (SelectedItem.MetalMaterial.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранный прокат имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей катушке.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                if (SelectedItem.Number == null || SelectedItem.Number == "")
                {
                    MessageBox.Show("Не оставляйте номер пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if (!SelectedItem.Number.ToLower().Contains("удалить"))
                {
                    if (!string.IsNullOrEmpty(SelectedItem.Number) && !string.IsNullOrEmpty(SelectedItem.ZK))
                    {
                        int count = 0;
                        foreach (Nozzle entity in Nozzles)
                        {
                            if (SelectedItem.Number.Equals(entity.Number) && SelectedItem.ZK.Equals(entity.ZK))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Катушка с таким ЗК уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }
                    else if (SelectedItem.Number != null && SelectedItem.Melt != null && SelectedItem.Certificate != null && SelectedItem.Drawing != null)
                    {
                        int count = 0;
                        foreach (Nozzle entity in Nozzles)
                        {
                            if (SelectedItem.Number.Equals(entity.Number) && SelectedItem.Melt.Equals(entity.Melt) && SelectedItem.Certificate.Equals(entity.Certificate) && SelectedItem.Drawing.Equals(entity.Drawing))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Катушка с таким сертификатными данными уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show("Не подтянута плавка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                }

                if (SelectedItem.NozzleJournals != null)
                {
                    foreach (NozzleJournal journal in SelectedItem.NozzleJournals)
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
                            if (journal.PointId == 95 && journal.InspectorId != null)
                            {
                                if (SelectedItem.PN == null || SelectedItem.PN == "")
                                {
                                    MessageBox.Show("Не выбрано давление!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    check = false;
                                }
                                if (SelectedItem.TensileStrength == null || SelectedItem.TensileStrength == "")
                                {
                                    MessageBox.Show("Не введен КП!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    check = false;
                                }
                                if (SelectedItem.Grooving == null || SelectedItem.Grooving == "")
                                {
                                    MessageBox.Show("Не введена разделка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    check = false;
                                }
                            }
                            if (journal.PointId == 95 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag = true;
                            }
                        }
                    }
                }

                if (flag)
                {
                    SelectedItem.Status = "Cоотв.";
                    if (assemblyFlag) SelectedItem.Status = "Готово к сборке";
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

        public NozzleEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            pIDRepo = new PIDRepository(db);
            EditPIDCommand = new Supervision.Commands.Command(o => EditPID());
            repo = new NozzleRepository(db);
            inspectorRepo = new InspectorRepository(db);
            materialRepo = new MetalMaterialRepository(db);
            forgingRepo = new ForgingMaterialRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand(SaveItem);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.Command(o => AddJournalOperation());
            RemoveOperationCommand = new Supervision.Commands.Command(o => RemoveOperation());
            EditMaterialCommand = new Supervision.Commands.Command(o => EditMaterial());
            EditForgingMaterialCommand = new Supervision.Commands.Command(o => EditForgingMaterial());
        }
    }
}
