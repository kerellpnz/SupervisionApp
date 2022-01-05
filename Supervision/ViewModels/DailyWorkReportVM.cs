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
    public class DailyWorkReportVM : BasePropertyChanged
    {
        private readonly DataContext db;
        private readonly InspectorRepository inspectorRepo;
        private readonly PIDRepository PIDRepo;
        private IList<Inspector> inspectors;
        private IList<PID> pIDs;
        private IEnumerable<string> journalNumbers;
        private IEnumerable<ConsolidatedDailyWorkReport> consolidatedDailyWorkReports;
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
        public IEnumerable<ConsolidatedDailyWorkReport> ConsolidatedDailyWorkReports
        {
            get => consolidatedDailyWorkReports;
            set
            {
                consolidatedDailyWorkReports = value;
                RaisePropertyChanged();
            }
        }

        public IList<PID> PIDs
        {
            get => pIDs;
            set
            {
                pIDs = value;
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
        private ObservableCollection<DailyWorkReport> journal;
        public ObservableCollection<DailyWorkReport> Journal
        {
            get => journal;
            set
            {
                journal = value;
                RaisePropertyChanged();
            }
        }

        private DateTime? startDate;
        private DateTime? endDate;
        private Inspector inspector;
        private PID pID;

        public PID PID
        {
            get => pID;
            set
            {
                pID = value;
                RaisePropertyChanged();
            }
        }

        public Inspector Inspector
        {
            get => inspector;
            set
            {
                inspector = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                RaisePropertyChanged();
            }
        }

        private string filePath; 


        private void CreateFile(object obj)
        {
            IEnumerable<DailyWorkReport> DailyWorkReport = obj as IEnumerable<DailyWorkReport>;

            var e = DailyWorkReport.GroupBy(a => new
            {
                a.DN,
                a.PID,
                a.Customer,
                a.Point,
                a.Description,
                a.Name,
                a.StringDate,
                a.Status,
                a.Remark,
                a.RemarkClosed,
                a.Engineer,
                a.Comment,
                a.JournalNumber
            }).Select(j => new ConsolidatedDailyWorkReport()
            {
                DN = j.Key.DN,
                PID = j.Key.PID,
                Customer = j.Key.Customer,
                Point = j.Key.Point,
                Description = j.Key.Description,
                Name = j.Key.Name,
                Date = j.Key.StringDate,
                Status = j.Key.Status,
                Remark = j.Key.Remark,
                RemarkClosed = j.Key.RemarkClosed,
                Engineer = j.Key.Engineer,
                Comment = j.Key.Comment,
                JournalNumber = j.Key.JournalNumber,
                DailyWorkReports = j.Select(a => a.Number).ToList()
            });

            ConsolidatedDailyWorkReports = e;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage())
            {
                // Grab the sheet with the template.
                ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Ежедневный отчет");
                sheet.PrinterSettings.PaperSize = ePaperSize.A4;
                sheet.PrinterSettings.Orientation = eOrientation.Landscape;
                sheet.Cells["A1:M1"].Merge = true;
                sheet.Column(1).Width = 18;
                sheet.Column(1).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(1).Style.Font.Size = 12;
                sheet.Column(1).Style.WrapText = true;
                sheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(2).Width = 19.3;
                sheet.Column(2).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(2).Style.Font.Size = 12;
                sheet.Column(2).Style.WrapText = true;
                sheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(3).Width = 27;
                sheet.Column(3).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(3).Style.Font.Size = 12;
                sheet.Column(3).Style.WrapText = true;
                sheet.Column(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(3).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(4).Width = 11.5;
                sheet.Column(4).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(4).Style.Font.Size = 12;
                sheet.Column(4).Style.WrapText = true;
                sheet.Column(4).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(4).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(5).Width = 100;
                sheet.Column(5).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(5).Style.Font.Size = 12;
                sheet.Column(5).Style.WrapText = true;
                sheet.Column(5).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(6).Width = 40;
                sheet.Column(6).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(6).Style.Font.Size = 12;
                sheet.Column(6).Style.WrapText = true;
                sheet.Column(6).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(6).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(7).Width = 13;
                sheet.Column(7).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(7).Style.Font.Size = 12;
                sheet.Column(7).Style.WrapText = true;
                sheet.Column(7).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(7).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(8).Width = 18;
                sheet.Column(8).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(8).Style.Font.Size = 12;
                sheet.Column(8).Style.WrapText = true;
                sheet.Column(8).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(8).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(9).Width = 16;
                sheet.Column(9).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(9).Style.Font.Size = 12;
                sheet.Column(9).Style.WrapText = true;
                sheet.Column(9).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(9).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(10).Width = 16;
                sheet.Column(10).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(10).Style.Font.Size = 12;
                sheet.Column(10).Style.WrapText = true;
                sheet.Column(10).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(10).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(11).Width = 20;
                sheet.Column(11).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(11).Style.Font.Size = 12;
                sheet.Column(11).Style.WrapText = true;
                sheet.Column(11).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(11).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(12).Width = 16;
                sheet.Column(12).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(12).Style.Font.Size = 12;
                sheet.Column(12).Style.WrapText = true;
                sheet.Column(12).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Column(12).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(13).Width = 19;
                sheet.Column(13).Style.Font.Name = "Franklin Gothic Book";
                sheet.Column(13).Style.Font.Size = 12;
                sheet.Column(13).Style.WrapText = true;
                sheet.Column(13).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Column(13).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                sheet.Row(1).Style.Font.Bold = true;
                sheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Row(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom;
                sheet.Row(1).CustomHeight = true;
                sheet.Row(1).Height = 21;

                sheet.Row(2).Style.Font.Bold = true;
                sheet.Row(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Row(2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Row(2).CustomHeight = true;
                sheet.Row(2).Height = 63;

                sheet.Row(3).Style.Font.Bold = true;
                sheet.Row(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Row(3).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet.Row(3).CustomHeight = true;
                sheet.Row(3).Height = 16.5;
                sheet.Cells["A1"].Value = $"Ежедневный отчет о работе службы МТО";
                sheet.Cells["A2"].Value = "DN";
                sheet.Cells["B2"].Value = "Номер PID";
                sheet.Cells["C2"].Value = "Номер спецификации";
                sheet.Cells["D2"].Value = "Пункт ПТК";
                sheet.Cells["E2"].Value = "Контролируемые параметры";
                sheet.Cells["F2"].Value = "Наименование изделия";
                sheet.Cells["G2"].Value = "Дата";
                sheet.Cells["H2"].Value = "Статус";
                sheet.Cells["I2"].Value = "Замечание открыто";
                sheet.Cells["J2"].Value = "Замечание закрыто";
                sheet.Cells["K2"].Value = "Инженер";
                sheet.Cells["L2"].Value = "Примечание";
                sheet.Cells["M2"].Value = "Номер ЖТН";
                sheet.Cells["A3"].Value = "1";
                sheet.Cells["B3"].Value = "2";
                sheet.Cells["C3"].Value = "3";
                sheet.Cells["D3"].Value = "4";
                sheet.Cells["E3"].Value = "5";
                sheet.Cells["F3"].Value = "6";
                sheet.Cells["G3"].Value = "7";
                sheet.Cells["H3"].Value = "8";
                sheet.Cells["I3"].Value = "9";
                sheet.Cells["J3"].Value = "10";
                sheet.Cells["K3"].Value = "11";
                sheet.Cells["L3"].Value = "12";
                sheet.Cells["M3"].Value = "13";

                int recordIndex = 4;
                foreach (var row in ConsolidatedDailyWorkReports)
                {
                    sheet.Row(recordIndex).CustomHeight = false;
                    string numbers = "";
                    sheet.Cells[recordIndex, 1].Value = row.DN;
                    sheet.Cells[recordIndex, 2].Value = row.PID;
                    sheet.Cells[recordIndex, 3].Value = row.Customer;
                    sheet.Cells[recordIndex, 4].Value = row.Point;
                    sheet.Cells[recordIndex, 5].Value = row.Description;
                    sheet.Cells[recordIndex, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                    foreach (var i in row.DailyWorkReports)
                    {
                        numbers += i + ";\n";
                    }
                    sheet.Cells[recordIndex, 6].Value = $"{row.Name}: {numbers}";
                    sheet.Cells[recordIndex, 7].Value = row.Date;
                    sheet.Cells[recordIndex, 8].Value = row.Status;
                    sheet.Cells[recordIndex, 9].Value = row.Remark;
                    sheet.Cells[recordIndex, 10].Value = row.RemarkClosed;
                    sheet.Cells[recordIndex, 11].Value = row.Engineer;
                    sheet.Cells[recordIndex, 12].Value = !String.IsNullOrWhiteSpace(row.Comment) ? row.Comment : "-";
                    sheet.Cells[recordIndex, 13].Value = row.JournalNumber;
                    recordIndex++;
                }
                sheet.PrinterSettings.PrintArea = sheet.Cells[1, 1, recordIndex, 13];
                sheet.PrinterSettings.FitToPage = true;
                sheet.PrinterSettings.FitToWidth = 1;
                sheet.PrinterSettings.FitToHeight = 0;
                sheet.Cells[2, 1, recordIndex, 13].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells[2, 1, recordIndex, 13].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells[2, 1, recordIndex, 13].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.Cells[2, 1, recordIndex, 13].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                sheet.View.ZoomScale = 70;
                sheet.View.PageBreakView = true;

                byte[] bin = package.GetAsByteArray();
                filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Ежедневный отчет.xlsx";
                if (File.Exists(filePath))
                    File.Delete(filePath);
                File.WriteAllBytes(filePath, bin);
                FileInfo fi = new FileInfo(filePath);
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
                if (PID != null && (StartDate != null || EndDate != null || Inspector != null)) MessageBox.Show("Для того, чтобы отобразить работу по выбранному PID," +
                    " очистите все поля, кроме поля PID", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    if (StartDate != null && Inspector == null)
                    {
                        IEnumerable<Inspector> Inspectors = await Task.Run(() => GetDesktopInspectorRecordsAsync());
                        var report = new ObservableCollection<DailyWorkReport>();
                        foreach (var insp in Inspectors)
                        {
                            Selection(insp, report);
                        }
                        Journal = report;
                    }
                    if (Inspector != null && StartDate != null)
                    {
                        var insp = await Task.Run(() => GetDesktopInspectorRecordsAsync(Inspector.Id));
                        var report = new ObservableCollection<DailyWorkReport>();
                        Selection(insp, report);
                        Journal = report;
                    }
                    if (PID != null)
                    {
                        IEnumerable<Inspector> Inspectors = await Task.Run(() => GetDesktopInspectorRecordsAsync());
                        var report = new ObservableCollection<DailyWorkReport>();
                        foreach (var insp in Inspectors)
                        {
                            foreach (var i in insp.CoatingJournals.Where(i => i.DetailId != null && i.Entity.PID != null && i.Entity.PID.Number == PID.Number))
                            {
                                var temp = new DailyWorkReport(i);
                                report.Add(temp);
                            }
                            foreach (var i in insp.SheetGateValveJournals.Where(i => i.DetailId != null && i.Entity.PID != null && i.Entity.PID.Number == PID.Number))
                            {
                                var temp = new DailyWorkReport(i);
                                report.Add(temp);
                            }
                            foreach (var i in insp.SheetGateValveCaseJournals.Where(i => i.DetailId != null && i.Entity.PID != null && i.Entity.PID.Number == PID.Number))
                            {
                                var temp = new DailyWorkReport(i);
                                report.Add(temp);
                            }
                            foreach (var i in insp.SheetGateValveCoverJournals.Where(i => i.DetailId != null && i.Entity.PID != null && i.Entity.PID.Number == PID.Number))
                            {
                                var temp = new DailyWorkReport(i);
                                report.Add(temp);
                            }
                            foreach (var i in insp.GateJournals.Where(i => i.DetailId != null && i.Entity.PID != null && i.Entity.PID.Number == PID.Number))
                            {
                                var temp = new DailyWorkReport(i);
                                report.Add(temp);
                            }
                            foreach (var i in insp.NozzleJournals.Where(i => i.DetailId != null && i.Entity.PID != null && i.Entity.PID.Number == PID.Number))
                            {
                                var temp = new DailyWorkReport(i);
                                report.Add(temp);
                            }
                            foreach (var i in insp.SaddleJournals.Where(i => i.DetailId != null && i.Entity.PID != null && i.Entity.PID.Number == PID.Number))
                            {
                                var temp = new DailyWorkReport(i);
                                report.Add(temp);
                            }
                            foreach (var i in insp.SpindleJournals.Where(i => i.DetailId != null && i.Entity.PID != null && i.Entity.PID.Number == PID.Number))
                            {
                                var temp = new DailyWorkReport(i);
                                report.Add(temp);
                            }
                        }
                        Journal = report;
                    }

                    if (Journal != null)
                        CreateFile(Journal);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Selection(Inspector insp, ObservableCollection<DailyWorkReport> report)
        {
            if (EndDate == null) EndDate = DateTime.Now;

            foreach (var i in insp.CoatingJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }

            foreach (var i in insp.SheetGateValveJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }

            foreach (var i in insp.CaseBottomJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.CoverFlangeJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.CoverSleeveJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.CoverSleeve008Journals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.Ring043Journals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            //foreach (var i in insp.Ring047Journals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            //{
            //    var temp = new DailyWorkReport(i);
            //    report.Add(temp);
            //}
            foreach (var i in insp.SheetGateValveCaseJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.SheetGateValveCoverJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.AssemblyUnitSealingJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }

            foreach (var i in insp.MainFlangeSealingJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }

            foreach (var i in insp.CounterFlangeJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }

            foreach (var i in insp.FrontalSaddleSealingJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.GateJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.NozzleJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.RunningSleeveJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.SaddleJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.ScrewNutJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.ScrewStudJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.ShearPinJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.SpindleJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.ColumnJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.SpringJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.AbovegroundCoatingJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.AbrasiveMaterialJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.UndercoatJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.UndergroundCoatingJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.ControlWeldJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.ForgingMaterialJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.PipeMaterialJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.RolledMaterialJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.SheetMaterialJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.StoresControlJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.WeldingMaterialJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.CoatingChemicalCompositionJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.CoatingPlasticityJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.CoatingPorosityJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.CoatingProtectivePropertiesJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.DegreasingChemicalCompositionJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            //foreach (var i in insp.NDTControls.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            //{
            //    var temp = new DailyWorkReport(i);
            //    report.Add(temp);
            //}
            //foreach (var i in insp.WeldingProceduresJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            //{
            //    var temp = new DailyWorkReport(i);
            //    report.Add(temp);
            //}
            foreach (var i in insp.FactoryInspectionJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
            foreach (var i in insp.PIDJournals.Where(i => i.Date >= StartDate && i.Date <= EndDate && i.DetailId != null))
            {
                var temp = new DailyWorkReport(i);
                report.Add(temp);
            }
        }

        private async Task<Inspector> GetDesktopInspectorRecordsAsync(int id)
        {
            using (DesktopDataContext context = new DesktopDataContext())
            {
                var result = await context.Inspectors
                    .Include(i => i.PIDJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.Specification)
                    .Include(i => i.SheetGateValveJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.CoatingJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.CaseBottomJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverFlangeJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverSleeveJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverSleeve008Journals).ThenInclude(e => e.Entity)
                    .Include(i => i.Ring043Journals).ThenInclude(e => e.Entity)
                    //.Include(i => i.Ring047Journals).ThenInclude(e => e.Entity)
                    .Include(i => i.SheetGateValveCaseJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.SheetGateValveCoverJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.AssemblyUnitSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.MainFlangeSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CounterFlangeJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.FrontalSaddleSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.GateJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.NozzleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.RunningSleeveJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SaddleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.ScrewNutJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ScrewStudJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ShearPinJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SpindleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
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

        private async Task<IEnumerable<Inspector>> GetDesktopInspectorRecordsAsync()
        {
            using (DesktopDataContext context = new DesktopDataContext())
            {
                await context.Inspectors
                    .Include(i => i.PIDJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.Specification)
                    .Include(i => i.SheetGateValveJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.CoatingJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.CaseBottomJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverFlangeJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverSleeveJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CoverSleeve008Journals).ThenInclude(e => e.Entity)
                    .Include(i => i.Ring043Journals).ThenInclude(e => e.Entity)
                    //.Include(i => i.Ring047Journals).ThenInclude(e => e.Entity)
                    .Include(i => i.SheetGateValveCaseJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.SheetGateValveCoverJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.AssemblyUnitSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.MainFlangeSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.CounterFlangeJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.FrontalSaddleSealingJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.GateJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.NozzleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.RunningSleeveJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SaddleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
                    .Include(i => i.ScrewNutJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ScrewStudJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.ShearPinJournals).ThenInclude(e => e.Entity)
                    .Include(i => i.SpindleJournals).ThenInclude(e => e.Entity).ThenInclude(e => e.PID).ThenInclude(e => e.Specification)
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
                    .LoadAsync();
                return context.Inspectors.Local.ToObservableCollection(); 
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
                PIDs = await Task.Run(() => PIDRepo.GetAllAsyncOnlyPID());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public static DailyWorkReportVM LoadVM(DataContext context)
        {
            DailyWorkReportVM vm = new DailyWorkReportVM(context);
            vm.LoadItemCommand.ExecuteAsync();
            return vm;
        }

        public DailyWorkReportVM(DataContext context)
        {
            db = context;
            inspectorRepo = new InspectorRepository(db);
            PIDRepo = new PIDRepository(db);
            CreateDesktopReportFileCommand = new Commands.AsyncCommand(CreateDesktopReportFile);
            CloseWindowCommand = new Command(o => CloseWindow(o));
            OpenExcelReportCommand = new Command(o => OpenExcelReport());
            PrintReportCommand = new Command(o => PrintReport());
            LoadItemCommand = new Commands.AsyncCommand(Load);
        }
    }

    public class ResultDailyWorkReport
    {
        public string JournalNumber { get; set; }
        public IEnumerable<ConsolidatedDailyWorkReport> ConsolidatedDailyWorkReports { get; set; }
    }

    public class ConsolidatedDailyWorkReport
    {
        public string DN { get; set; }
        public string PID { get; set; }
        public string Customer { get; set; }
        public string Point { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string RemarkClosed { get; set; }
        public string Engineer { get; set; }
        public string Comment { get; set; }
        public string JournalNumber { get; set; }
        public IList<string> DailyWorkReports { get; set; }
    }

    public class DailyWorkReport
    {
        public string DN { get; set; }
        public string PID { get; set; }
        public string Customer { get; set; }
        public string Point { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string StringDate { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string RemarkClosed { get; set; }
        public string Engineer { get; set; }
        public string Comment { get; set; }
        public string JournalNumber { get; set; }
        

        #region Constructors

        public DailyWorkReport() { }



        public DailyWorkReport(CoatingJournal journal)
        {
            DN = journal.Entity.PID != null ? journal.Entity.PID.Designation : "-";
            PID = journal.Entity.PID != null ? journal.Entity.PID.Number : "-";
            Customer = journal.Entity.PID != null ? (journal.Entity.PID.Specification != null ? journal.Entity.PID.Specification.Number : "-")  : "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = "№О" + journal.Entity.Number;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }





        public DailyWorkReport(SheetGateValveJournal journal)
        {
            DN = journal.Entity.PID != null ? journal.Entity.PID.Designation : "-";
            PID = journal.Entity.PID != null ? journal.Entity.PID.Number : "-";
            Customer = journal.Entity.PID != null ? (journal.Entity.PID.Specification != null ? journal.Entity.PID.Specification.Number : "-")  : "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = "№О" + journal.Entity.Number;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }





        public DailyWorkReport(CaseBottomJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }







        public DailyWorkReport(CoverFlangeJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(CoverSleeveJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(CoverSleeve008Journal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(Ring043Journal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        //public DailyWorkReport(Ring047Journal journal)
        //{
        //    DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
        //    PID = "-";
        //    Customer = "-";
        //    Point = journal.Point;
        //    Description = journal.Description;
        //    Name = journal.Entity.Name;
        //    Number = journal.Entity.NameForReport;
        //    Date = journal.Date;
        //    StringDate = Date.Value.ToShortDateString();
        //    Status = journal.Status;
        //    Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
        //    RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
        //    Engineer = journal.Inspector.FullName;
        //    Comment = journal.Comment;
        //    JournalNumber = journal.JournalNumber;
        //}

        public DailyWorkReport(SheetGateValveCaseJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = journal.Entity.PID != null ? journal.Entity.PID.Number : "-";
            Customer = journal.Entity.PID != null ? (journal.Entity.PID.Specification != null ? journal.Entity.PID.Specification.Number : "-")  : "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(SheetGateValveCoverJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = journal.Entity.PID != null ? journal.Entity.PID.Number : "-";
            Customer = journal.Entity.PID != null ? (journal.Entity.PID.Specification != null ? journal.Entity.PID.Specification.Number : "-")  : "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }




        public DailyWorkReport(AssemblyUnitSealingJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(MainFlangeSealingJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }



        public DailyWorkReport(CounterFlangeJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }



        public DailyWorkReport(FrontalSaddleSealingJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(GateJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = journal.Entity.PID != null ? journal.Entity.PID.Number : "-";
            Customer = journal.Entity.PID != null ? (journal.Entity.PID.Specification != null ? journal.Entity.PID.Specification.Number : "-")  : "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(NozzleJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = journal.Entity.PID != null ? journal.Entity.PID.Number : "-";
            Customer = journal.Entity.PID != null ? (journal.Entity.PID.Specification != null ? journal.Entity.PID.Specification.Number : "-")  : "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(RunningSleeveJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(SaddleJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = journal.Entity.PID != null ? journal.Entity.PID.Number : "-";
            Customer = journal.Entity.PID != null ? (journal.Entity.PID.Specification != null ? journal.Entity.PID.Specification.Number : "-")  : "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(ScrewNutJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(ScrewStudJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(ShearPinJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(SpindleJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = journal.Entity.PID != null ? journal.Entity.PID.Number : "-";
            Customer = journal.Entity.PID != null ? (journal.Entity.PID.Specification != null ? journal.Entity.PID.Specification.Number : "-")  : "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(ColumnJournal journal)
        {
            DN = !String.IsNullOrWhiteSpace(journal.Entity.DN) ? journal.Entity.DN : "DN?";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(SpringJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.FullName;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(AbovegroundCoatingJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.FullName;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }
        public DailyWorkReport(AbrasiveMaterialJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.FullName;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(UndercoatJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.FullName;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(UndergroundCoatingJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.FullName;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(ControlWeldJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.FullName;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(SheetMaterialJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(PipeMaterialJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(RolledMaterialJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(ForgingMaterialJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.NameForReport;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(StoresControlJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description; 
            Name = "Хранение и складирование материалов";
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(WeldingMaterialJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = journal.Entity.Name;
            Number = journal.Entity.FullName;
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(CoatingChemicalCompositionJournal journal)
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
        }

        public DailyWorkReport(CoatingPlasticityJournal journal)
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
        }

        public DailyWorkReport(CoatingPorosityJournal journal)
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
        }

        public DailyWorkReport(CoatingProtectivePropertiesJournal journal)
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
        }

        public DailyWorkReport(DegreasingChemicalCompositionJournal journal)
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
        }

        //public DailyWorkReport(NDTControlJournal journal)
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

        //public DailyWorkReport(WeldingProceduresJournal journal)
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

        public DailyWorkReport(FactoryInspectionJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
            Point = journal.Point;
            Description = journal.Description;
            Name = "-";
            Number = "";
            Date = journal.Date;
            StringDate = Date.Value.ToShortDateString();
            Status = journal.Status;
            Remark = !String.IsNullOrWhiteSpace(journal.RemarkIssued) ? journal.FullNameRemarkIssued : "-";
            RemarkClosed = !String.IsNullOrWhiteSpace(journal.RemarkClosed) ? journal.FullNameRemarkClosed : "-";
            Engineer = journal.Inspector.FullName;
            Comment = journal.Comment;
            JournalNumber = journal.JournalNumber;
        }

        public DailyWorkReport(PIDJournal journal)
        {
            DN = "-";
            PID = "-";
            Customer = "-";
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
        }

        #endregion

    }
}
