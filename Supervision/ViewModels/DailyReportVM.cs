using DataLayer;
using DataLayer.Entities.AssemblyUnits;
using Microsoft.EntityFrameworkCore;
using Supervision.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Supervision.ViewModels
{
    public class DailyReportVM : BasePropertyChanged
    {
        private readonly DataContext db;
        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<ProductType> productTypes;
        public IEnumerable<ProductType> ProductTypes
        {
            get => productTypes;
            set
            {
                productTypes = value;
                RaisePropertyChanged();
            }
        }
        private ProductType selectedProductType;
        public ProductType SelectedProductType
        {
            get => selectedProductType;
            set
            {
                selectedProductType = value;
                RaisePropertyChanged();
            }
        }
        
        private IList<DailyReport> sheetGateValves;
        public IList<DailyReport> SheetGateValves
        {
            get => sheetGateValves;
            set
            {
                sheetGateValves = value;
                RaisePropertyChanged();
            }
        }
        private ICollectionView sheetGateValvesView;
        public ICollectionView SheetGateValvesView
        {
            get => sheetGateValvesView;
            set
            {
                sheetGateValvesView = value;
                RaisePropertyChanged();
            }
        }
        private async Task<IEnumerable<SheetGateValve>> GetAllSheetGateValveData()
        {
            return await db.SheetGateValves.Include(i => i.PID)
                .ThenInclude(i => i.Specification)
                .ThenInclude(i => i.Customer)
                .Include(i => i.Gate)
                .Include(i => i.SheetGateValveJournals)
                .ThenInclude(i => i.EntityTCP)
                .ThenInclude(i => i.OperationType)
                .Include(i => i.CoatingJournals)
                .ThenInclude(i => i.EntityTCP)
                .ThenInclude(i => i.OperationType).ToListAsync();
        }
        private async Task Report()
        {
            try
            {
                IsBusy = true;
                await Task.Run(() => GetAllSheetGateValveData().ContinueWith(task =>
                {
                    if (task.Exception == null)
                    {
                        var result = task.Result;
                        SheetGateValves = new List<DailyReport>();
                        foreach (var i in result)
                        {
                            var record = new DailyReport(i);
                            SheetGateValves.Add(record);
                        }
                        SheetGateValvesView = CollectionViewSource.GetDefaultView(SheetGateValves);
                    }
                }));
            }
            finally
            {
                IsBusy = false;
            }
        }
        public IAsyncCommand GetReportCommand { get; private set; }

        public ICommand CloseWindowCommand { get; set; }
        private void CloseWindow(object obj)
        {
            Window w = obj as Window;
            w?.Close();
        }

        public DailyReportVM(DataContext context)
        {
            db = context;
            GetReportCommand = new AsyncCommand(Report);
            CloseWindowCommand = new Command(o => CloseWindow(o));
        }
    }

    public class DailyReport
    {
        public string CustomerName { get; set; }
        public string SpecificationNumber { get; set; }
        public string PIDNumber { get; set; }
        public string Facility { get; set; }
        public string Designation { get; set; }
        public string GateNumber { get; set; }
        public string UnitNumber { get; set; }
        public DateTime? AssemblyDate { get; set; }
        public string StringAssemblyDate { get; set; }
        public DateTime? TestDate { get; set; }
        public string StringTestDate { get; set; }
        public DateTime? CoatingDate { get; set; }
        public string StringCoatingDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public string StringShippingDate { get; set; }
        public string Remark { get; set; }
        public string RemarkClosed { get; set; }

        public DailyReport()
        {

        }

        public DailyReport(SheetGateValve valve)
        {
            CustomerName = valve.PID?.Specification?.Customer?.Name;
            SpecificationNumber = valve.PID?.Specification?.Number;
            PIDNumber = valve.PID?.Number;
            Facility = valve.PID?.Specification?.Facility;
            Designation = valve.PID?.Designation;
            UnitNumber = $"№О"+valve.Number;
            GateNumber = valve.Gate?.Number;
            AssemblyDate = valve.SheetGateValveJournals?.Where(e => e.Point == "8.1").Select(a => a.Date).Max().GetValueOrDefault().Date;
            TestDate = valve.SheetGateValveJournals?.Where(e => e.Point == "8.14").Select(a => a.Date).Max().GetValueOrDefault().Date;
            CoatingDate = valve.CoatingJournals?.Where(e => e.Point == "9 (АКП)" && e.Description.Contains("адгез")).Select(a => a.Date).Max().GetValueOrDefault().Date;

            //if (!String.IsNullOrWhiteSpace(valve.AutoNumber) || !String.IsNullOrEmpty(valve.AutoNumber))
            //{
            //    ShippingDate = valve.SheetGateValveJournals?.Where(e => e.EntityTCP.OperationType.Name == "Отгрузка").Select(a => a.Date).Max().GetValueOrDefault().Date;
            //    StringShippingDate = ShippingDate.Value == Convert.ToDateTime("01.01.0001") ? "-" : ShippingDate.Value.ToShortDateString();
            //}

            ShippingDate = valve.SheetGateValveJournals?.Where(e => e.EntityTCP.OperationType.Name == "Отгрузка").Select(a => a.Date).Max().GetValueOrDefault().Date;
            StringShippingDate = ShippingDate.Value == Convert.ToDateTime("01.01.0001") ? "-" : ShippingDate.Value.ToShortDateString();

            StringAssemblyDate = AssemblyDate.Value == Convert.ToDateTime("01.01.0001") ? "-" : AssemblyDate.Value.ToShortDateString();
            StringTestDate = TestDate.Value == Convert.ToDateTime("01.01.0001") ? "-" : TestDate.Value.ToShortDateString();
            StringCoatingDate = CoatingDate.Value == Convert.ToDateTime("01.01.0001") ? "-" : CoatingDate.Value.ToShortDateString();
        }
    }
}
