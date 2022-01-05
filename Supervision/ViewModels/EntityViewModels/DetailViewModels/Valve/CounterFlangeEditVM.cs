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
using DataLayer.TechnicalControlPlans.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.Entities.Detailing;

namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve
{
    public class CounterFlangeEditVM : ViewModelBase
    {
        private readonly DataContext db;
        private IEnumerable<string> journalNumbers;
        private IList<ForgingMaterial> materials;
        private IEnumerable<string> drawings;
        private IEnumerable<CounterFlangeTCP> points;
        private IList<Inspector> inspectors;
        
        private readonly BaseTable parentEntity;
        private CounterFlangeJournal operation;
        private CounterFlange selectedItem;
        private CounterFlangeTCP selectedTCPPoint;
        private readonly CounterFlangeRepository repo;
        private readonly InspectorRepository inspectorRepo;
        private readonly ForgingMaterialRepository materialRepo;
        private readonly JournalNumberRepository journalRepo;
        private IEnumerable<CounterFlange> counterFlanges;

        public IEnumerable<CounterFlange> CounterFlanges
        {
            get => counterFlanges;
            set
            {
                counterFlanges = value;
                RaisePropertyChanged();
            }
        }

        public CounterFlange SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public CounterFlangeJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        
        public IEnumerable<CounterFlangeTCP> Points
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

        public IList<ForgingMaterial> Materials
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

        public CounterFlangeTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }


        public static CounterFlangeEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            CounterFlangeEditVM vm = new CounterFlangeEditVM(entity, context);
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
                CounterFlanges = await Task.Run(() => repo.GetAllAsync());
                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));

                await Task.Run(() => materialRepo.Load());
                Materials = materialRepo.GetByDetail("Ответный фланец");

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
                bool flag = true;
                if (SelectedItem.CounterFlangeJournals != null)
                {
                    foreach (CounterFlangeJournal journal in SelectedItem.CounterFlangeJournals)
                    {
                        if (journal.Status != null)
                        {
                            if (journal.Date == null)
                            {
                                MessageBox.Show("Не выбрана дата", "Ошибка");
                            }
                            if (journal.Status == "Не соответствует")
                            {
                                SelectedItem.Status = "Не соотв.";
                                flag = false;
                            }
                        }
                    }
                }
                

                if (flag)
                {
                    SelectedItem.Status = "Cоотв.";
                }

                if (SelectedItem != null && SelectedItem.Number != null)
                {
                    int count = 0;
                    foreach (CounterFlange entity in CounterFlanges)
                    {
                        if (SelectedItem.Number.Equals(entity.Number))
                        {
                            count++;
                            if (count > 1)
                            {
                                MessageBox.Show("Ответный фланец с таким номером уже существует!", "Ошибка");
                                count = 0;
                            }
                        }
                    }
                }

                if (SelectedItem.ForgingMaterialId != null)
                {
                    if (await Task.Run(() => materialRepo.IsAssembliedAsync(SelectedItem)))
                    {
                        SelectedItem.ForgingMaterial = null;
                    }
                }

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
                SelectedItem.CounterFlangeJournals.Add(new CounterFlangeJournal(SelectedItem, SelectedTCPPoint));
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
                        SelectedItem.CounterFlangeJournals.Remove(Operation);
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

    protected override void CloseWindow(object obj)
        {
            //if (repo.HasChanges(SelectedItem) || repo.HasChanges(SelectedItem.CounterFlangeJournals))
            //{
            //    MessageBoxResult result = MessageBox.Show("Закрыть без сохранения изменений?", "Выход", MessageBoxButton.YesNo);

            //    if (result == MessageBoxResult.Yes)
            //    {
            //        Window w = obj as Window;
            //        w?.Close();
            //    }
            //}
            //else
            //{
                Window w = obj as Window;
                w?.Close();
            //}
        }

        public CounterFlangeEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            repo = new CounterFlangeRepository(db);
            inspectorRepo = new InspectorRepository(db);
            materialRepo = new ForgingMaterialRepository(db);
            journalRepo = new JournalNumberRepository(db);
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            SaveItemCommand = new Supervision.Commands.AsyncCommand(SaveItem);
            CloseWindowCommand = new Supervision.Commands.Command(o => CloseWindow(o));
            AddOperationCommand = new Supervision.Commands.AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new Supervision.Commands.AsyncCommand(RemoveOperation);
            EditForgingMaterialCommand = new Supervision.Commands.Command(o => EditForgingMaterial());
        }
    }
}
