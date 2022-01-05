using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusinessLayer.Repository.Implementations.Entities;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using Supervision.Commands;


namespace Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve
{
    public class MainFlangeSealingEditVM : BasePropertyChanged
    {
        private readonly DataContext db;
        private readonly MainFlangeSealingRepository sealRepo;
        private readonly MainFlangeSealControlRepository sealControlRepo;
        private readonly InspectorRepository inspectorRepo;
        private readonly JournalNumberRepository journalRepo;

        private IEnumerable<string> journalNumbers;
        private IEnumerable<string> materials;
        private IEnumerable<string> drawings;
        private IEnumerable<MainFlangeSealingTCP> points;
        private IEnumerable<Inspector> inspectors;
        private readonly BaseTable parentEntity;
        private MainFlangeSealing selectedItem;
        private MainFlangeSealingTCP selectedTCPPoint;
        private IEnumerable<string> names;
        private MainFlangeSealControl sealControl;

        public MainFlangeSealControl SealControl
        {
            get => sealControl;
            set
            {
                sealControl = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<MainFlangeSealing> mainFlangeSeals;

        public IEnumerable<MainFlangeSealing> MainFlangeSeals
        {
            get => mainFlangeSeals;
            set
            {
                mainFlangeSeals = value;
                RaisePropertyChanged();
            }
        }


        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            protected set
            {
                isBusy = value;
                RaisePropertyChanged();
            }
        }

        private MainFlangeSealingJournal operation;
        public MainFlangeSealingJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }
        public MainFlangeSealing SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<MainFlangeSealingTCP> Points
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

        public IEnumerable<string> Names
        {
            get => names;
            set
            {
                names = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> Materials
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

        public MainFlangeSealingTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }



        public static MainFlangeSealingEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            MainFlangeSealingEditVM vm = new MainFlangeSealingEditVM(entity, context);
            vm.LoadItemCommand.ExecuteAsync(id);
            return vm;
        }

