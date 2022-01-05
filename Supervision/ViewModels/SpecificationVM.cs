using System.Collections.Generic;
using DataLayer;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Supervision.Views.EntityViews;
using DataLayer.Journals;
using BusinessLayer.Repository.Implementations.Entities;
using Supervision.Commands;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Supervision.Views.FileWorkViews;
using System.Linq;
using OfficeOpenXml;
using System;
using System.IO;
using System.Diagnostics;

namespace Supervision.ViewModels
{
    public class SpecificationVM : ViewModelBase
    {
        private DataContext db;
        private readonly SpecificationRepository specificationRepo;
        private readonly CustomerRepository customerRepo;
        private readonly PIDRepository pIDRepo;
        private readonly ProductTypeRepository productTypeRepo;
        private IList<ProductType> productTypes;
        private IEnumerable<PID> pIDs;
        private IEnumerable<Specification> allInstances;
        private ICollectionView allInstancesView;
        private Specification selectedItem;
        private PID selectedPID;
        private IEnumerable<Customer> customers;
        private string number = "";       

        public string Number
        {
            get => number;
            set
            {
                number = value;
                RaisePropertyChanged();
                allInstancesView.Filter += (obj) =>
                {
                    if (obj is Specification item && item.Number != null)
                    {
                        return item.Number.ToLower().Contains(Number.ToLower());
                    }
                    else return true;
                };
            }
        }

