using System;
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
using DataLayer.Entities.Detailing.SheetGateValveDetails;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using DataLayer.Entities.Periodical;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Periodical;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve;
using Supervision.ViewModels.EntityViewModels.Materials;
using Supervision.Views.EntityViews;
using Supervision.Views.EntityViews.DetailViews.Valve;
using Supervision.Views.EntityViews.DetailViews.WeldGateValve;
using Supervision.Views.EntityViews.MaterialViews;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.WeldGateValve
{
    public class SheetGateValveCoverEditVM : ViewModelBase
    {

        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<CoverFlange> coverFlanges;
        private IEnumerable<CoverSleeve> coverSleeves;
        private IEnumerable<CoverSleeve008> coverSleeves008;
        private IEnumerable<CaseBottom> caseBottoms;
        private IEnumerable<ForgingMaterial> forgingMaterials;
        private IEnumerable<MetalMaterial> metalMaterials;
        private IEnumerable<Spindle> spindles;
        private IEnumerable<Column> columns;
        private IEnumerable<string> drawings;
        private IEnumerable<SheetGateValveCoverTCP> points;
        private IEnumerable<Inspector> inspectors;
        private IList<SheetGateValveCoverJournal> inputControlJournal;
        private IList<SheetGateValveCoverJournal> assemblyJournal;
        private IList<SheetGateValveCoverJournal> assemblyWeldingJournal;
        private IList<SheetGateValveCoverJournal> mechanicalJournal;
        private IList<SheetGateValveCoverJournal> nDTJournal;
        private readonly BaseTable parentEntity;
        private readonly SheetGateValveCoverRepository repo;
        private readonly InspectorRepository inspectorRepo;
        private readonly JournalNumberRepository journalRepo;
        private readonly CoverFlangeRepository coverFlangeRepo;
        private readonly CoverSleeveRepository coverSleeveRepo;
        private readonly CoverSleeve008Repository coverSleeve008Repo;
        private readonly CaseBottomRepository caseBottomRepo;
        private readonly ForgingMaterialRepository forgingRepo;
        private readonly MetalMaterialRepository metalMaterialRepo;
        private readonly ColumnRepository columnRepo;
        private readonly SpindleRepository spindleRepo;
        private readonly AssemblyUnitSealingRepository sealRepo;
        private readonly WeldingPeriodicalRepository repoWeld;
        private SheetGateValveCover selectedItem;
        private SheetGateValveCoverTCP selectedTCPPoint;
        private SheetGateValveCoverJournal operation;
        private IEnumerable<PID> pIDs;
        private readonly PIDRepository pIDRepo;
        private IEnumerable<SheetGateValveCover> covers;
        private IList<WeldingProcedures> Welding;

        private CoverFlange AddedCoverFlange;
        private CaseBottom AddedCaseBotttom;
        private ForgingMaterial AddedForgingMaterial;
        private CoverSleeve AddedCoverSleeve;
        private CoverSleeve008 AddedCoverSleeve008;
        private Spindle AddedSpindle;
        private Column AddedColumn;

        private IEnumerable<AssemblyUnitSealing> seals;        
        private AssemblyUnitSealing selectedSeal;
        private BaseValveCoverWithSeals selectedSealFromList;

        public AssemblyUnitSealing SelectedSeal
        {
            get => selectedSeal;
            set
            {
                selectedSeal = value;
                RaisePropertyChanged();
            }
        }
        public BaseValveCoverWithSeals SelectedSealFromList
        {
            get => selectedSealFromList;
            set
            {
                selectedSealFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<AssemblyUnitSealing> Seals
        {
            get => seals;
            set
            {
                seals = value;
                RaisePropertyChanged();
            }
        }

        public Supervision.Commands.IAsyncCommand AddSealToValveCommand { get; private set; }
        private async Task AddSealToValve()
        {
            try
            {
                //IsBusy = true;
                //if (SelectedItem.BaseValveSCoverWithSeals == null)
                //{
                    if (SelectedSeal != null)
                    {
                        IsBusy = true;
                        if (await sealRepo.IsAmountRemainingCover(SelectedSeal))
                        {
                            BaseValveCoverWithSeals baseValveCoverWithSeals = new BaseValveCoverWithSeals() { BaseWeldValveId = SelectedItem.Id, AssemblyUnitSealingId = SelectedSeal.Id, AssemblyUnitSealing = SelectedSeal };
                            SelectedItem.BaseValveSCoverWithSeals.Add(baseValveCoverWithSeals);
                            SelectedSeal.AmountRemaining -= 1;

                            int value = await Task.Run(() => sealRepo.Update(SelectedSeal));
                            if (value == 0)
                            {
                                SelectedSeal.AmountRemaining += 1;
                                SelectedItem.BaseValveSCoverWithSeals.Remove(baseValveCoverWithSeals);
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedSeal = null;
                            }
                        }
                    }
                    else MessageBox.Show("Объект не выбран!", "Ошибка");
                //}
                //else MessageBox.Show("Невозможно добавить более 1 комплекта графлекса!", "Ошибка");                           
            }
            finally
            {
                IsBusy = false;
            }
        }


        public Supervision.Commands.IAsyncCommand DeleteSealFromValveCommand { get; private set; }
        private async Task DeleteSealFromValve()
        {
            try
            {           

                IsBusy = true;
                if (SelectedSealFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        BaseValveCoverWithSeals baseValveCoverWithSeals = SelectedSealFromList;
                        SelectedSeal = baseValveCoverWithSeals.AssemblyUnitSealing;
                        SelectedSeal.AmountRemaining += 1;
                        SelectedItem.BaseValveSCoverWithSeals.Remove(baseValveCoverWithSeals);

                        int value = await Task.Run(() => sealRepo.Update(SelectedSeal));
                        if (value == 0)
                        {
                            SelectedSeal.AmountRemaining -= 1;
                            SelectedItem.BaseValveSCoverWithSeals.Add(baseValveCoverWithSeals);
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            SelectedSeal = null;
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
            if (SelectedSealFromList != null)
            {
                _ = new AssemblyUnitSealingEditView
                {
                    DataContext = AssemblyUnitSealingEditVM.LoadVM(SelectedSealFromList.AssemblyUnitSealing.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }



        public IEnumerable<SheetGateValveCover> Covers
        {
            get => covers;
            set
            {
                covers = value;
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

        public SheetGateValveCoverJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
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

        public IList<SheetGateValveCoverJournal> AssemblyWeldingJournal
        {
            get => assemblyWeldingJournal;
            set
            {
                assemblyWeldingJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveCoverJournal> MechanicalJournal
        {
            get => mechanicalJournal;
            set
            {
                mechanicalJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveCoverJournal> NDTJournal
        {
            get => nDTJournal;
            set
            {
                nDTJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveCoverJournal> InputControlJournal
        {
            get => inputControlJournal;
            set
            {
                inputControlJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveCoverJournal> AssemblyJournal
        {
            get => assemblyJournal;
            set
            {
                assemblyJournal = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<SheetGateValveCoverTCP> Points
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

        public IEnumerable<CoverFlange> CoverFlanges
        {
            get => coverFlanges;
            set
            {
                coverFlanges = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<CoverSleeve> CoverSleeves
        {
            get => coverSleeves;
            set
            {
                coverSleeves = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<CoverSleeve008> CoverSleeves008
        {
            get => coverSleeves008;
            set
            {
                coverSleeves008 = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<CaseBottom> CaseBottoms
        {
            get => caseBottoms;
            set
            {
                caseBottoms = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<ForgingMaterial> ForgingMaterials
        {
            get => forgingMaterials;
            set
            {
                forgingMaterials = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<MetalMaterial> MetalMaterials
        {
            get => metalMaterials;
            set
            {
                metalMaterials = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<Spindle> Spindles
        {
            get => spindles;
            set
            {
                spindles = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<Column> Columns
        {
            get => columns;
            set
            {
                columns = value;
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

        public SheetGateValveCoverTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }

        public static SheetGateValveCoverEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            SheetGateValveCoverEditVM vm = new SheetGateValveCoverEditVM(entity, context);
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
                Covers = await Task.Run(() => repo.GetAllAsyncForCompare());
                PIDs = await Task.Run(() => pIDRepo.GetAllAsync());
                Welding = await Task.Run(() => repoWeld.GetAllAsync());

                await Task.Run(() => sealRepo.Load());
                await Task.Run(() => coverFlangeRepo.Load());
                await Task.Run(() => caseBottomRepo.Load());
                await Task.Run(() => coverSleeve008Repo.Load());
                await Task.Run(() => coverSleeveRepo.Load());
                await Task.Run(() => spindleRepo.Load());

                Seals = sealRepo.SortList();
                CoverFlanges = coverFlangeRepo.SortList();
                CoverSleeves = coverSleeveRepo.SortList();
                CoverSleeves008 = coverSleeve008Repo.SortList();
                CaseBottoms = caseBottomRepo.SortList();
                Spindles = spindleRepo.SortList();


                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));

                await Task.Run(() => forgingRepo.Load());
                ForgingMaterials = forgingRepo.GetByDetail("Крышка");

                await Task.Run(() => metalMaterialRepo.Load());
                MetalMaterials = metalMaterialRepo.SortList();

                Columns = await Task.Run(() => columnRepo.GetAllAsync());                
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Drawings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Points = await Task.Run(() => repo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());

                AssemblyJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка").OrderBy(x => x.PointId).ToList();
                InputControlJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                AssemblyWeldingJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                MechanicalJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
                NDTJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                
                if (SelectedItem.CoverFlangeId != null)
                {
                    AddedCoverFlange = SelectedItem.CoverFlange;
                }
                if (SelectedItem.CaseBottomId != null)
                {
                    AddedCaseBotttom = SelectedItem.CaseBottom;
                }
                if (SelectedItem.ForgingMaterialId != null)
                {
                    AddedForgingMaterial = SelectedItem.ForgingMaterial;
                }
                if (SelectedItem.CoverSleeveId != null)
                {
                    AddedCoverSleeve = SelectedItem.CoverSleeve;
                }
                if (SelectedItem.CoverSleeve008Id != null)
                {
                    AddedCoverSleeve008 = SelectedItem.CoverSleeve008;
                }
                if (SelectedItem.SpindleId != null)
                {
                    AddedSpindle = SelectedItem.Spindle;
                }
                if (SelectedItem.ColumnId != null)
                {
                    AddedColumn = SelectedItem.Column;
                }
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
                SelectedItem.SheetGateValveCoverJournals.Add(new SheetGateValveCoverJournal(SelectedItem, SelectedTCPPoint));                
                AssemblyJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка").OrderBy(x => x.PointId).ToList();
                InputControlJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                AssemblyWeldingJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                MechanicalJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
                NDTJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                
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
                        SelectedItem.SheetGateValveCoverJournals.Remove(Operation);                        
                        AssemblyJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка").OrderBy(x => x.PointId).ToList();
                        InputControlJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Входной контроль").OrderBy(x => x.PointId).ToList();
                        AssemblyWeldingJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                        MechanicalJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Механическая обработка").OrderBy(x => x.PointId).ToList();
                        NDTJournal = SelectedItem.SheetGateValveCoverJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
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

        public ICommand EditSpindleCommand { get; private set; }
        private void EditSpindle()
        {
            if (SelectedItem.Spindle != null)
            {
                _ = new SpindleEditView
                {
                    DataContext = SpindleEditVM.LoadVM(SelectedItem.Spindle.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите шпиндель", "Ошибка");
        }

        public ICommand EditCoverSleeveCommand { get; private set; }
        private void EditCoverSleeve()
        {
            if (SelectedItem.CoverSleeve != null)
            {
                _ = new CoverSleeveEditView
                {
                    DataContext = CoverSleeveEditVM.LoadVM(SelectedItem.CoverSleeve.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите втулку(016)", "Ошибка");
        }

        public ICommand EditCoverSleeve008Command { get; private set; }
        private void EditCoverSleeve008()
        {
            if (SelectedItem.CoverSleeve008 != null)
            {
                _ = new CoverSleeve008EditView
                {
                    DataContext = CoverSleeve008EditVM.LoadVM(SelectedItem.CoverSleeve008.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите втулку(008)", "Ошибка");
        }

        public ICommand EditCaseBottomCommand { get; private set; }
        private void EditCaseBottom()
        {
            if (SelectedItem.CaseBottom != null)
            {
                _ = new CaseBottomEditView
                {
                    DataContext = CaseBottomEditVM.LoadVM(SelectedItem.CaseBottom.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите днище", "Ошибка");
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
            else MessageBox.Show("Для просмотра привяжите поковку", "Ошибка");
        }


        public ICommand EditCoverFlangeCommand { get; private set; }
        private void EditCoverFlange()
        {
            if (SelectedItem.CoverFlange != null)
            {
                _ = new CoverFlangeEditView
                {
                    DataContext = CoverFlangeEditVM.LoadVM(SelectedItem.CoverFlange.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите фланец", "Ошибка");
        }

        public ICommand EditColumnCommand { get; private set; }
        private void EditColumn()
        {
            if (SelectedItem.Column != null)
            {
                _ = new ColumnEditView
                {
                    DataContext = ColumnEditVM.LoadVM(SelectedItem.Column.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите стойку", "Ошибка");
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

        public Supervision.Commands.IAsyncCommand<object> SaveItemCommand { get; private set; }
        private async Task SaveItem(object obj)
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

        public new Supervision.Commands.IAsyncCommand<object> CloseWindowCommand { get; private set; }
        protected new async Task CloseWindow(object obj)
        {
            if (IsBusy)
            {
                MessageBoxResult result = MessageBox.Show("Процесс сохранения уже запущен, теперь все в \"руках\" сервера. Попробовать отправить запрос на сохранение повторно? (Возможен вылет программы и не сохранение результата)", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await SaveItemCommand.ExecuteAsync(obj);
                }
            }
            else
            {
                bool flag = true;
                bool check = true;
                bool assemblyFlag1 = false;
                bool assemblyFlag2 = false;
                bool assemblyFlag3 = false;

                if (SelectedItem.SheetGateValveCoverJournals != null)
                {
                    foreach (SheetGateValveCoverJournal journal in SelectedItem.SheetGateValveCoverJournals)
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
                            if (journal.PointId == 63 && journal.InspectorId != null)
                            {
                                if (SelectedItem.CoverFlangeId == null)
                                {
                                    check = false;
                                    MessageBox.Show("Не выбран фланец!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 64 && journal.InspectorId != null)
                            {
                                if (SelectedItem.CoverSleeveId == null)
                                {
                                    check = false;
                                    MessageBox.Show("Не выбрана центральная втулка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 65 && journal.InspectorId != null)
                            {
                                if (SelectedItem.CoverSleeve008Id == null)
                                {
                                    check = false;
                                    MessageBox.Show("Не выбрана дренажная втулка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 66 && journal.InspectorId != null && Welding[0].Id == 1 && Welding[0].LastControl != null)
                            {
                                if (journal.Date > Welding[0].LastControl)
                                {
                                    Welding[0] = await Task.Run(() => repoWeld.GetByIdIncludeAsync(1));
                                    Welding[0].WeldingProceduresJournals.Add(new WeldingProceduresJournal()
                                    {
                                        DetailId = 1,
                                        PointId = 120,
                                        Point = journal.Point,
                                        Description = journal.Description,
                                        JournalNumber = journal.JournalNumber,
                                        Date = journal.Date,
                                        Status = journal.Status,
                                        RemarkClosed = journal.RemarkClosed,
                                        RemarkIssued = journal.RemarkIssued,
                                        Comment = journal.Comment,
                                        InspectorId = journal.InspectorId,
                                        PlaceOfControl = journal.PlaceOfControl,
                                        Documents = journal.Documents,
                                        DateOfRemark = journal.DateOfRemark,
                                        RemarkInspector = journal.RemarkInspector
                                    });
                                    Welding[0].LastControl = journal.Date;
                                    Welding[0].NextControl = Convert.ToDateTime(journal.Date).AddDays(7);
                                    int value = await Task.Run(() => repoWeld.Update(Welding[0]));
                                    if (value == 0)
                                    {
                                        MessageBox.Show("Режимы АФ не были сохранены в периодический контроль из-за ошибки сервера!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            if (journal.PointId == 67 && journal.InspectorId != null && Welding[1].Id == 2 && Welding[1].LastControl != null)
                            {
                                if (journal.Date > Welding[1].LastControl)
                                {
                                    Welding[1] = await Task.Run(() => repoWeld.GetByIdIncludeAsync(2));
                                    Welding[1].WeldingProceduresJournals.Add(new WeldingProceduresJournal()
                                    {
                                        DetailId = 2,
                                        PointId = 120,
                                        Point = journal.Point,
                                        Description = journal.Description,
                                        JournalNumber = journal.JournalNumber,
                                        Date = journal.Date,
                                        Status = journal.Status,
                                        RemarkClosed = journal.RemarkClosed,
                                        RemarkIssued = journal.RemarkIssued,
                                        Comment = journal.Comment,
                                        InspectorId = journal.InspectorId,
                                        PlaceOfControl = journal.PlaceOfControl,
                                        Documents = journal.Documents,
                                        DateOfRemark = journal.DateOfRemark,
                                        RemarkInspector = journal.RemarkInspector
                                    });
                                    Welding[1].LastControl = journal.Date;
                                    Welding[1].NextControl = Convert.ToDateTime(journal.Date).AddDays(7);
                                    int value = await Task.Run(() => repoWeld.Update(Welding[1]));
                                    if (value == 0)
                                    {
                                        MessageBox.Show("Режимы МП не были сохранены в периодический контроль из-за ошибки сервера!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            if (journal.PointId == 68 && journal.InspectorId != null && Welding[2].Id == 3 && Welding[2].LastControl != null)
                            {
                                if (journal.Date > Welding[2].LastControl)
                                {
                                    Welding[2] = await Task.Run(() => repoWeld.GetByIdIncludeAsync(3));
                                    Welding[2].WeldingProceduresJournals.Add(new WeldingProceduresJournal()
                                    {
                                        DetailId = 3,
                                        PointId = 120,
                                        Point = journal.Point,
                                        Description = journal.Description,
                                        JournalNumber = journal.JournalNumber,
                                        Date = journal.Date,
                                        Status = journal.Status,
                                        RemarkClosed = journal.RemarkClosed,
                                        RemarkIssued = journal.RemarkIssued,
                                        Comment = journal.Comment,
                                        InspectorId = journal.InspectorId,
                                        PlaceOfControl = journal.PlaceOfControl,
                                        Documents = journal.Documents,
                                        DateOfRemark = journal.DateOfRemark,
                                        RemarkInspector = journal.RemarkInspector
                                    });
                                    Welding[2].LastControl = journal.Date;
                                    Welding[2].NextControl = Convert.ToDateTime(journal.Date).AddDays(7);
                                    int value = await Task.Run(() => repoWeld.Update(Welding[2]));
                                    if (value == 0)
                                    {
                                        MessageBox.Show("Режимы РД не были сохранены в периодический контроль из-за ошибки сервера!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            if (journal.PointId == 76 && journal.InspectorId != null)
                            {
                                if (Welding[0].Id == 1 && Welding[0].NextControl != null)
                                {
                                    if (Welding[0].NextControl < DateTime.Now && journal.Date > Convert.ToDateTime(Welding[0].NextControl).AddDays(2))
                                    {
                                        journal.Status = "Не соответствует";
                                        SelectedItem.Status = "НЕ СООТВ.";
                                        flag = false;
                                        MessageBox.Show("Просрочен контроль режимов сварки АФ. Статус контроля документов установлен на \"Не соответствует\"." +
                                            "Обратитесь в службу ОТК завода для выяснения обстоятельств.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                                if (Welding[1].Id == 2 && Welding[1].NextControl != null)
                                {
                                    if (Welding[1].NextControl < DateTime.Now && journal.Date > Convert.ToDateTime(Welding[1].NextControl).AddDays(2))
                                    {
                                        journal.Status = "Не соответствует";
                                        SelectedItem.Status = "НЕ СООТВ.";
                                        flag = false;
                                        MessageBox.Show("Просрочен контроль режимов сварки МП. Статус контроля документов установлен на \"Не соответствует\"." +
                                            "Обратитесь в службу ОТК завода для выяснения обстоятельств.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            if (journal.PointId == 73 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag1 = true;
                            }
                            if (journal.PointId == 76 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag2 = true;
                            }
                            if (journal.PointId == 154 && journal.InspectorId != null && journal.Status == "Cоответствует")
                            {
                                assemblyFlag3 = true;
                            }
                        }
                    }
                }                 

                if (SelectedItem.MetalMaterialId != null)
                {
                    if (SelectedItem.CaseBottomId == null && SelectedItem.ForgingMaterialId == null)
                    {
                        AddDataFromMetalMaterial();
                    }
                    else ErrorMaterialMessage();
                }
                if (SelectedItem.ForgingMaterialId != null)
                {
                    if (SelectedItem.MetalMaterialId == null && SelectedItem.CaseBottomId == null)
                    {
                        if (AddedForgingMaterial == null || !SelectedItem.ForgingMaterial.Equals(AddedForgingMaterial))
                        {
                            if (await Task.Run(() => forgingRepo.IsAssembliedAsync(SelectedItem)))
                            {
                                SelectedItem.ForgingMaterial = null;
                                check = false;
                            }
                            else AddDataFromForgingMaterial();                            
                        }
                        else AddDataFromForgingMaterial();
                    }
                    else ErrorMaterialMessage();
                }
                if (SelectedItem.CaseBottomId != null)
                {
                    if (SelectedItem.MetalMaterialId == null && SelectedItem.ForgingMaterialId == null)
                    {
                        if (AddedCaseBotttom == null || !SelectedItem.CaseBottom.Equals(AddedCaseBotttom))
                        {
                            if (await Task.Run(() => caseBottomRepo.IsAssembliedAsync(SelectedItem)))
                            {
                                SelectedItem.CaseBottom = null;
                                check = false;
                            }
                            else AddDataFromCaseBottom();
                        }
                        else AddDataFromCaseBottom();
                    }
                    else ErrorMaterialMessage();
                }  

                void ErrorMaterialMessage()
                {
                    MessageBox.Show("Выберите один материал!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    SelectedItem.ForgingMaterial = null;
                    SelectedItem.CaseBottom = null;
                    SelectedItem.MetalMaterial = null;
                    check = false;
                }

                void AddDataFromForgingMaterial()
                {
                    SelectedItem.Material = SelectedItem.ForgingMaterial.Material;
                    SelectedItem.Melt = SelectedItem.ForgingMaterial.Melt;
                    SelectedItem.MetalMaterial = null;
                    SelectedItem.CaseBottom = null;
                    
                    if (SelectedItem.ForgingMaterial.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранная поковка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                void AddDataFromMetalMaterial()
                {
                    SelectedItem.Material = SelectedItem.MetalMaterial.Material;
                    SelectedItem.Melt = SelectedItem.MetalMaterial.Melt;
                    SelectedItem.ForgingMaterial = null;
                    SelectedItem.CaseBottom = null;
                    
                    if (SelectedItem.MetalMaterial.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранный прокат имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                void AddDataFromCaseBottom()
                {
                    if (SelectedItem.CaseBottom.Melt != null)
                    {
                        if (String.IsNullOrWhiteSpace(SelectedItem.Number))
                        {
                            SelectedItem.Number = SelectedItem.CaseBottom.Number;
                        }
                        SelectedItem.Material = SelectedItem.CaseBottom.Material;
                        SelectedItem.Melt = SelectedItem.CaseBottom.Melt;
                        SelectedItem.MetalMaterial = null;
                        SelectedItem.ForgingMaterial = null;
                        
                        if (SelectedItem.CaseBottom.Status == "НЕ СООТВ.")
                        {
                            SelectedItem.Status = "НЕ СООТВ.";
                            flag = false;
                            MessageBox.Show("Выбранное днище имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        SelectedItem.MetalMaterial = null;
                        SelectedItem.ForgingMaterial = null;
                        check = false;
                        MessageBox.Show("У днища отсутствует плавка! Перейдите в днище, привяжите материал и нажмите \"Закрыть\"", "Материал", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                void CheckSpindleStatus()
                {
                    if (SelectedItem.Spindle.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранный шпиндель имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                void CheckCoverSleeveStatus()
                {
                    if (SelectedItem.CoverSleeve.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранная втулка(016) имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                void CheckCoverSleeve008Status()
                {
                    if (SelectedItem.CoverSleeve008.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранная втулка(008) имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                void CheckCoverFlangeStatus()
                {
                    if (SelectedItem.CoverFlange.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранный фланец имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                void CheckColumnStatus()
                {
                   if (SelectedItem.Column.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранная стойка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей крышке", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }


                if (SelectedItem.SpindleId != null)
                {
                    if (AddedSpindle == null || !SelectedItem.Spindle.Equals(AddedSpindle))
                    {
                        if (await Task.Run(() => spindleRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.Spindle = null;
                            check = false;
                        }
                        else CheckSpindleStatus();
                    }
                    else CheckSpindleStatus();
                }
                if (SelectedItem.CoverSleeveId != null)
                {
                    foreach (SheetGateValveCoverJournal journal in AssemblyWeldingJournal)
                    {
                        if (journal.PointId == 64 && journal.InspectorId == null)
                        {
                            check = false;
                            MessageBox.Show("Не выбрана операция СпС (шов №10)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    if (AddedCoverSleeve == null || !SelectedItem.CoverSleeve.Equals(AddedCoverSleeve))
                    {
                        if (await Task.Run(() => coverSleeveRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.CoverSleeve = null;
                            check = false;
                        }
                        else CheckCoverSleeveStatus();
                    }
                    else CheckCoverSleeveStatus();
                }
                if (SelectedItem.CoverSleeve008Id != null)
                {
                    foreach (SheetGateValveCoverJournal journal in AssemblyWeldingJournal)
                    {
                        if (journal.PointId == 65 && journal.InspectorId == null)
                        {
                            check = false;
                            MessageBox.Show("Не выбрана операция СпС (шов №12, №13)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    if (AddedCoverSleeve008 == null || !SelectedItem.CoverSleeve008.Equals(AddedCoverSleeve008))
                    {
                        if (await Task.Run(() => coverSleeve008Repo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.CoverSleeve008 = null;
                            check = false;
                        }
                        else CheckCoverSleeve008Status();
                    }
                    else CheckCoverSleeve008Status();
                }
                if (SelectedItem.CoverFlangeId != null)
                {
                    foreach (SheetGateValveCoverJournal journal in AssemblyWeldingJournal)
                    {
                        if (journal.PointId == 63 && journal.InspectorId == null)
                        {
                            check = false;
                            MessageBox.Show("Не выбрана операция СпС (шов №9)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    if (AddedCoverFlange == null || !SelectedItem.CoverFlange.Equals(AddedCoverFlange))
                    {
                        if (await Task.Run(() => coverFlangeRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.CoverFlange = null;
                            check = false;
                        }
                        else CheckCoverFlangeStatus();
                    }
                    else CheckCoverFlangeStatus();
                }
                if (SelectedItem.ColumnId != null)
                {
                    if (AddedColumn == null || !SelectedItem.Column.Equals(AddedColumn))
                    {
                        if (await Task.Run(() => columnRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.Column = null;
                            check = false;
                        }
                        else CheckColumnStatus();
                    }
                    else CheckColumnStatus();
                }

                if (SelectedItem.Number == null || SelectedItem.Number == "")
                {
                    MessageBox.Show("Не оставляйте номер пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if (!SelectedItem.Number.ToLower().Contains("удалить"))
                {
                    if (SelectedItem.Number != null && SelectedItem.DN != null && SelectedItem.Melt != null && SelectedItem.Certificate != null && SelectedItem.Drawing != null)
                    {
                        int count = 0;
                        foreach (SheetGateValveCover entity in Covers)
                        {

                            if (SelectedItem.Number.Equals(entity.Number) && 
                                    SelectedItem.DN.Equals(entity.DN) && 
                                        SelectedItem.Melt.Equals(entity.Melt) &&
                                            SelectedItem.Certificate.Equals(entity.Certificate) &&
                                                SelectedItem.Drawing.Equals(entity.Drawing))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Крышка с таким номером уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    if (SelectedItem.PN == null || SelectedItem.PN == "")
                    {
                        MessageBox.Show("Не выбрано давление!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Drawing == null || SelectedItem.Drawing == "")
                    {
                        MessageBox.Show("Не введен чертеж!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                }

                if (flag)
                {
                    SelectedItem.Status = "Cоотв.";
                    if (assemblyFlag1 && assemblyFlag2 && assemblyFlag3) SelectedItem.Status = "Готово к сборке";
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

                

           

            //if (repo.HasChanges(SelectedItem) || repo.HasChanges(SelectedItem.SheetGateValveCoverJournals))
            //{
            //    MessageBoxResult result = MessageBox.Show("Закрыть без сохранения изменений?", "Выход", MessageBoxButton.YesNo);

            //    if (result == MessageBoxResult.Yes)
            //    {
            //        base.CloseWindow(obj);
            //    }
            //}
            //else
            //{
            //    base.CloseWindow(obj);
            //}
        }

        public SheetGateValveCoverEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            pIDRepo = new PIDRepository(db);
            EditPIDCommand = new Supervision.Commands.Command(o => EditPID());
            repo = new SheetGateValveCoverRepository(db);
            inspectorRepo = new InspectorRepository(db);
            journalRepo = new JournalNumberRepository(db);
            coverFlangeRepo = new CoverFlangeRepository(db);
            coverSleeveRepo = new CoverSleeveRepository(db);
            coverSleeve008Repo = new CoverSleeve008Repository(db);
            caseBottomRepo = new CaseBottomRepository(db);
            forgingRepo = new ForgingMaterialRepository(db);
            metalMaterialRepo = new MetalMaterialRepository(db);
            columnRepo = new ColumnRepository(db);
            spindleRepo = new SpindleRepository(db);
            sealRepo = new AssemblyUnitSealingRepository(db);
            repoWeld = new WeldingPeriodicalRepository(db);

            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand<object>(SaveItem);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.Command(o => AddJournalOperation());
            RemoveOperationCommand = new Supervision.Commands.Command(o => RemoveOperation());
            EditSpindleCommand = new Supervision.Commands.Command(o => EditSpindle());
            EditCoverFlangeCommand = new Supervision.Commands.Command(o => EditCoverFlange());
            EditCoverSleeveCommand = new Supervision.Commands.Command(o => EditCoverSleeve());
            EditCoverSleeve008Command = new Supervision.Commands.Command(o => EditCoverSleeve008());
            EditCaseBottomCommand = new Supervision.Commands.Command(o => EditCaseBottom());
            EditForgingMaterialCommand = new Supervision.Commands.Command(o => EditForgingMaterial());
            EditMaterialCommand = new Supervision.Commands.Command(o => EditMaterial());
            EditColumnCommand = new Supervision.Commands.Command(o => EditColumn());
            AddSealToValveCommand = new Supervision.Commands.AsyncCommand(AddSealToValve);
            DeleteSealFromValveCommand = new Supervision.Commands.AsyncCommand(DeleteSealFromValve);
            EditSealCommand = new Supervision.Commands.Command(o => EditSeal());
        }

        

        
    }
}
