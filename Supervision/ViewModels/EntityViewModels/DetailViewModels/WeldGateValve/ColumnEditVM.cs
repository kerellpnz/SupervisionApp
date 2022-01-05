using DataLayer;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
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
using DataLayer.Entities.Detailing;
using Supervision.Views.EntityViews.DetailViews;
using Supervision.Views.EntityViews;
using System.Text.RegularExpressions;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels
{
    public class ColumnEditVM : ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<RunningSleeve> runningSleeves;
        private readonly RunningSleeveRepository runningSleeveRepo;
        private IEnumerable<string> journalNumbers;
        //private IList<MetalMaterial> materials;
        private IEnumerable<string> drawings;
        private IEnumerable<ColumnTCP> points;
        private IList<Inspector> inspectors;
        //private IEnumerable<ColumnJournal> castJournal;
        //private IEnumerable<ColumnJournal> sheetJournal;
        //private IEnumerable<ColumnJournal> compactJournal;
        private readonly BaseTable parentEntity;
        private ColumnJournal operation;
        private Column selectedItem;
        private ColumnTCP selectedTCPPoint;
        private readonly ColumnRepository repo;
        private readonly InspectorRepository inspectorRepo;
        //private readonly MetalMaterialRepository materialRepo;
        private readonly JournalNumberRepository journalRepo;
        //private IEnumerable<PID> pIDs;
        //private readonly PIDRepository pIDRepo;
        private IEnumerable<Column> columns;

        public IEnumerable<Column> Columns
        {
            get => columns;
            set
            {
                columns = value;
                RaisePropertyChanged();
            }
        }

        //public IEnumerable<PID> PIDs
        //{
        //    get => pIDs;
        //    set
        //    {
        //        pIDs = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public ICommand EditPIDCommand { get; private set; }
        //private void EditPID()
        //{
        //    if (SelectedItem.PID != null)
        //    {
        //        _ = new PIDEditView
        //        {
        //            DataContext = PIDEditVM.LoadPIDEditVM(SelectedItem.PID.Id, SelectedItem, db)
        //        };
        //    }
        //    else MessageBox.Show("PID не выбран", "Ошибка");
        //}


        public Column SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public ColumnJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<RunningSleeve> RunningSleeves
        {
            get => runningSleeves;
            set
            {
                runningSleeves = value;
                RaisePropertyChanged();
            }
        }
        //public IEnumerable<ColumnJournal> CastJournal
        //{
        //    get => castJournal;
        //    set
        //    {
        //        castJournal = value;
        //        RaisePropertyChanged();
        //    }
        //}
        //public IEnumerable<ColumnJournal> SheetJournal
        //{
        //    get => sheetJournal;
        //    set
        //    {
        //        sheetJournal = value;
        //        RaisePropertyChanged();
        //    }
        //}
        //public IEnumerable<ColumnJournal> CompactJournal
        //{
        //    get => compactJournal;
        //    set
        //    {
        //        compactJournal = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public IEnumerable<ColumnTCP> Points
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

        //public IList<MetalMaterial> Materials
        //{
        //    get => materials;
        //    set
        //    {
        //        materials = value;
        //        RaisePropertyChanged();
        //    }
        //}
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

        public ColumnTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }


        public static ColumnEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            ColumnEditVM vm = new ColumnEditVM(entity, context);
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
                Columns = await Task.Run(() => repo.GetAllAsync());
                //PIDs = await Task.Run(() => pIDRepo.GetAllAsync());

                await Task.Run(() => runningSleeveRepo.Load());
                RunningSleeves = runningSleeveRepo.SortList();

                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));
                //Materials = await Task.Run(() => materialRepo.GetAllAsync());
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Drawings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Points = await Task.Run(() => repo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
                /*CastJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШ").OrderBy(x => x.PointId);
                SheetJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШЛ").OrderBy(x => x.PointId);
                CompactJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШК").OrderBy(x => x.PointId);*/
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
                SelectedItem.ColumnJournals.Add(new ColumnJournal(SelectedItem, SelectedTCPPoint));
                await SaveItemCommand.ExecuteAsync();
                /*CastJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШ").OrderBy(x => x.PointId);
                SheetJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШЛ").OrderBy(x => x.PointId);
                CompactJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШК").OrderBy(x => x.PointId);*/
                SelectedTCPPoint = null;
            }
        }


        public ICommand EditRunningSleeveCommand { get; private set; }
        private void EditRunningSleeve()
        {
            if (SelectedItem.RunningSleeve != null)
            {
                _ = new RunningSleeveEditView
                {
                    DataContext = RunningSleeveEditVM.LoadVM(SelectedItem.RunningSleeve.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите материал", "Ошибка");
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
                        SelectedItem.ColumnJournals.Remove(Operation);
                        await SaveItemCommand.ExecuteAsync();
                        /*CastJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШ").OrderBy(x => x.PointId);
                        SheetJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШЛ").OrderBy(x => x.PointId);
                        CompactJournal = SelectedItem.ColumnJournals.Where(i => i.EntityTCP.ProductType.ShortName == "ЗШК").OrderBy(x => x.PointId);*/
                    }
                }
                else MessageBox.Show("Выберите операцию!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }

        }

        //public ICommand EditMaterialCommand { get; private set; }
        //private void EditMaterial()
        //{
        //    if (SelectedItem.MetalMaterial != null)
        //    {
        //        if (SelectedItem.MetalMaterial is PipeMaterial)
        //        {
        //            _ = new PipeMaterialEditView
        //            {
        //                DataContext = PipeMaterialEditVM.LoadPipeMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
        //            };
        //        }

        //        if (SelectedItem.MetalMaterial is SheetMaterial)
        //        {
        //            _ = new SheetMaterialEditView
        //            {
        //                DataContext = SheetMaterialEditVM.LoadSheetMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
        //            };
        //        }
        //        //else if (SelectedItem.MetalMaterial is ForgingMaterial)
        //        //{
        //        //    _ = new ForgingMaterialEditView
        //        //    {
        //        //        DataContext = ForgingMaterialEditVM.LoadForgingMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
        //        //    };
        //        //}
        //        else if (SelectedItem.MetalMaterial is RolledMaterial)
        //        {
        //            _ = new RolledMaterialEditView
        //            {
        //                DataContext = RolledMaterialEditVM.LoadRolledMaterialEditVM(SelectedItem.MetalMaterial.Id, SelectedItem, db)
        //            };
        //        }
        //    }
        //    else MessageBox.Show("Для просмотра привяжите материал", "Ошибка");
        //}

        protected new async Task CloseWindow(object obj)
        {
            bool check = true;
            bool flag = true;
            bool assemblyFlag = false;

            if (SelectedItem.RunningSleeveId != null)
            {
                if (await Task.Run(() => runningSleeveRepo.IsAssembliedAsync(SelectedItem)))
                {
                    SelectedItem.RunningSleeve = null;
                    check = false;
                }
                else if (SelectedItem.RunningSleeve.Status == "НЕ СООТВ.")
                {
                    SelectedItem.Status = "НЕ СООТВ.";
                    flag = false;
                    MessageBox.Show("Выбранная ходовая втулка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей стойке.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            if (SelectedItem.Number == null || SelectedItem.Number == "")
            {
                MessageBox.Show("Не оставляйте номер пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                check = false;
            }
            else if (!SelectedItem.Number.ToLower().Contains("удалить"))
            {
                if (SelectedItem.Number != null)
                {
                    int count = 0;
                    foreach (Column entity in Columns)
                    {
                        if (SelectedItem.Number.Equals(entity.Number))
                        {
                            count++;
                            if (count > 1)
                            {
                                MessageBox.Show("Стойка с таким номером уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                //if (SelectedItem.Drawing == null || SelectedItem.Drawing == "")
                //{
                //    MessageBox.Show("Не введен чертеж!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                //    check = false;
                //}
            }

            if (SelectedItem.ColumnJournals != null)
            {
                foreach (ColumnJournal journal in SelectedItem.ColumnJournals)
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
                        if (journal.PointId == 90 && journal.InspectorId != null && journal.Status == "Cоответствует")
                        {
                            assemblyFlag = true;
                        }
                    }
                }
            }   

            if (flag)
            {
                SelectedItem.Status = "Cоотв.";
                if (assemblyFlag && SelectedItem.RunningSleeveId != null) SelectedItem.Status = "Готово к сборке";
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

        public ColumnEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            //pIDRepo = new PIDRepository(db);
            //EditPIDCommand = new Supervision.Commands.Command(o => EditPID());
            repo = new ColumnRepository(db);
            runningSleeveRepo = new RunningSleeveRepository(db);
            EditRunningSleeveCommand = new Supervision.Commands.Command(o => EditRunningSleeve());
            inspectorRepo = new InspectorRepository(db);
            //materialRepo = new MetalMaterialRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand(SaveItem);
            CloseWindowCommand = new Supervision.Commands.Command(async o => await CloseWindow(o));
            AddOperationCommand = new Supervision.Commands.AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new Supervision.Commands.AsyncCommand(RemoveOperation);
            //EditMaterialCommand = new Supervision.Commands.Command(o => EditMaterial());
        }
    }
}