        public IList<ProductType> ProductTypes
        {
            get => productTypes;
            set
            {
                productTypes = value;
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

        public Specification SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }
        public PID SelectedPID
        {
            get => selectedPID;
            set
            {
                selectedPID = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<Specification> AllInstances
        {
            get => allInstances;
            set
            {
                allInstances = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<Customer> Customers
        {
            get => customers;
            set
            {
                customers = value;
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

        public ICommand AddFileCommand { get; private set; }
        private void AddFile()
        {
//            try
//            {
//                IsBusy = true;
//                if (SelectedItem != null)
//                {

//                    OpenFileDialog dialog = new OpenFileDialog();
//                    bool? result = dialog.ShowDialog();
//                    if (result == true)
//                    {
//                        var fileName = dialog.FileName;
//                        var extension = Path.GetExtension(fileName);
//#if DEBUG
//                        DirectoryInfo dirInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\" + SelectedItem.Number);
//                        if (!dirInfo.Exists)
//                        {
//                            dirInfo.Create();
//                        }
//                        var newFileName = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\" + SelectedItem.Number + @"\" + SelectedItem.Number + extension;
//#else
//                        DirectoryInfo dirInfo = new DirectoryInfo(@"O:\38-00 - Челябинское УТН\38-04 - СМТО\Производство\Спецификации\" + SelectedItem.Number);
//                        if (!dirInfo.Exists)
//                        {
//                            dirInfo.Create();
//                        }
//                        var newFileName = @"O:\38-00 - Челябинское УТН\38-04 - СМТО\Производство\Спецификации\" + SelectedItem.Number + @"\" + SelectedItem.Number + extension;
//#endif
//                        if (File.Exists(newFileName))
//                            File.Delete(newFileName);
//                        File.Copy(fileName, newFileName, true);
//                        SelectedItem.FilePath = newFileName;
//                        SaveItemsCommand.ExecuteAsync();
                        
//                    }
//                }
//            }
//            finally
//            {
//                IsBusy = false;
//            }
        }        

        public ICommand EditPIDCommand { get; private set; }
        private void EditPID()
        {
            if (SelectedPID != null)
            {
                _ = new PIDEditView
                {
                    DataContext = PIDEditVM.LoadPIDEditVM(SelectedPID.Id, SelectedPID, db)
                };
                if (MainViewModel.HelpMode == true)
                {
                    MessageBox.Show("Поле \"Обозначение\" заполняется на основе данных из бумажного/цифрого носителя спецификации - столбца " +
                        "\"Наименование Продукции\".\nНе забывайте указывать срок поставки, количество и, при возможности, массу, так как " +
                        "эта информация пойдет в будущие отчеты по отгрузке. В поле \"Описание\" заносится информация из столбца \"Наименование Продукции\"" +
                        "полностью.\n\n" +
                        "Во вкладке \"Контроль\" в таблицу операции ПТК вносится фактическая информация, касаемая проверки данного PID (Дата контроля, Инженер, Статус, Номер журнала)\n" +
                        "Во вкладке \"Продукция\" отображаются все текущие ЗШ по данному PID после сборки.", "Раздел \"Редактирование PID\"");
                }
            }
        }

        public ICommand FileStorageOpenCommand { get; private set; }
        private void FileStorageOpen()
        {
            if (SelectedItem != null)
            {
                _ = new AddFileView
                {
                    DataContext = AddFileVM.LoadVM(db, SelectedItem)
                };
            }
        }

        private IEnumerable<NonCheckedPIDReport> nonCheckedPIDs;
        public IEnumerable<NonCheckedPIDReport> NonCheckedPIDs
        {
            get => nonCheckedPIDs;
            set
            {
                nonCheckedPIDs = value;
                RaisePropertyChanged();
            }
        }


        public class NonCheckedPIDReport
        {
            public string SpecificationNumber { get; set; }
            public IList<string> PIDNumbers { get; set; }
        }

        private void OpenNonCheckedPIDReport(IEnumerable<PID> report)
        {
            NonCheckedPIDs = report.GroupBy(a => new
            {
                a.Specification?.Number
            })
                .Select(j => new NonCheckedPIDReport
                {
                    SpecificationNumber = j.Key.Number ?? "Без номера",
                    PIDNumbers = j.Select(c => c.Number).ToList()
                }); ;
            
            if (NonCheckedPIDs != null)
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                // Create Excel EPPlus Package
                using (ExcelPackage package = new ExcelPackage())
                {
                    foreach (var i in NonCheckedPIDs)
                    {
                        ExcelWorksheet sheet = package.Workbook.Worksheets.Add(String.IsNullOrEmpty(i.SpecificationNumber) ? "Без номера" : i.SpecificationNumber);
                        sheet.PrinterSettings.PaperSize = ePaperSize.A4;
                        sheet.PrinterSettings.Orientation = eOrientation.Portrait;
                        sheet.Cells["A1:B1"].Merge = true;
                        sheet.Column(1).Width = 5.5;
                        sheet.Column(1).Style.Font.Name = "Franklin Gothic Book";
                        sheet.Column(1).Style.Font.Size = 12;
                        sheet.Column(1).Style.WrapText = true;
                        sheet.Column(2).Width = 100;
                        sheet.Column(2).Style.Font.Name = "Franklin Gothic Book";
                        sheet.Column(2).Style.Font.Size = 12;
                        sheet.Column(2).Style.WrapText = true;

                        sheet.Row(1).Style.Font.Bold = true;
                        sheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Row(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom;
                        sheet.Row(1).CustomHeight = false;

                        sheet.Row(2).Style.Font.Bold = true;
                        sheet.Row(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Row(2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        sheet.Row(2).CustomHeight = false;

                        sheet.Row(3).Style.Font.Bold = true;
                        sheet.Row(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet.Row(3).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        sheet.Row(3).CustomHeight = false;

                        sheet.Cells["A1"].Value = $"Перечень непроверенных PID в спецификации № {i.SpecificationNumber}";
                        sheet.Cells["A2"].Value = "№ п/п";
                        sheet.Cells["B2"].Value = "Номер PID";

                        sheet.Cells["A3"].Value = "1";
                        sheet.Cells["B3"].Value = "2";

                        int recordIndex = 4;
                        foreach (var row in i.PIDNumbers)
                        {
                            sheet.Row(recordIndex).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            sheet.Row(recordIndex).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            sheet.Row(recordIndex).CustomHeight = false;

                            sheet.Cells[recordIndex, 1].Value = (recordIndex - 3).ToString();
                            sheet.Cells[recordIndex, 2].Value = row ?? "Без номера";

                            sheet.Row(recordIndex).OutlineLevel = 1;
                            
                            recordIndex++;

                        }
                        sheet.PrinterSettings.PrintArea = sheet.Cells[1, 1, recordIndex, 2];
                        sheet.PrinterSettings.FitToPage = true;
                        sheet.PrinterSettings.FitToWidth = 1;
                        sheet.PrinterSettings.FitToHeight = 0;
                        sheet.Cells[2, 1, recordIndex, 2].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells[2, 1, recordIndex, 2].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells[2, 1, recordIndex, 2].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells[2, 1, recordIndex, 2].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.View.ZoomScale = 70;
                        sheet.View.PageBreakView = true;
                    }
                    byte[] bin = package.GetAsByteArray();
                    var filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\PIDs.xlsx";
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    File.WriteAllBytes(filePath, bin);
                    FileInfo fi = new FileInfo(filePath);
                    if (filePath != null & File.Exists(filePath))
                        Process.Start(filePath);
                }
            }
        }

        public IAsyncCommand GetNonCheckedCommand { get; private set; }
        private async Task GetNonChecked ()
        {
            try
            {
                IsBusy = true;
                var temp = await Task.Run(() => pIDRepo.GetNonChecked());
                OpenNonCheckedPIDReport(temp);
                
            }
            finally
            {
                IsBusy = false;
            }
        }



        public IAsyncCommand SaveItemsCommand { get; private set; }
        private async Task SaveItems()
        {
            try
            {                            
                IsBusy = true;
                await Task.Run(() => specificationRepo.Update(AllInstances));
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
                SelectedItem = await specificationRepo.AddAsync(new Specification());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand AddNewPIDCommand { get; private set; }
        private async Task AddNewPID()
        {
            try
            {
                IsBusy = true;
                if (SelectedItem != null && !String.IsNullOrWhiteSpace(SelectedItem.Number))
                {
                    if (!String.IsNullOrWhiteSpace(SelectedItem.Program))
                    {
                        if (SelectedItem.CustomerId != null)
                        {
                            int count = 0;
                            foreach (Specification spec in AllInstances)
                            {
                                if (SelectedItem.Number.Equals(spec.Number))
                                {
                                    count++;
                                    if (count > 1)
                                    {
                                        MessageBox.Show("Спецификация с таким номером уже существует!", "Ошибка");
                                    }
                                }
                            }
                            if (count < 2)
                            {
                                PID pid = new PID(SelectedItem);
                                pid.ProductType = ProductTypes[0];
                                pid.ProductTypeId = 1;
                                SelectedPID = await pIDRepo.AddAsync(pid);
                                var tcpPoints = await pIDRepo.GetTCPsAsync();
                                var records = new List<PIDJournal>();
                                foreach (var tcp in tcpPoints)
                                {
                                    var journal = new PIDJournal(SelectedPID, tcp);
                                    if (journal != null)
                                        records.Add(journal);
                                }
                                await pIDRepo.AddJournalRecordAsync(records);
                                EditPID();
                            }
                        }
                        else MessageBox.Show("Не выбран заказчик!", "Ошибка");
                    }
                    else MessageBox.Show("Не указана программа поставки!", "Ошибка");
                }
                else MessageBox.Show("Спецификация имеет пустой номер или не выбрана!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IAsyncCommand RemoveSelectedItemCommand { get; private set; }
        private async Task RemoveSelectedItem()
        {
            if (SelectedItem != null)
            {
                try
                {
                    IsBusy = true;
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        int deleteIndex = SelectedItem.PIDs.Count();
                        for (int i = deleteIndex-1; i >= 0; i--)
                        {
                            await pIDRepo.RemoveAsync(SelectedItem.PIDs[i]);
                        }
                        await specificationRepo.RemoveAsync(SelectedItem);
                    }  
                }
                finally
                {
                    IsBusy = false;
                }
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        public IAsyncCommand RemoveSelectedPIDCommand { get; private set; }
        private async Task RemoveSelectedPID()
        {
            if (SelectedPID != null)
            {
                try
                {
                    IsBusy = true;
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        await pIDRepo.RemoveAsync(SelectedPID);
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }


        public IAsyncCommand UpdateListCommand { get; private set; }
        private async Task UpdateList()
        {
            try
            {
                IsBusy = true;
                AllInstances = new ObservableCollection<Specification>();
                PIDs = await Task.Run(() => pIDRepo.GetAllAsync());                
                ProductTypes = await Task.Run(() => productTypeRepo.GetAllAsync());
                AllInstances = await Task.Run(() => specificationRepo.GetAllAsync());                
                Customers = await Task.Run(() => customerRepo.GetAllAsync());
                AllInstancesView = CollectionViewSource.GetDefaultView(AllInstances);                
            }
            finally
            {
                IsBusy = false;
            }
        }

        //public IAsyncCommand UpdatePIDStatsCommand { get; private set; }
        //private async Task UpdatePIDStats()
        //{
        //    if (SelectedItem != null)
        //    {
        //        foreach (PID pid in SelectedItem.PIDs)
        //        {
        //            try
        //            {
        //                IsBusy = true;
        //                PID pidFromRepo = await Task.Run(() => pIDRepo.GetByIdIncludeAsync(pid.Id));
        //                pIDRepo.SetShippedProductAsync(pidFromRepo);
        //                await Task.Run(() => pIDRepo.Update(pidFromRepo));
        //            }
        //            finally
        //            {
        //                IsBusy = false;
        //            }
        //        }
        //    }
        //}

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
                if (SelectedItem != null && SelectedItem.Number != null)
                {
                    int count = 0;
                    foreach (Specification spec in AllInstances)
                    {
                        if (SelectedItem.Number.Equals(spec.Number))
                        {
                            count++;
                            if (count > 1)
                            {
                                MessageBox.Show("Спецификация с таким номером уже существует!", "Ошибка");
                                count = 0;
                                check = false;
                            }
                        }
                    }
                }

                if (check)
                {
                    try
                    {
                        IsBusy = true;
                        int value = await Task.Run(() => specificationRepo.Update(AllInstances));
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

        public static SpecificationVM LoadSpecificationVM(DataContext context)
        {
            SpecificationVM vm = new SpecificationVM(context);
            vm.UpdateListCommand.ExecuteAsync();
            return vm;
        }

        public SpecificationVM(DataContext context)
        {
            db = context;
            specificationRepo = new SpecificationRepository(db);
            productTypeRepo = new ProductTypeRepository(db);
            customerRepo = new CustomerRepository(db);
            pIDRepo = new PIDRepository(db);
            CloseWindowCommand = new AsyncCommand<object>(CloseWindow);
            UpdateListCommand = new AsyncCommand(UpdateList, CanExecute);
            //UpdatePIDStatsCommand = new AsyncCommand(UpdatePIDStats, CanExecute);
            AddNewItemCommand = new AsyncCommand(AddNewItem);
            AddNewPIDCommand = new AsyncCommand(AddNewPID);
            RemoveSelectedItemCommand = new AsyncCommand(RemoveSelectedItem);
            RemoveSelectedPIDCommand = new AsyncCommand(RemoveSelectedPID);
            SaveItemsCommand = new AsyncCommand(SaveItems);
            EditPIDCommand = new Command(_ => EditPID());            
            FileStorageOpenCommand = new Command(_ => FileStorageOpen());
            AddFileCommand = new Command(_ => AddFile());
            GetNonCheckedCommand = new AsyncCommand(GetNonChecked);
        }
    }
}
