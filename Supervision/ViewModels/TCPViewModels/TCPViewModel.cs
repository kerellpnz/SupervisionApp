using DataLayer;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using DataLayer.TechnicalControlPlans;
using Supervision.Commands;
using System.Threading.Tasks;
using BusinessLayer.Repository.Implementations.Entities;

namespace Supervision.ViewModels.TCPViewModels
{
    class TCPViewModel<TEntityTCP> : ViewModelBase
        where TEntityTCP : BaseTCP, new()
    {
        private readonly DataContext db;
        private readonly TCPRepository<TEntityTCP> repo;
        private readonly ProductTypeRepository productTypeRepo;
        private readonly OperationTypeRepository operationTypeRepo;
        private IEnumerable<ProductType> productTypes;
        private IEnumerable<OperationType> operationTypes;
        private IEnumerable<TEntityTCP> tCPs;
        private ICollectionView tCPsView;
        private TEntityTCP selectedPoint;

        public TEntityTCP SelectedPoint
        {
            get => selectedPoint;
            set
            {
                selectedPoint = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<TEntityTCP> TCPs
        {
            get => tCPs;
            set
            {
                tCPs = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<OperationType> OperationTypes
        {
            get => operationTypes;
            set
            {
                operationTypes = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<ProductType> ProductTypes
        {
            get => productTypes;
            set
            {
                productTypes = value;
                RaisePropertyChanged();
            }
        }

        public ICollectionView TCPsView
        {
            get => tCPsView;
            set
            {
                tCPsView = value;
                RaisePropertyChanged();
            }
        }

        public IAsyncCommand SaveItemsCommand { get; private set; }
        private async Task SaveItems()
        {
            try
            {
                IsBusy = true;
                await Task.Run(() => repo.Update(TCPs));
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
                SelectedPoint = await repo.AddAsync(new TEntityTCP());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand RemoveSelectedItemCommand { get; private set; }
        private async Task RemoveSelectedItem()
        {
            if (SelectedPoint != null)
            {
                MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        IsBusy = true;
                        await repo.RemoveAsync(SelectedPoint);
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
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
                        int value = await Task.Run(() => repo.Update(TCPs));
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

        public IAsyncCommand UpdateListCommand { get; private set; }
        private async Task UpdateList()
        {
            try
            {
                IsBusy = true;
                TCPs = await Task.Run(() => repo.GetAllAsync());
                ProductTypes = await Task.Run(() => productTypeRepo.GetAllAsync());
                OperationTypes = await Task.Run(() => operationTypeRepo.GetAllAsync());
                TCPsView = CollectionViewSource.GetDefaultView(TCPs);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public static TCPViewModel<T> LoadVM<T>(DataContext context) where T : BaseTCP, new()
        {
            TCPViewModel<T> vm = new TCPViewModel<T>(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
        }

        public TCPViewModel(DataContext context)
        {
            db = context;
            repo = new TCPRepository<TEntityTCP>(db);
            productTypeRepo = new ProductTypeRepository(db);
            operationTypeRepo = new OperationTypeRepository(db);
            CloseWindowCommand = new AsyncCommand<object>(CloseWindow);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem);
            RemoveSelectedItemCommand = new AsyncCommand(RemoveSelectedItem);
            SaveItemsCommand = new AsyncCommand(SaveItems);
        }
    }
}
