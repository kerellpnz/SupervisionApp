using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using BusinessLayer.Repository.Implementations.Entities.Material;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Entities.Materials;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using Supervision.Commands;
using Supervision.ViewModels.EntityViewModels.Materials;
using Supervision.ViewModels.EntityViewModels.Periodical.Gate;
using Supervision.Views.EntityViews;
using Supervision.Views.EntityViews.MaterialViews;
using Supervision.Views.EntityViews.PeriodicalControl.Gate;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve
{
    public class GateEditVM: ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<MetalMaterial> materials;
        private IEnumerable<string> drawings;
        private IEnumerable<PID> pIDs;
        private GateJournal operation;
        private IEnumerable<Gate> gates;

        public IEnumerable<Gate> Gates
        {
            get => gates;
            set
            {
                gates = value;
                RaisePropertyChanged();
            }
        }

        public GateJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
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
        private IEnumerable<GateTCP> points;
        private IEnumerable<Inspector> inspectors;
        //private IList<GateJournal> inputControlJournal;
        //private IList<GateJournal> preparationJournal;
        private IList<GateJournal> coatingJournal;
        //private IList<GateJournal> testJournal;
        private IList<GateJournal> documentationJournal;
        //private IList<GateJournal> shippedJournal;
        private readonly BaseTable parentEntity;
        private readonly GateRepository gateRepo;
        private readonly InspectorRepository inspectorRepo;
        private readonly MetalMaterialRepository materialRepo;
        private readonly PIDRepository pIDRepo;
        private readonly JournalNumberRepository journalRepo;
        private Gate selectedItem;
        private GateTCP selectedTCPPoint;

        public Gate SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        //public IList<GateJournal> InputControlJournal
        //{
        //    get => inputControlJournal;
        //    set
        //    {
        //        inputControlJournal = value;
        //        RaisePropertyChanged();
        //    }
        //}
       
        //public IList<GateJournal> PreparationJournal
        //{
        //    get => preparationJournal;
        //    set
        //    {
        //        preparationJournal = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public IList<GateJournal> CoatingJournal
        {
            get => coatingJournal;
            set
            {
                coatingJournal = value;
                RaisePropertyChanged();
            }
        }
        //public IList<GateJournal> TestJournal
        //{
        //    get => testJournal;
        //    set
        //    {
        //        testJournal = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public IList<GateJournal> DocumentationJournal
        {
            get => documentationJournal;
            set
            {
                documentationJournal = value;
                RaisePropertyChanged();
            }
        }
        //public IList<GateJournal> ShippedJournal
        //{
        //    get => shippedJournal;
        //    set
        //    {
        //        shippedJournal = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public IEnumerable<GateTCP> Points
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

        public IEnumerable<MetalMaterial> Materials
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

        public GateTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }


        public static GateEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            GateEditVM vm = new GateEditVM(entity, context);
            vm.LoadItemCommand.ExecuteAsync(id);
            return vm;
        }

        private bool CanExecute()
        {
            return true;
        }

        public ICommand DegreasingChemicalCompositionOpenCommand { get; private set; }
        private void DegreasingChemicalCompositionOpen()
        {
            _ = new GatePeriodicalView
            {
                DataContext = DegreasingChemicalCompositionVM.LoadVM(db)
            };
        }

        public ICommand CoatingChemicalCompositionOpenCommand { get; private set; }
        private void CoatingChemicalCompositionOpen()
        {
            _ = new GatePeriodicalView
            {
                DataContext = CoatingChemicalCompositionVM.LoadVM(db)
            };
        }

        public ICommand CoatingPlasticityOpenCommand { get; private set; }
        private void CoatingPlasticityOpen()
        {
            _ = new GatePeriodicalView
            {
                DataContext = CoatingPlasticityVM.LoadVM(db)
            };
        }

        public ICommand CoatingProtectivePropertiesOpenCommand { get; private set; }
        private void CoatingProtectivePropertiesOpen()
        {
            _ = new GatePeriodicalView
            {
                DataContext = CoatingProtectivePropertiesVM.LoadVM(db)
            };
        }

        public ICommand CoatingPorosityOpenCommand { get; private set; }
        private void CoatingPorosityOpen()
        {
            _ = new CoatingPorosityView
            {
                DataContext = CoatingPorosityVM.LoadVM(db)
            };
        }

        public Commands.IAsyncCommand<int> LoadItemCommand { get; private set; }
        public async Task Load(int id)
        {
            try
            {
                IsBusy = true;
                Gates = await Task.Run(() => gateRepo.GetAllAsyncForCompare());
                SelectedItem = await Task.Run(() => gateRepo.GetByIdIncludeAsync(id));

                await Task.Run(() => materialRepo.Load());
                Materials = materialRepo.SortList();

                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                PIDs = await Task.Run(() => pIDRepo.GetAllAsync());
                Drawings = await Task.Run(() => gateRepo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Points = await Task.Run(() => gateRepo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
                //InputControlJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                //PreparationJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Подготовка поверхности").OrderBy(x => x.PointId).ToList();
                CoatingJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Покрытие").OrderBy(x => x.PointId).ToList();
                //TestJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "ПСИ").OrderBy(x => x.PointId).ToList();
                DocumentationJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                //ShippedJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Отгрузка").OrderBy(x => x.PointId).ToList();
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
                  await Task.Run(() => gateRepo.Update(SelectedItem));
                
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
                SelectedItem.GateJournals.Add(new GateJournal(SelectedItem, SelectedTCPPoint));
                
                //InputControlJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                //PreparationJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Подготовка поверхности").OrderBy(x => x.PointId).ToList();
                CoatingJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Покрытие").OrderBy(x => x.PointId).ToList();
                //TestJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "ПСИ").OrderBy(x => x.PointId).ToList();
                DocumentationJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                //ShippedJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Отгрузка").OrderBy(x => x.PointId).ToList();
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
                        SelectedItem.GateJournals.Remove(Operation);                        
                        //InputControlJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                        //PreparationJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Подготовка поверхности").OrderBy(x => x.PointId).ToList();
                        CoatingJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Покрытие").OrderBy(x => x.PointId).ToList();
                        //TestJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "ПСИ").OrderBy(x => x.PointId).ToList();
                        DocumentationJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                        //ShippedJournal = SelectedItem.GateJournals.Where(i => i.EntityTCP.OperationType.Name == "Отгрузка").OrderBy(x => x.PointId).ToList();
                    }
                }
                else MessageBox.Show("Выберите операцию!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
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

        public ICommand EditMaterialCommand { get; private set; }
        private void EditMaterial()
        {
            if (SelectedItem.MetalMaterial != null)
            {
                if (SelectedItem.MetalMaterial is PipeMaterial)
                {
                    _ = new PipeMaterialEditView
                    {
                        DataContext = PipeMaterialEditVM.LoadPipeMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
                    };
                }

                if (SelectedItem.MetalMaterial is SheetMaterial)
                {
                    _ = new SheetMaterialEditView
                    {
                        DataContext = SheetMaterialEditVM.LoadSheetMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
                    };
                }
                //else if (SelectedItem.MetalMaterial is ForgingMaterial)
                //{
                //    _ = new ForgingMaterialEditView
                //    {
                //        DataContext = ForgingMaterialEditVM.LoadForgingMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
                //    };
                //}
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
            bool check = true;
            bool flag = true;
            bool assemblyFlag = false;

            if (SelectedItem.MetalMaterialId != null)
            {
                SelectedItem.Material = SelectedItem.MetalMaterial.Material;
                SelectedItem.Melt = SelectedItem.MetalMaterial.Melt;
                SelectedItem.Certificate = SelectedItem.MetalMaterial.Certificate;
                if (SelectedItem.MetalMaterial.Status == "НЕ СООТВ.")
                {
                    SelectedItem.Status = "НЕ СООТВ.";
                    flag = false;
                    MessageBox.Show("Выбранный прокат имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущему шиберу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            if (SelectedItem.Number == null || SelectedItem.Number == "")
            {
                MessageBox.Show("Не оставляйте номер пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                check = false;
            }
            else if (!SelectedItem.Number.ToLower().Contains("удалить"))
            {
                if (SelectedItem.Number != null && SelectedItem.ZK != null)
                {
                    int count = 0;
                    foreach (Gate entity in Gates)
                    {
                        if (SelectedItem.Number.Equals(entity.Number) && SelectedItem.ZK.Equals(entity.ZK))
                        {
                            count++;
                            if (count > 1)
                            {
                                MessageBox.Show("Шибер с таким ЗК уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }

            if (SelectedItem.GateJournals != null)
            {
                foreach (GateJournal journal in SelectedItem.GateJournals)
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
                        if (journal.PointId == 42 && journal.InspectorId != null && journal.Status == "Cоответствует")
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
                    int value = await Task.Run(() => gateRepo.Update(SelectedItem));
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

        public GateEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            gateRepo = new GateRepository(db);
            inspectorRepo = new InspectorRepository(db);
            materialRepo = new MetalMaterialRepository(db);
            pIDRepo = new PIDRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand(SaveItem);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.Command(o => AddJournalOperation());
            RemoveOperationCommand = new Supervision.Commands.Command(o => RemoveOperation());
            EditMaterialCommand = new Supervision.Commands.Command(o => EditMaterial());
            EditPIDCommand = new Supervision.Commands.Command(o => EditPID());
            DegreasingChemicalCompositionOpenCommand = new Supervision.Commands.Command(o => DegreasingChemicalCompositionOpen());
            CoatingChemicalCompositionOpenCommand = new Supervision.Commands.Command(o => CoatingChemicalCompositionOpen());
            CoatingPlasticityOpenCommand = new Supervision.Commands.Command(o => CoatingPlasticityOpen());
            CoatingPorosityOpenCommand = new Supervision.Commands.Command(o => CoatingPorosityOpen());
            CoatingProtectivePropertiesOpenCommand = new Supervision.Commands.Command(o => CoatingProtectivePropertiesOpen());
        }
    }
}
