using DataLayer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DataLayer.Entities.Materials;
using Supervision.Views.EntityViews.MaterialViews;
using DataLayer.Journals.Materials;
using BusinessLayer.Repository.Implementations.Entities.Material;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Supervision.Commands;

namespace Supervision.ViewModels.EntityViewModels.Materials
{
    public class ForgingMaterialVM : ViewModelBase
    {
        private readonly DataContext db;
        private readonly ForgingMaterialRepository forgingRepo;
        private IEnumerable<ForgingMaterial> allInstances;
        private ICollectionView allInstancesView;
        private ForgingMaterial selectedItem;

        private string name;
        private string number = "";
        private string drawing = "";
        private string batch = "";
        private string material = "";
        private string certificate = "";
        private string melt = "";
        private string target = "";

        #region Filter
        public string Number 
        {
            get => number;
            set
            {
                number = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is ForgingMaterial item && item.Number != null)
                    {
                        return item.Number.ToLower().Contains(Number.ToLower());
                    }
                    else return true;
                };
            }
        }

        public string Drawing
        {
            get => drawing;
            set
            {
                drawing = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is ForgingMaterial item && item.Drawing != null)
                    {
                        return item.Drawing.ToLower().Contains(Drawing.ToLower());
                    }
                    else return true;
                };
            }
        }

        public string Batch
        {
            get => batch;
            set
            {
                batch = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is ForgingMaterial item && item.Batch != null)
                    {
                        return item.Batch.ToLower().Contains(Batch.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Material
        {
            get => material;
            set
            {
                material= value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is ForgingMaterial item && item.Material != null)
                    {
                        return item.Material.ToLower().Contains(Material.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Certificate
        {
            get => certificate;
            set
            {
                certificate = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is ForgingMaterial item && item.Certificate != null)
                    {
                        return item.Certificate.ToLower().Contains(Certificate.ToLower());
                    }
                    else return true;
                };
            }
        }

        public string Target
        {
            get => target;
            set
            {
                target = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is ForgingMaterial item && item.Target != null)
                    {
                        return item.Target.ToLower().Contains(Target.ToLower());
                    }
                    else return true;
                };
            }
        }

        public string Melt
        {
            get => melt;
            set
            {
                melt = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is ForgingMaterial item && item.Melt != null)
                    {
                        return item.Melt.ToLower().Contains(Melt.ToLower());
                    }
                    else return true;
                };
            }
        }
        #endregion

        public string Name
        {
            get => name;
            set
            {
                name = value;
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
        public IEnumerable<ForgingMaterial> AllInstances
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

        public static ForgingMaterialVM LoadVM(DataContext context)
        {
            ForgingMaterialVM vm = new ForgingMaterialVM(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
        }

        public IAsyncCommand UpdateListCommand { get; private set; }
        private async Task UpdateList()
        {
            try
            {
                IsBusy = true;
                AllInstances = new ObservableCollection<ForgingMaterial>();
                AllInstances = await Task.Run(() => forgingRepo.GetAllAsync());
                AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);
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
                SelectedItem = await forgingRepo.AddAsync(new ForgingMaterial());
                var tcpPoints = await forgingRepo.GetTCPsAsync();
                var records = new List<ForgingMaterialJournal>();
                foreach (var tcp in tcpPoints)
                {
                    var journal = new ForgingMaterialJournal(SelectedItem, tcp);
                    if (journal != null)
                        records.Add(journal);
                }
                await forgingRepo.AddJournalRecordAsync(records);
                EditSelectedItem();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand CopySelectedItemCommand { get; private set; }
        private async Task CopySelectedItem()
        {
            if (SelectedItem != null)
            {
                try
                {
                    IsBusy = true;
                    string Input = Microsoft.VisualBasic.Interaction.InputBox("Введите количество копий, нумерация продолжится автоматически относительно выбранной детали:");
                    
                    if (System.Int32.TryParse(Input, out int x))
                    {
                        // you know that the parsing attempt
                        // was successful
                        if (x > 20)
                        {
                            MessageBox.Show("Установлено ограничение не больше 20 шт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            var temp = await forgingRepo.GetByIdIncludeAsync(SelectedItem.Id);
                            ForgingMaterial detail = new ForgingMaterial(temp);
                            if (System.Int32.TryParse(detail.Number, out int numb))
                            {
                                for (int y = 0; y < x; y++)
                                {
                                    numb += 1;
                                    detail.Number = numb.ToString();
                                    var copy = await forgingRepo.AddAsync(new ForgingMaterial(detail));
                                    var jour = new ObservableCollection<ForgingMaterialJournal>();
                                    if (temp.ForgingMaterialJournals != null)
                                    {
                                        foreach (var i in temp.ForgingMaterialJournals)
                                        {
                                            var record = new ForgingMaterialJournal(copy.Id, i);
                                            jour.Add(record);
                                        }
                                        int value = await Task.Run(() => forgingRepo.UpdateJournalRecord(jour));
                                        if (value == 0)
                                        {
                                            MessageBox.Show("При копировании произошла ошибка сервера. Проверьте целостность данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }                                    
                                }
                                detail = null;
                            }
                            else
                            {
                                MessageBox.Show("В поле Номер установлено некорректное значение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введено некорректное значение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public IAsyncCommand RemoveSelectedItemCommand { get; private set; }

        public ICommand EditSelectedItemCommand { get; private set; }
        private void EditSelectedItem()
        {
            if (SelectedItem != null)
            {
                _ = new ForgingMaterialEditView
                {
                    DataContext = ForgingMaterialEditVM.LoadVM(SelectedItem.Id, SelectedItem, db)
                };
                if (MainViewModel.HelpMode == true)
                {
                    MessageBox.Show("Поля внутри данного раздела заполняются в соответствии с их описанием, без добавления \"серт.\", " +
                        "\"партия\", \"шт.\", \"№\" и т.п. внутри поля.\n\nВкладка \"Операции\" содержит все операции ПТК относящиеся к текущей детали. " +
                        "Чтобы полностью закрыть операцию нужно ввести дату, фамилию, статус, выбрать журнал ТН, написать номер " +
                        "замечания, если необходимо.\n" +
                        "Если внутри данного раздела не оказалось требуемых операций ПТК доступных для заполнения, выберете ее из выпадающего списка " +
                        "\"Выбор пункта\" и нажмите кнопку \"Добавить операцию\". Если после этого необходимая операция ПТК по-прежнему не отобразилась на экране " +
                        "обратитесь к администратору.\n\n" +
                        "Текст набранный в поле \"Примечание\" внутри каждой операции отобразится в журнале при печати ежедневного отчета, " +
                        "текст в поле \"Примечание\" внизу данного раздела, останется внутри текущей детали и будет являться дополнительной информацией к ней.\n\n" +
                        
                        "После заполнения нужных полей и закрытия операций, необходимо нажать кнопку \"Сохранить\".\n\n" +
                        "Если поковок/отливок по сертификату несколько, внимательно заполняйте информацию в данном разделе " +
                    "и не забудьте добавить примечание при необходимости. В поле \"Номер\" установите цифру \"1\", нажмите кнопку сохранить и закройте данный раздел. Далее выберете созданный вами объект и нажмите кнопку " +
                    "\"Копировать\". Введите количество, принятых вами поковок/отливок, отняв единицу, программа скопирует всю информацию из выбранного первого объекта и автоматически создаст и пронумерует оставшиеся. " +
                    "Если при создании первой поковки/отливки была допущена ошибка, исправлять ее придется уже в каждой. Такой способ добавления поковок/отливок необходим для привязки каждой отдельной единицы к своей будущей детали" +
                    " и их учета.", "Раздел Поковка/Отливка");
                }                
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        protected override void CloseWindow(object obj)
        {
            Window w = obj as Window;
            w?.Close();
        }

        private bool CanExecute()
        {
            return true;
        }

        public ForgingMaterialVM(DataContext context)
        {
            db = context;
            forgingRepo = new ForgingMaterialRepository(db);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem, CanExecute);
            CopySelectedItemCommand = new AsyncCommand(CopySelectedItem, CanExecute);
            EditSelectedItemCommand = new Command(o => EditSelectedItem());
            CloseWindowCommand = new Command(o => CloseWindow(o));
        }






        //public string Name
        //{
        //    get => name;
        //    set
        //    {
        //        name = value;
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

        //public IEnumerable<ForgingMaterial> AllInstances
        //{
        //    get => allInstances;
        //    set
        //    {
        //        allInstances = value;
        //        RaisePropertyChanged();
        //    }
        //}
        //public ICollectionView AllInstancesView
        //{
        //    get => allInstancesView;
        //    set
        //    {
        //        allInstancesView = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public IAsyncCommand AddNewItemCommand { get; private set; }
        //private async Task AddNewItem()
        //{
        //    try
        //    {
        //        IsBusy = true;
        //        SelectedItem = await forgingRepo.AddAsync(new ForgingMaterial());
        //        var tcpPoints = await forgingRepo.GetTCPsAsync();
        //        var records = new List<ForgingMaterialJournal>();
        //        foreach (var tcp in tcpPoints)
        //        {
        //            var journal = new ForgingMaterialJournal(SelectedItem, tcp);
        //            if (journal != null)
        //                records.Add(journal);
        //        }
        //        await forgingRepo.AddJournalRecordAsync(records);
        //        EditSelectedItem();
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}

        //public IAsyncCommand CopySelectedItemCommand { get; private set; }
        //private async Task CopySelectedItem()
        //{
        //    if (SelectedItem != null)
        //    {
        //        try
        //        {
        //            IsBusy = true;
        //            var temp = await forgingRepo.GetByIdIncludeAsync(SelectedItem.Id);
        //            var copy = await forgingRepo.AddAsync(new ForgingMaterial(temp));
        //            var jour = new ObservableCollection<ForgingMaterialJournal>();
        //            foreach (var i in temp.ForgingMaterialJournals)
        //            {
        //                var record = new ForgingMaterialJournal(copy.Id, i);
        //                jour.Add(record);
        //            }
        //            forgingRepo.UpdateJournalRecord(jour);
        //        }
        //        finally
        //        {
        //            IsBusy = false;
        //        }
        //    }
        //}

        //public IAsyncCommand GroupCopySelectedItemCommand { get; private set; }
        //private async Task GroupCopySelectedItem()
        //{
        //    if (SelectedItem != null)
        //    {
        //        try
        //        {
        //            IsBusy = true;
        //            string Input = Microsoft.VisualBasic.Interaction.InputBox("Введите количество копий, нумерация продолжится автоматически относительно выбранной детали:");
        //            int x = 0;
        //            if (System.Int32.TryParse(Input, out x))
        //            {
        //                // you know that the parsing attempt
        //                // was successful
        //                if (x > 20)
        //                {
        //                    MessageBox.Show("Установлено ограничение не больше 20 шт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //                }
        //                else
        //                {
        //                    var temp = await forgingRepo.GetByIdIncludeAsync(SelectedItem.Id);
        //                    ForgingMaterial detail = new ForgingMaterial(temp);
        //                    int numb = 0;
        //                    if (System.Int32.TryParse(detail.Number, out numb))
        //                    {
        //                        for (int y = 0; y < x; y++)
        //                        {
        //                            numb += 1;
        //                            detail.Number = numb.ToString();
        //                            var copy = await forgingRepo.AddAsync(new ForgingMaterial(detail));
        //                            var jour = new ObservableCollection<ForgingMaterialJournal>();
        //                            foreach (var i in temp.ForgingMaterialJournals)
        //                            {
        //                                var record = new ForgingMaterialJournal(copy.Id, i);
        //                                jour.Add(record);
        //                            }
        //                            forgingRepo.UpdateJournalRecord(jour);
        //                        }
        //                        detail = null;
        //                    }
        //                    else
        //                    {
        //                        MessageBox.Show("В поле Номер установлено некорректное значение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show("Введено некорректное значение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //        finally
        //        {
        //            IsBusy = false;
        //        }
        //    }
        //}

        //public IAsyncCommand RemoveSelectedItemCommand { get; private set; }

        //public ICommand EditSelectedItemCommand { get; private set; }
        //private void EditSelectedItem()
        //{
        //    if (SelectedItem != null)
        //    {
        //        _ = new ForgingMaterialEditView
        //        {
        //            DataContext = ForgingMaterialEditVM.LoadForgingMaterialEditVM(SelectedItem.Id, SelectedItem, db)
        //        };
        //    }
        //    else MessageBox.Show("Объект не выбран", "Ошибка");
        //}

        //private bool CanExecute()
        //{
        //    return true;
        //}

        //public static ForgingMaterialVM LoadForgingMaterialVM(DataContext context)
        //{
        //    ForgingMaterialVM vm = new ForgingMaterialVM(context);
        //    vm.UpdateListCommand.ExecuteAsync();
        //    return vm;
        //}

        //public IAsyncCommand UpdateListCommand { get; private set; }
        //private async Task UpdateList()
        //{
        //    try
        //    {
        //        IsBusy = true;
        //        AllInstances = new ObservableCollection<ForgingMaterial>();
        //        AllInstances = await Task.Run(() => forgingRepo.GetAllAsync());
        //        AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}

        //public ForgingMaterialVM(DataContext context)
        //{
        //    db = context;
        //    forgingRepo = new ForgingMaterialRepository(db);
        //    UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
        //    AddNewItemCommand = new AsyncCommand(AddNewItem, CanExecute);
        //    CopySelectedItemCommand = new AsyncCommand(CopySelectedItem, CanExecute);
        //    GroupCopySelectedItemCommand = new AsyncCommand(GroupCopySelectedItem, CanExecute);
        //    EditSelectedItemCommand = new Command(o => EditSelectedItem());
        //}
    }
}
