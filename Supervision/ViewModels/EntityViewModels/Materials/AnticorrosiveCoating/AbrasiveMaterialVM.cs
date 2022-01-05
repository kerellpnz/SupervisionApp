﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities.Material;
using DataLayer;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using Supervision.Commands;
using Supervision.Views.EntityViews.MaterialViews.AnticorrosiveCoating;

namespace Supervision.ViewModels.EntityViewModels.Materials.AnticorrosiveCoating
{
    public class AbrasiveMaterialVM : ViewModelBase
    {
        private readonly DataContext db;
        private readonly AbrasiveMaterialRepository repo;
        private IEnumerable<AbrasiveMaterial> allInstances;
        private ICollectionView allInstancesView;
        private AbrasiveMaterial selectedItem;

        private string number = "";
        private string status = "";
        private string certificate = "";
        private string factory = "";
        private string batch = "";

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
                    if (obj is AbrasiveMaterial item && item.Name != null)
                    {
                        return item.Name.ToLower().Contains(Number.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Status
        {
            get => status;
            set
            {
                status = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is AbrasiveMaterial item && item.Status != null)
                    {
                        return item.Status.ToLower().Contains(Status.ToLower());
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
                    if (obj is AbrasiveMaterial item && item.Certificate != null)
                    {
                        return item.Certificate.ToLower().Contains(Certificate.ToLower());
                    }
                    else return true;
                };
            }
        }
        public string Factory
        {
            get => factory;
            set
            {
                factory = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is AbrasiveMaterial item && item.Factory != null)
                    {
                        return item.Factory.ToLower().Contains(Factory.ToLower());
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
                    if (obj is AbrasiveMaterial item && item.Batch != null)
                    {
                        return item.Batch.ToLower().Contains(Batch.ToLower());
                    }
                    else return true;
                };
            }
        }
        #endregion

        private DateTime? startDate = null;
        private DateTime? endDate = null;
        public DateTime? StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                FilteringPeriod();
                RaisePropertyChanged("StartDate");
            }
        }

        public DateTime? EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                FilteringPeriod();
                RaisePropertyChanged("EndDate");
            }
        }

        private void FilteringPeriod()
        {
            if (StartDate != null && EndDate != null) AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances.Where(e => e.InputControlDate >= StartDate && e.InputControlDate <= EndDate));
            else if (StartDate != null) AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances.Where(e => e.InputControlDate >= StartDate));
            else if (EndDate != null) AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances.Where(e => e.InputControlDate <= EndDate));
            else AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);
        }

        public AbrasiveMaterial SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<AbrasiveMaterial> AllInstances
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

        public IAsyncCommand AddNewItemCommand { get; private set; }
        private async Task AddNewItem()
        {
            try
            {
                IsBusy = true;
                SelectedItem = await repo.AddAsync(new AbrasiveMaterial());
                var tcpPoints = await repo.GetTCPsAsync();
                var records = new List<AbrasiveMaterialJournal>();
                foreach (var tcp in tcpPoints)
                {
                    var journal = new AbrasiveMaterialJournal(SelectedItem, tcp);
                    if (journal != null)
                        records.Add(journal);
                }
                await repo.AddJournalRecordAsync(records);
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
                    var temp = await repo.GetByIdIncludeAsync(SelectedItem.Id);
                    var copy = await repo.AddAsync(new AbrasiveMaterial(temp));
                    var jour = new ObservableCollection<AbrasiveMaterialJournal>();
                    if (temp.AbrasiveMaterialJournals != null)
                    {
                        foreach (var i in temp.AbrasiveMaterialJournals)
                        {
                            var record = new AbrasiveMaterialJournal(copy.Id, i);
                            jour.Add(record);
                        }
                        repo.UpdateJournalRecord(jour);
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
                _ = new BaseAnticorrosiveCoatingEditView
                {
                    DataContext = AbrasiveMaterialEditVM.LoadVM(SelectedItem.Id, SelectedItem, db)
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
                        "Вкладка \"Применяемость\" содержит таблицу ЗШ, в которых была применена текущая партия АКП.\n\n" +
                        "После заполнения нужных полей и закрытия операций, необходимо нажать кнопку \"Сохранить\".", "Раздел \"АКП\"");
                }
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        private bool CanExecute()
        {
            return true;
        }

        public static AbrasiveMaterialVM LoadVM(DataContext context)
        {
            AbrasiveMaterialVM vm = new AbrasiveMaterialVM(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
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

        public AbrasiveMaterialVM(DataContext context)
        {
            db = context;
            repo = new AbrasiveMaterialRepository(db);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem, CanExecute);
            CopySelectedItemCommand = new AsyncCommand(CopySelectedItem, CanExecute);
            EditSelectedItemCommand = new Command(o => EditSelectedItem());
        }
    }
}
