using BusinessLayer.Repository.Implementations.Entities;
using DataLayer;
using DataLayer.Journals;
using DataLayer.Journals.AssemblyUnits;
using DataLayer.Journals.Detailing;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Materials;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using DataLayer.Journals.Periodical;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Supervision.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Supervision.ViewModels
{
    public class JournalReportVM : BasePropertyChanged
    {
        private readonly DataContext db;
        private readonly InspectorRepository inspectorRepo;
        private IList<Inspector> inspectors;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<ResultJournalReport> consolidatedJournal;
        private string journalNumber;
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
        public IEnumerable<ResultJournalReport> ConsolidatedJournal
        {
            get => consolidatedJournal;
            set
            {
                consolidatedJournal = value;
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
        public string JournalNumber
        {
            get => journalNumber;
            set
            {
                journalNumber = value;
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
        private ObservableCollection<JournalReport> journal;
        public ObservableCollection<JournalReport> Journal
        {
            get => journal;
            set
            {
                journal = value;
                RaisePropertyChanged();
            }
        }

        private DateTime? date;
        private Inspector inspector;

        public Inspector Inspector
        {
            get => inspector;
            set
            {
                inspector = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? Date
        {
            get => date;
            set
            {
                date = value;
                RaisePropertyChanged();
            }
        }

        private string filePath; // = "C:\\Users\\Leeroy\\Desktop\\Programming\\Projects\\C#\\Supervision\\BaseJournalReport.xlsx";


        private void CreateFile(object obj)
        {
            IEnumerable<JournalReport> journalReport = obj as IEnumerable<JournalReport>;

            var e = journalReport.GroupBy(a => new
            {
                a.JournalNumber
            })
                .Select(i => new ResultJournalReport()
                {
                    JournalNumber = i.Key.JournalNumber,
                    ConsolidatedJournalReports = i.GroupBy(c => new
                    {
                        c.StringDate,
                        c.Name,
                        c.Point,
                        c.Description,
                        c.Engineer,
                        c.Comment,
                        c.Remark,
                        c.RemarkClosed,
                        c.Status,
                        c.PlaceOfControl,
                        c.Documents
                    })
                .Select(j => new ConsolidatedJournalReport()
                {
                    Date = j.Key.StringDate,
                    Name = j.Key.Name,
                    Point = j.Key.Point,
                    Description = j.Key.Description,
                    Engineer = j.Key.Engineer,
                    Comment = j.Key.Comment,
                    Remark = j.Key.Remark,
                    RemarkClosed = j.Key.RemarkClosed,
                    Status = j.Key.Status,
                    PlaceOfControl = j.Key.PlaceOfControl,
                    Documents = j.Key.Documents,
                    JournalReports = j.Select(a => a.Number).ToList()
                })
                });;

            ConsolidatedJournal = e;

            // Read Template
            using (FileStream templateDocumentStream = File.OpenRead("BaseJournalReport.xlsx"))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Create Excel EPPlus Package based on template stream
                using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                {
                    foreach (var a in ConsolidatedJournal)
                    {

                        // Grab the sheet with the template.
                        ExcelWorksheet sheet = package.Workbook.Worksheets.Add(String.IsNullOrEmpty(a.JournalNumber) ? "Без номера" : a.JournalNumber, package.Workbook.Worksheets["Report"]);
                        int recordIndex = 4;
                        sheet.Cells[1, 11].Value = a.JournalNumber;
                        foreach (var row in a.ConsolidatedJournalReports)
                        {
                            sheet.Row(recordIndex).CustomHeight = false;
                            string numbers = "";
                            sheet.Cells[recordIndex, 1].Value = (recordIndex - 3).ToString();
                            sheet.Cells[recordIndex, 2].Value = row.Date;
                            sheet.Cells[recordIndex, 3].Value = row.Point;
                            foreach (var i in row.JournalReports)
                            {
                                numbers += i + ";\n";
                            }
                            sheet.Cells[recordIndex, 4].Value = $"{row.Name}: {numbers}";
                            sheet.Cells[recordIndex, 5].Value = row.Description;
                            sheet.Cells[recordIndex, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                            sheet.Cells[recordIndex, 6].Value = row.PlaceOfControl;
                            sheet.Cells[recordIndex, 7].Value = row.Documents;
                            sheet.Cells[recordIndex, 8].Value = row.Status;
                            sheet.Cells[recordIndex, 9].Value = row.Remark;
                            sheet.Cells[recordIndex, 10].Value = row.RemarkClosed;
                            sheet.Cells[recordIndex, 11].Value = row.Engineer;
                            sheet.Cells[recordIndex, 12].Value = !String.IsNullOrWhiteSpace(row.Comment) ? row.Comment : "-";
                            recordIndex++;
                        }
                        sheet.PrinterSettings.PrintArea = sheet.Cells[1, 1, recordIndex - 1, 12];
                        sheet.Cells[2, 1, recordIndex - 1, 12].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells[2, 1, recordIndex - 1, 12].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells[2, 1, recordIndex - 1, 12].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        sheet.Cells[2, 1, recordIndex - 1, 12].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }
                    package.Workbook.Worksheets.MoveToEnd("Report");
                    package.Workbook.Worksheets["Report"].Hidden = eWorkSheetHidden.VeryHidden;

                    byte[] bin = package.GetAsByteArray();
                    filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Отчет ЖТН.xlsx";
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    File.WriteAllBytes(filePath, bin);
                    FileInfo fi = new FileInfo(filePath);
                }
            }
        }

        public ICommand OpenExcelReportCommand { get; private set; }
        private void OpenExcelReport()
        {
            if (filePath != null & File.Exists(filePath))
                Process.Start(filePath);
        }

        public ICommand PrintReportCommand { get; private set; }
        private void PrintReport()
        {
            if (filePath != null & File.Exists(filePath))
            {
                //TODO: Реализовать печать без открытия Excel файла

            }
        }

        
                    

        public IAsyncCommand CreateReportFileCommand { get; private set; }
        private async Task CreateReportFile()
        {
            try
            {
                IsBusy = true;
                var report = new ObservableCollection<JournalReport>();
                if (Inspector != null && Date != null)
                {
                    var insp = await Task.Run(() => GetInspectorRecordsAsync(Inspector.Id));

                    
                    foreach (var i in insp.CoatingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    foreach (var i in insp.SheetGateValveJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    foreach (var i in insp.CaseBottomJournals.Where(i => i.Date == Date &&  i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    
                    
                    foreach (var i in insp.CoverFlangeJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoverSleeveJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoverSleeve008Journals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.Ring043Journals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    //foreach (var i in insp.Ring047Journals.Where(i => i.Date == Date && i.DetailId != null))
                    //{
                    //    var temp = new JournalReport(i);
                    //    report.Add(temp);
                    //}
                    foreach (var i in insp.SheetGateValveCaseJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SheetGateValveCoverJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    
                    foreach (var i in insp.AssemblyUnitSealingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }

                    foreach (var i in insp.MainFlangeSealingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }

                    foreach (var i in insp.CounterFlangeJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    foreach (var i in insp.FrontalSaddleSealingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.GateJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.NozzleJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.RunningSleeveJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SaddleJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ScrewNutJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ScrewStudJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ShearPinJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SpindleJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ColumnJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SpringJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.AbovegroundCoatingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.AbrasiveMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.UndercoatJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.UndergroundCoatingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ControlWeldJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ForgingMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.PipeMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.RolledMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SheetMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.StoresControlJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.WeldingMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoatingChemicalCompositionJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoatingPlasticityJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoatingPorosityJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoatingProtectivePropertiesJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.DegreasingChemicalCompositionJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    //foreach (var i in insp.NDTControls.Where(i => i.Date == Date && i.DetailId != null))
                    //{
                    //    var temp = new JournalReport(i);
                    //    report.Add(temp);
                    //}
                    //foreach (var i in insp.WeldingProceduresJournals.Where(i => i.Date == Date && i.DetailId != null))
                    //{
                    //    var temp = new JournalReport(i);
                    //    report.Add(temp);
                    //}
                    foreach (var i in insp.FactoryInspectionJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.PIDJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    Journal = report;
                }

                if (Journal != null) CreateFile(Journal);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<Inspector> GetInspectorRecordsAsync(int id)
        {
            using (DataContext context = new DataContext())
            {
                var result = await context.Inspectors
                    .Include(i => i.PIDJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.Specification)
                    
                    
                    .Include(i => i.SheetGateValveJournals).ThenInclude(e => e.Entity)
                    
                    .Include(i => i.CoatingJournals).ThenInclude(e => e.Entity)
                    
                    .Include(i => i.CaseBottomJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)

                    
                    
                    .Include(i => i.CoverFlangeJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverSleeveJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverSleeve008Journals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.Ring043Journals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    //.Include(i => i.Ring047Journals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)

                    .Include(i => i.SheetGateValveCaseJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SheetGateValveCoverJournals).ThenInclude(e => e.Entity)
                    
                    
                    .Include(i => i.AssemblyUnitSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.MainFlangeSealingJournals).ThenInclude(e => e.Entity)

                    .Include(i => i.CounterFlangeJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.ForgingMaterial)

                    .Include(i => i.FrontalSaddleSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.GateJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.NozzleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.RunningSleeveJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SaddleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.ScrewNutJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ScrewStudJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ShearPinJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SpindleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.ColumnJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SpringJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.AbovegroundCoatingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.AbrasiveMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.UndercoatJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.UndergroundCoatingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ControlWeldJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ForgingMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.PipeMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.RolledMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SheetMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.StoresControlJournals)
                    .Include(i => i.WeldingMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoatingChemicalCompositionJournals)
                    .Include(i => i.CoatingPlasticityJournals)
                    .Include(i => i.CoatingPorosityJournals)
                    .Include(i => i.CoatingProtectivePropertiesJournals)
                    .Include(i => i.DegreasingChemicalCompositionJournals)
                    //.Include(i => i.NDTControls).ThenInclude(e => e.Entity)
                    //.Include(i => i.WeldingProceduresJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.FactoryInspectionJournals)
                    .FirstOrDefaultAsync(i => i.Id == id);
                return result;
            }
        }

        public IAsyncCommand CopyDbFileCommand { get; private set; }
        private void CopyDbFile()
        {
            File.Copy(@"T:\06-01-06 - БДУКП\СМТО ОП ПУТН\SupervisionData\SupervisionDataVol2.sqlite",
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\SupervisionDataVol2.sqlite",
                true);
        }

        public IAsyncCommand CreateDesktopReportFileCommand { get; private set; }
        private async Task CreateDesktopReportFile()
        {
            try
            {
                IsBusy = true;
#if DEBUG
#else
                CopyDbFile();
#endif
                var report = new ObservableCollection<JournalReport>();
                if (Inspector != null && Date != null)
                {
                    var insp = await Task.Run(() => GetDesktopInspectorRecordsAsync(Inspector.Id));

                    
                    foreach (var i in insp.CoatingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    foreach (var i in insp.SheetGateValveJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    foreach (var i in insp.CaseBottomJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    
                    
                    
                    foreach (var i in insp.CoverFlangeJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoverSleeveJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoverSleeve008Journals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.Ring043Journals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    //foreach (var i in insp.Ring047Journals.Where(i => i.Date == Date && i.DetailId != null))
                    //{
                    //    var temp = new JournalReport(i);
                    //    report.Add(temp);
                    //}
                    foreach (var i in insp.SheetGateValveCaseJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SheetGateValveCoverJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                   
                    
                    foreach (var i in insp.AssemblyUnitSealingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }

                    foreach (var i in insp.MainFlangeSealingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }

                    foreach (var i in insp.CounterFlangeJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    
                    foreach (var i in insp.FrontalSaddleSealingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.GateJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.NozzleJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.RunningSleeveJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SaddleJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ScrewNutJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ScrewStudJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ShearPinJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SpindleJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ColumnJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SpringJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.AbovegroundCoatingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.AbrasiveMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.UndercoatJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.UndergroundCoatingJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ControlWeldJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.ForgingMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.PipeMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.RolledMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.SheetMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.StoresControlJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.WeldingMaterialJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoatingChemicalCompositionJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoatingPlasticityJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoatingPorosityJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.CoatingProtectivePropertiesJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.DegreasingChemicalCompositionJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    //foreach (var i in insp.NDTControls.Where(i => i.Date == Date && i.DetailId != null))
                    //{
                    //    var temp = new JournalReport(i);
                    //    report.Add(temp);
                    //}
                    //foreach (var i in insp.WeldingProceduresJournals.Where(i => i.Date == Date && i.DetailId != null))
                    //{
                    //    var temp = new JournalReport(i);
                    //    report.Add(temp);
                    //}
                    foreach (var i in insp.FactoryInspectionJournals.Where(i => i.Date == Date))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    foreach (var i in insp.PIDJournals.Where(i => i.Date == Date && i.DetailId != null))
                    {
                        var temp = new JournalReport(i);
                        report.Add(temp);
                    }
                    Journal = report;
                }

                if (Journal != null) 
                    CreateFile(Journal);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<Inspector> GetDesktopInspectorRecordsAsync(int id)
        {
            using (DesktopDataContext context = new DesktopDataContext())
            {
                var result = await context.Inspectors
                    .Include(i => i.PIDJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.Specification)
                    
                    .Include(i => i.SheetGateValveJournals).ThenInclude(e => e.Entity)
                    
                    .Include(i => i.CoatingJournals).ThenInclude(e => e.Entity)
                    
                    .Include(i => i.CaseBottomJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)

                    
                    
                    .Include(i => i.CoverFlangeJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverSleeveJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverSleeve008Journals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.Ring043Journals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    //.Include(i => i.Ring047Journals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)

                    .Include(i => i.SheetGateValveCaseJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SheetGateValveCoverJournals).ThenInclude(e => e.Entity)
                    
                    
                    .Include(i => i.AssemblyUnitSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.MainFlangeSealingJournals).ThenInclude(e => e.Entity)

                    .Include(i => i.CounterFlangeJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.ForgingMaterial)

                    .Include(i => i.FrontalSaddleSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.GateJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.NozzleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.RunningSleeveJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SaddleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.ScrewNutJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ScrewStudJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ShearPinJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SpindleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.MetalMaterial)
                    .Include(i => i.ColumnJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SpringJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.AbovegroundCoatingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.AbrasiveMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.UndercoatJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.UndergroundCoatingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ControlWeldJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ForgingMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.PipeMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.RolledMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SheetMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.StoresControlJournals)
                    .Include(i => i.WeldingMaterialJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoatingChemicalCompositionJournals)
                    .Include(i => i.CoatingPlasticityJournals)
                    .Include(i => i.CoatingPorosityJournals)
                    .Include(i => i.CoatingProtectivePropertiesJournals)
                    .Include(i => i.DegreasingChemicalCompositionJournals)
                    //.Include(i => i.NDTControls).ThenInclude(e => e.Entity)
                    //.Include(i => i.WeldingProceduresJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.FactoryInspectionJournals)
                    .FirstOrDefaultAsync(i => i.Id == id);
                return result;
            }
        }

        public ICommand CloseWindowCommand { get; set; }
        private void CloseWindow(object obj)
        {
            Window w = obj as Window;
            w?.Close();
        }

        public Commands.IAsyncCommand LoadItemCommand { get; private set; }
        public async Task Load()
        {
            try
            {
                IsBusy = true;
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public static JournalReportVM LoadVM(DataContext context)
        {
            JournalReportVM vm = new JournalReportVM(context);
            vm.LoadItemCommand.ExecuteAsync();
            return vm;
        }

        public JournalReportVM(DataContext context)
        {
            db = context;
            inspectorRepo = new InspectorRepository(db);
            CreateReportFileCommand = new Commands.AsyncCommand(CreateReportFile);
            CreateDesktopReportFileCommand = new Commands.AsyncCommand(CreateDesktopReportFile);
            CloseWindowCommand = new Command(o => CloseWindow(o));
            OpenExcelReportCommand = new Command(o => OpenExcelReport());
            PrintReportCommand = new Command(o => PrintReport());
            LoadItemCommand = new Commands.AsyncCommand(Load);
        }
    }

    public class ResultJournalReport 
    {
        public string JournalNumber { get; set; }
        public IEnumerable<ConsolidatedJournalReport> ConsolidatedJournalReports { get; set; }
    }

    public class ConsolidatedJournalReport
    {
        public string Date { get; set; }
        public string Point { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Engineer { get; set; }
        public string Comment { get; set; }
        public string Remark { get; set; }
        public string RemarkClosed { get; set; }
        public string PlaceOfControl { get; set; }
        public string Documents { get; set; }
        public IList<string> JournalReports { get; set; }
    }

    public class JournalReport
    {
        public DateTime? Date { get; set; }
        public string StringDate { get; set; }
        public string Point { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Engineer { get; set; }
        public string Remark { get; set; }
        public string RemarkClosed { get; set; }
        public string PlaceOfControl { get; set; }
        public string Documents { get; set; }
        public string Comment { get; set; }
        public string JournalNumber { get; set; }

#region Constructors

        public JournalReport() {}

        

        public JournalReport(CoatingJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number = "№О"+journal.Entity.Number;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-"; 
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        

        

        public JournalReport(SheetGateValveJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.FullName;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        

        

        public JournalReport(CaseBottomJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        

        

        

        public JournalReport(CoverFlangeJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(CoverSleeveJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(CoverSleeve008Journal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport; 
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(Ring043Journal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        //public JournalReport(Ring047Journal journal)
        //{
        //    Date = journal.Date;
        //    StringDate = Date.Value.ToShortDateString();
        //    Point = journal.Point;
        //    Name = journal.Entity.Name;
        //    Number = journal.Entity.NameForReport;
        //    Description = journal.Description;
        //    Status = journal.Status;
        //    Engineer = journal.Inspector.FullName;
        //    Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
        //    RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
        //    Comment = journal.Comment;
        //    JournalNumber = journal.JournalNumber;
        //    PlaceOfControl = journal.PlaceOfControl;
        //    Documents = journal.Documents;
        //}

        public JournalReport(SheetGateValveCaseJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(SheetGateValveCoverJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        

        
        public JournalReport(AssemblyUnitSealingJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(MainFlangeSealingJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }



        public JournalReport(CounterFlangeJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        

        public JournalReport(FrontalSaddleSealingJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(GateJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(NozzleJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number = !String.IsNullOrWhiteSpace(journal.Entity.ZK) ? journal.Entity.NameForReport : "DN"+journal.Entity.DN + ", №"+journal.Entity.Number + ", пл."+journal.Entity.Melt + ", паспорт №"+ journal.Entity.Certificate;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(RunningSleeveJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(SaddleJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(ScrewNutJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =   journal.Entity.NameForReport + " " + journal.Entity.Thread + " - " + journal.Entity.Amount + " шт.";
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(ScrewStudJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =   journal.Entity.NameForReport + " " + journal.Entity.Thread + " - " + journal.Entity.Amount + " шт.";
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(ShearPinJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(SpindleJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(ColumnJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(SpringJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.Number + " - " + journal.Entity.Amount + " шт., " + "cерт." + journal.Entity.Certificate + ", парт." + journal.Entity.Batch;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(AbovegroundCoatingJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.Batch;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }
        public JournalReport(AbrasiveMaterialJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.Batch;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(UndercoatJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.Batch;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(UndergroundCoatingJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.Batch;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(ControlWeldJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.Number;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(SheetMaterialJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(PipeMaterialJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name;
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(RolledMaterialJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(ForgingMaterialJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number =  journal.Entity.NameForReport;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(StoresControlJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = "Хранение и складирование материалов";
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(WeldingMaterialJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = journal.Entity.Name; 
            Number = " № " + journal.Entity.Batch + " - " + journal.Entity.Amount;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(CoatingChemicalCompositionJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(CoatingPlasticityJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(CoatingPorosityJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(CoatingProtectivePropertiesJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(DegreasingChemicalCompositionJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        //public JournalReport(NDTControlJournal journal)
        //{
        //    Date = journal.Date;
        //    StringDate = Date.Value.ToShortDateString();
        //    Point = journal.Point;
        //    Name = journal.Entity.Name;
        //    Description = journal.Description;
        //    Status = journal.Status;
        //    Engineer = journal.Inspector.FullName;
        //    Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
        //    RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
        //    Comment = journal.Comment;
        //    JournalNumber = journal.JournalNumber;
        //}

        //public JournalReport(WeldingProceduresJournal journal)
        //{
        //    Date = journal.Date;
        //    StringDate = Date.Value.ToShortDateString();
        //    Point = journal.Point;
        //    Name = journal.Entity.Name;
        //    Description = journal.Description;
        //    Status = journal.Status;
        //    Engineer = journal.Inspector.FullName;
        //    Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
        //    RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
        //    Comment = journal.Comment;
        //    JournalNumber = journal.JournalNumber;
        //}

        public JournalReport(FactoryInspectionJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

        public JournalReport(PIDJournal journal)
        {
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Point = journal.Point;
            Name = "PID"; 
            Number = journal.Entity.Number + ";\nСпецификация № " + journal.Entity.Specification.Number;
            Description = journal.Description;
            Status = journal.Status;
            Engineer = journal.Inspector.FullName;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
            PlaceOfControl = journal.PlaceOfControl;
            Documents = journal.Documents;
        }

#endregion

    }
}