        public IAsyncCommand<int> LoadItemCommand { get; private set; }
        public async Task Load(int id)
        {
            try
            {
                IsBusy = true;
                MainFlangeSeals = await Task.Run(() => sealRepo.GetAllAsync());
                SealControl = await Task.Run(() => sealControlRepo.GetByIdIncludeAsync(1));
                SelectedItem = await Task.Run(() => sealRepo.GetByIdIncludeAsync(id));
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Materials = await Task.Run(() => sealRepo.GetPropertyValuesDistinctAsync(i => i.Material));
                Drawings = await Task.Run(() => sealRepo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Names = await Task.Run(() => sealRepo.GetPropertyValuesDistinctAsync(i => i.Name));
                Points = await Task.Run(() => sealRepo.GetTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand SaveItemCommand { get; private set; }
        private async Task Save()
        {
            try
            {
                IsBusy = true;
                await Task.Run(() => sealRepo.Update(SelectedItem));
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand AddOperationCommand { get; private set; }
        public async Task AddJournalOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.MainFlangeSealingJournals.Add(new MainFlangeSealingJournal()
                {
                    DetailDrawing = SelectedItem.Drawing,
                    DetailNumber = SelectedItem.Number,
                    DetailName = SelectedItem.Name,
                    DetailId = SelectedItem.Id,
                    Point = SelectedTCPPoint.Point,
                    Description = SelectedTCPPoint.Description,
                    PointId = SelectedTCPPoint.Id,
                });
                await SaveItemCommand.ExecuteAsync();
            }
        }


        public IAsyncCommand RemoveOperationCommand { get; private set; }
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
                        SelectedItem.MainFlangeSealingJournals.Remove(Operation);
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

        public IAsyncCommand<object> CloseWindowCommand { get; private set; }
        protected async Task CloseWindow(object obj)
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

                if (MainFlangeSeals.Count() % 10 == 0)
                    SelectedItem.isWatchingLab = true;

                if (SelectedItem.Certificate == null || SelectedItem.Certificate == "")
                {
                    MessageBox.Show("Не оставляйте сертификат пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if (!SelectedItem.Certificate.ToLower().Contains("удалить"))
                {
                    if (SelectedItem.Certificate != null && SelectedItem.Batch != null && SelectedItem.Drawing != null && SelectedItem.Name != null)
                    {
                        int count = 0;
                        foreach (MainFlangeSealing entity in MainFlangeSeals)
                        {
                            if (SelectedItem.Certificate.Equals(entity.Certificate) && SelectedItem.Batch.Equals(entity.Batch) && SelectedItem.Drawing.Equals(entity.Drawing) && SelectedItem.Name.Equals(entity.Name))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Уплотнения с такими сертификатными данными уже существуют! Действия сохранены не будут! В поле \"Сертификат\" напишите \"удалить\". Добавьте принятое количество к имеющимся уплотнениям. Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }
                    if (SelectedItem.Batch == null || SelectedItem.Batch == "")
                    {
                        MessageBox.Show("Не введена партия! Если этих данных нет в сертификате, ставьте \"-\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Drawing == null || SelectedItem.Drawing == "")
                    {
                        MessageBox.Show("Не введен чертеж! Если этих данных нет в сертификате, ставьте \"-\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Name == null || SelectedItem.Name == "")
                    {
                        MessageBox.Show("Не введено наименование уплотнения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                    if (SelectedItem.Amount == 0)
                    {
                        MessageBox.Show("Не введено количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        check = false;
                    }
                }

                if (SelectedItem.MainFlangeSealingJournals != null)
                {
                    foreach (MainFlangeSealingJournal journal in SelectedItem.MainFlangeSealingJournals)
                    {
                        if (journal.Status != null)
                        {
                            if (journal.Date == null)
                            {
                                MessageBox.Show("Не выбрана дата", "Ошибка");
                                check = false;
                            }
                            else if (journal.Description.Contains("аккредитованной") && journal.Status == "Не соответствует")
                            {
                                SelectedItem.Status = "НЕ СООТВ.";
                                flag = false;
                                SealControl.DateAfterRemark = journal.Date;
                                SealControl.DateForEndMode = Convert.ToDateTime(journal.Date).AddMonths(3);
                                await Task.Run(() => sealControlRepo.Update(SealControl));
                            }
                            else if (journal.Status == "Не соответствует")
                            {
                                SelectedItem.Status = "НЕ СООТВ.";
                                flag = false;
                            }
                        }
                    }
                }

                if (flag)
                {
                    SelectedItem.Status = "Cоотв.";
                }

                if (SealControl.DateForEndMode != null && SealControl.DateForEndMode > DateTime.Now)
                {
                    MessageBox.Show("Не забудьте про испытание в аккредитованной лаборатории!", "ВНИМАНИЕ!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (SelectedItem.isWatchingLab)
                {
                    MessageBox.Show("Данное уплотнение в программе является каждым десятым, что говорит о необходимости его испытания в аккредитованной лаборатории!", "ВНИМАНИЕ!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }                

                if (check)
                {
                    try
                    {
                        IsBusy = true;
                        if (SelectedItem.Amount > 0)
                            SelectedItem.AmountRemaining = SelectedItem.Amount - SelectedItem.BaseValveWithSeals.Count;
                        int value = await Task.Run(() => sealRepo.Update(SelectedItem));
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

        public MainFlangeSealingEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            sealRepo = new MainFlangeSealingRepository(context);
            sealControlRepo = new MainFlangeSealControlRepository(context);
            inspectorRepo = new InspectorRepository(context);
            journalRepo = new JournalNumberRepository(context);
            LoadItemCommand = new AsyncCommand<int>(Load);
            SaveItemCommand = new AsyncCommand(Save);
            CloseWindowCommand = new AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new AsyncCommand(AddJournalOperation);
            RemoveOperationCommand = new AsyncCommand(RemoveOperation);
        }
    }
}
