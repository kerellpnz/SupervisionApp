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
using DevExpress.Mvvm.Native;
using Supervision.Commands;
using Supervision.ViewModels.EntityViewModels.Materials;
using Supervision.Views.EntityViews;
using Supervision.Views.EntityViews.DetailViews.Valve;
using Supervision.Views.EntityViews.MaterialViews;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve
{
    public class SaddleEditVM: ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IList<MetalMaterial> materials;
        private IList<ForgingMaterial> forgingMaterials; 
        
        private IEnumerable<string> drawings;
        private IEnumerable<SaddleTCP> points;
        private IList<Inspector> inspectors;
        private IEnumerable<SaddleJournal> inputControlJournal;
        private IEnumerable<SaddleJournal> manufacturingJournal;
        //private IEnumerable<SaddleJournal> compactJournal;
        private IList<FrontalSaddleSealing> frontalSaddleSeals;
        private FrontalSaddleSealing selectedFrontalSaddleSealing;
        private SaddleWithSealing selectedFrontalSaddleSealingFromList;
        private readonly BaseTable parentEntity;
        private SaddleJournal operation;
        private Saddle selectedItem;
        private SaddleTCP selectedTCPPoint;
        private readonly SaddleRepository saddleRepo;
        private readonly InspectorRepository inspectorRepo;
        private readonly MetalMaterialRepository materialRepo;
        private readonly ForgingMaterialRepository forgingRepo;
        private readonly FrontalSaddleSealingRepository sealsRepo;
        private readonly JournalNumberRepository journalRepo;
        private IEnumerable<PID> pIDs;
        private readonly PIDRepository pIDRepo;
        private IEnumerable<Saddle> saddles;

        public IEnumerable<Saddle> Saddles
        {
            get => saddles;
            set
            {
                saddles = value;
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

        public Saddle SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public SaddleJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }
        
        public IEnumerable<SaddleJournal> InputControlJournal
        {
            get => inputControlJournal;
            set
            {
                inputControlJournal = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<SaddleJournal> ManufacturingJournal
        {
            get => manufacturingJournal;
            set
            {
                manufacturingJournal = value;
                RaisePropertyChanged();
            }
        }
        //public IEnumerable<SaddleJournal> CompactJournal
        //{
        //    get => compactJournal;
        //    set
        //    {
        //        compactJournal = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public IEnumerable<SaddleTCP> Points
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
        public IList<FrontalSaddleSealing> FrontalSaddleSeals
        {
            get => frontalSaddleSeals;
            set
            {
                frontalSaddleSeals = value;
                RaisePropertyChanged();
            }
        }
        public FrontalSaddleSealing SelectedFrontalSaddleSealing
        {
            get => selectedFrontalSaddleSealing;
            set
            {
                selectedFrontalSaddleSealing = value;
                RaisePropertyChanged();
            }
        }

        public SaddleWithSealing SelectedFrontalSaddleSealingFromList
        {
            get => selectedFrontalSaddleSealingFromList;
            set
            {
                selectedFrontalSaddleSealingFromList = value;
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
        public IEnumerable<string> JournalNumbers
        {
            get => journalNumbers;
            set
            {
                journalNumbers = value;
                RaisePropertyChanged();
            }
        }

        public SaddleTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }


        public static SaddleEditVM LoadSaddleEditVM(int id, BaseTable entity, DataContext context)
        {
            SaddleEditVM vm = new SaddleEditVM(entity, context);
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
                Saddles = await Task.Run(() => saddleRepo.GetAllAsyncForCompare());
                PIDs = await Task.Run(() => pIDRepo.GetAllAsync());
                SelectedItem = await Task.Run(() => saddleRepo.GetByIdIncludeAsync(id));

                await Task.Run(() => materialRepo.Load());
                Materials = materialRepo.SortList();

                await Task.Run(() => forgingRepo.Load());
                ForgingMaterials = forgingRepo.GetByDetail("Обойма");

                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());

                await Task.Run(() => sealsRepo.Load());
                FrontalSaddleSeals = sealsRepo.SortList();

                Drawings = await Task.Run(() => saddleRepo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Points = await Task.Run(() => saddleRepo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
                InputControlJournal = SelectedItem.SaddleJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                ManufacturingJournal = SelectedItem.SaddleJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
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
                await Task.Run(() => saddleRepo.Update(SelectedItem));                
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
                SelectedItem.SaddleJournals.Add(new SaddleJournal(SelectedItem, SelectedTCPPoint));
                InputControlJournal = SelectedItem.SaddleJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                ManufacturingJournal = SelectedItem.SaddleJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
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
                        SelectedItem.SaddleJournals.Remove(Operation);
                        InputControlJournal = SelectedItem.SaddleJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                        ManufacturingJournal = SelectedItem.SaddleJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
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

        public Supervision.Commands.IAsyncCommand AddSealToSaddleCommand { get; private set; }
        private async Task AddSealToSaddle()
        {
            try
            {
                if (SelectedItem.SaddleWithSealings?.Count() < 4 || SelectedItem.SaddleWithSealings == null)
                {
                    if (SelectedFrontalSaddleSealing != null)
                    {
                        IsBusy = true;
                        if (await sealsRepo.IsAmountRemaining(SelectedFrontalSaddleSealing))
                        {
                            SaddleWithSealing saddleWithSealing = new SaddleWithSealing() { SaddleId = SelectedItem.Id, FrontalSaddleSealingId = SelectedFrontalSaddleSealing.Id, FrontalSaddleSealing = SelectedFrontalSaddleSealing };
                            SelectedItem.SaddleWithSealings.Add(saddleWithSealing);
                            SelectedFrontalSaddleSealing.AmountRemaining -= 1;

                            int value = await Task.Run(() => sealsRepo.Update(SelectedFrontalSaddleSealing));
                            if (value == 0)
                            {
                                SelectedFrontalSaddleSealing.AmountRemaining += 1;
                                SelectedItem.SaddleWithSealings.Remove(saddleWithSealing);
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedFrontalSaddleSealing = null;
                            }
                        }
                    }
                    else MessageBox.Show("Объект не выбран!", "Ошибка");
                }
                else MessageBox.Show("Невозможно привязать более 4 уплотнений!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand RemoveSealFromSaddleCommand { get; private set; }
        private async Task RemoveSealFromSaddle()
        {
            try
            {
                IsBusy = true;
                if (SelectedFrontalSaddleSealingFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        SaddleWithSealing saddleWithSealing = SelectedFrontalSaddleSealingFromList;
                        SelectedFrontalSaddleSealing = saddleWithSealing.FrontalSaddleSealing;
                        SelectedFrontalSaddleSealing.AmountRemaining += 1;
                        SelectedItem.SaddleWithSealings.Remove(saddleWithSealing);

                        int value = await Task.Run(() => sealsRepo.Update(SelectedFrontalSaddleSealing));
                        if (value == 0)
                        {
                            SelectedFrontalSaddleSealing.AmountRemaining -= 1;
                            SelectedItem.SaddleWithSealings.Add(saddleWithSealing);
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            SelectedFrontalSaddleSealing = null;
                        }
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditSealCommand { get; private set; }
        private void EditSeal()
        {
            if (SelectedFrontalSaddleSealingFromList != null)
            {
                _ = new FrontalSaddleSealingEditView
                {
                    DataContext = FrontalSaddleSealingEditVM.LoadFrontalSaddleSealingEditVM(SelectedFrontalSaddleSealingFromList.FrontalSaddleSealing.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
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
                bool flag = true;
                bool check = true;
                bool assemblyFlag1 = false;
                bool assemblyFlag2 = false;

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
                            MessageBox.Show("Выбранная поковка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей обойме.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        MessageBox.Show("Выбранный прокат имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей обойме.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        foreach (Saddle entity in Saddles)
                        {
                            if (SelectedItem.Number.Equals(entity.Number) && SelectedItem.ZK.Equals(entity.ZK))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Обойма с таким ЗК уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }
                    else if (SelectedItem.Number != null && SelectedItem.Melt != null && SelectedItem.Certificate != null && SelectedItem.Drawing != null)
                    {
                        int count = 0;
                        foreach (Saddle entity in Saddles)
                        {
                            if (SelectedItem.Number.Equals(entity.Number) && SelectedItem.Melt.Equals(entity.Melt) && SelectedItem.Certificate.Equals(entity.Certificate) && SelectedItem.Drawing.Equals(entity.Drawing))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Обойма с таким сертификатными данными уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }
                    //if (SelectedItem.ZK == null || SelectedItem.ZK == "")
                    //{
                    //    MessageBox.Show("Не введено ЗК!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    check = false;
                    //}
                    if (SelectedItem.DN == null || SelectedItem.DN == "")
                    {
                        MessageBox.Show("Не выбран диаметр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    //if (SelectedItem.PN == null || SelectedItem.PN == "")
                    //{
                    //    MessageBox.Show("Не выбрано давление!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    check = false;
                    //}
                    if (SelectedItem.Melt == null || SelectedItem.Melt == "")
                    {
                        MessageBox.Show("Не введена плавка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Drawing == null || SelectedItem.Drawing == "")
                    {
                        MessageBox.Show("Не введен чертеж! При необходимости ставьте \"-\"", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (SelectedItem.SaddleJournals != null)
                {
                    foreach (SaddleJournal journal in SelectedItem.SaddleJournals)
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
                            if (journal.PointId == 80 && journal.InspectorId != null)
                            {
                                if (SelectedItem.PN == null || SelectedItem.PN == "")
                                {
                                    MessageBox.Show("Не выбрано давление!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    check = false;
                                }
                                if (SelectedItem.ZK == null || SelectedItem.ZK == "")
                                {
                                    MessageBox.Show("Не введено ЗК!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    check = false;
                                }
                            }
                            if (journal.PointId == 80 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag1 = true;
                            }
                            if (journal.PointId == 155 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag2 = true;
                            }
                        }
                    }
                }

                if (flag)
                {
                    SelectedItem.Status = "Cоотв.";
                    if (assemblyFlag1 && assemblyFlag2) SelectedItem.Status = "Готово к сборке";
                }

                if (check)
                {
                    try
                    {
                        IsBusy = true;
                        int value = await Task.Run(() => saddleRepo.Update(SelectedItem));
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
        //protected override void CloseWindow(object obj)
        //{
        //    if (saddleRepo.HasChanges(SelectedItem) || saddleRepo.HasChanges(SelectedItem.SaddleJournals))
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

        public SaddleEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            pIDRepo = new PIDRepository(db);
            EditPIDCommand = new Supervision.Commands.Command(o => EditPID());
            saddleRepo = new SaddleRepository(db);
            inspectorRepo = new InspectorRepository(db);
            materialRepo = new MetalMaterialRepository(db);
            forgingRepo = new ForgingMaterialRepository(db);
            sealsRepo = new FrontalSaddleSealingRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand(SaveItem);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.Command(o => AddJournalOperation());
            RemoveOperationCommand = new Supervision.Commands.Command(o => RemoveOperation());
            AddSealToSaddleCommand = new Supervision.Commands.AsyncCommand(AddSealToSaddle);
            RemoveSealFromSaddleCommand = new Supervision.Commands.AsyncCommand(RemoveSealFromSaddle);
            EditMaterialCommand = new Supervision.Commands.Command(o => EditMaterial());
            EditForgingMaterialCommand = new Supervision.Commands.Command(o => EditForgingMaterial());
            EditSealCommand = new Supervision.Commands.Command(o => EditSeal());
        }
    }
}
