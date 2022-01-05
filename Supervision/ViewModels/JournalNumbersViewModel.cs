using DataLayer;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using DataLayer.Journals;
using BusinessLayer.Repository.Implementations.Entities;
using Supervision.Commands;
using System.Threading.Tasks;

namespace Supervision.ViewModels.TCPViewModels
{
    public class JournalNumbersViewModel : ViewModelBase
    {
        private readonly DataContext db;
        private readonly JournalNumberRepository repo;
        private IEnumerable<JournalNumber> allInstances;
        private ICollectionView allInstancesView;
        private JournalNumber selectedPoint;

        public JournalNumber SelectedPoint
        {
            get => selectedPoint;
            set
            {
                selectedPoint = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<JournalNumber> AllInstances
        {
            get => allInstances;
            set
            {
                allInstances = value;
                RaisePropertyChanged();
            }
        }

        public ICollectionView AllInstancesView
        {
            get => allInstancesView;
            set
            {
                allInstancesView = value;
                RaisePropertyChanged();
            }
        }

        public IAsyncCommand SaveItemsCommand { get; private set; }
        private async Task SaveItems()
        {
            try
            {
                IsBusy = true;
                await Task.Run(() => repo.Update(AllInstances));
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand AddNewItemCommand { get; private set; }
        private async Task AddNewItem()
        {
            try
            {
                IsBusy = true;
                await repo.AddAsync(new JournalNumber());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand UpdateListCommand { get; private set; }
        private async Task UpdateList()
        {
            try
            {
                IsBusy = true;
                AllInstances = await Task.Run(() => repo.GetAllAsync());
                AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);
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
                    await SaveItemsCommand.ExecuteAsync();
                }
            }
            else
            {
                bool check = true;
                if (check)
                {
                    try
                    {
                        IsBusy = true;
                        int value = await Task.Run(() => repo.Update(AllInstances));
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

        private bool CanExecute()
        {
            return true;
        }

        public static JournalNumbersViewModel LoadVM(DataContext context)
        {
            JournalNumbersViewModel vm = new JournalNumbersViewModel(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
        }

        public JournalNumbersViewModel(DataContext context)
        {
            db = context;
            repo = new JournalNumberRepository(db);
            CloseWindowCommand = new AsyncCommand<object>(CloseWindow);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem);
            SaveItemsCommand = new AsyncCommand(SaveItems);
        }
    }
}
