using DataLayer;
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
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using System.Text.RegularExpressions;
using Supervision.Commands;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels
{
    public class Ring043EditVM : ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IList<MetalMaterial> materials;
        private IEnumerable<string> drawings;
        private IEnumerable<Ring043TCP> points;
        private IList<Inspector> inspectors;
        private readonly BaseTable parentEntity;
        private Ring043Journal operation;
        private Ring043 selectedItem;
        private Ring043TCP selectedTCPPoint;
        private readonly Ring043Repository repo;
        private readonly InspectorRepository inspectorRepo;
        private readonly MetalMaterialRepository materialRepo;
        private readonly ForgingMaterialRepository forgingRepo;
        private readonly JournalNumberRepository journalRepo;
        private IEnumerable<Ring043> rings043;
        private IList<ForgingMaterial> forgingMaterials;


        public IList<ForgingMaterial> ForgingMaterials
        {
            get => forgingMaterials;
            set
            {
                forgingMaterials = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<Ring043> Rings043
        {
            get => rings043;
            set
            {
                rings043 = value;
                RaisePropertyChanged();
            }
        }

        public Ring043 SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public Ring043Journal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<Ring043TCP> Points
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

        public Ring043TCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }


        public static Ring043EditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            Ring043EditVM vm = new Ring043EditVM(entity, context);
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
                Rings043 = await Task.Run(() => repo.GetAllAsync());
                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));

                await Task.Run(() => materialRepo.Load());
                Materials = materialRepo.SortList();

                await Task.Run(() => forgingRepo.Load());
                ForgingMaterials = forgingRepo.GetByDetail("Кольцо");

                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Drawings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Drawing));
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
                SelectedItem.Ring043Journals.Add(new Ring043Journal(SelectedItem, SelectedTCPPoint));
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
                        SelectedItem.Ring043Journals.Remove(Operation);
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
                            MessageBox.Show("Выбранная поковка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущему кольцу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        MessageBox.Show("Выбранный прокат имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущему кольцу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                if (SelectedItem.Number == null || SelectedItem.Number == "")
                {
                    MessageBox.Show("Не оставляйте номер пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if (!SelectedItem.Number.ToLower().Contains("удалить"))
                {
                    /*if (SelectedItem.Number != null && SelectedItem.ZK != null)
                    {
                        int count = 0;
                        foreach (Ring043 entity in Rings043)
                        {

                            if (SelectedItem.Number.Equals(entity.Number) && SelectedItem.ZK.Equals(entity.ZK))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Кольцо-фланец с таким ЗК уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }*/

                    if (SelectedItem.DN == null || SelectedItem.DN == "")
                    {
                        MessageBox.Show("Не выбран диаметр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    //if (SelectedItem.ZK == null || SelectedItem.ZK == "")
                    //{
                    //    MessageBox.Show("Не введено ЗК!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    check = false;
                    //}
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

                if (SelectedItem.Ring043Journals != null)
                {
                    foreach (Ring043Journal journal in SelectedItem.Ring043Journals)
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

        public Ring043EditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            repo = new Ring043Repository(db);
            inspectorRepo = new InspectorRepository(db);
            materialRepo = new MetalMaterialRepository(db);
            forgingRepo = new ForgingMaterialRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand(SaveItem);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new Supervision.Commands.AsyncCommand(RemoveOperation);
            EditMaterialCommand = new Supervision.Commands.Command(o => EditMaterial());
            EditForgingMaterialCommand = new Supervision.Commands.Command(o => EditForgingMaterial());
        }
    }
}
