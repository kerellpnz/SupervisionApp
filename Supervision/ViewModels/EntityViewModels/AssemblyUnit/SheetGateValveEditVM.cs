using BusinessLayer.Repository.Implementations.Entities;
using BusinessLayer.Repository.Implementations.Entities.Detailing;
using BusinessLayer.Repository.Implementations.Entities.Material;
using DataLayer;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.SheetGateValveDetails;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.Entities.Periodical;
using DataLayer.Journals.AssemblyUnits;
using DataLayer.Journals.Detailing;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Materials;
using DataLayer.Journals.Periodical;
using DataLayer.TechnicalControlPlans.AssemblyUnits;
using DevExpress.Mvvm.Native;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Supervision.ViewModels.EntityViewModels.DetailViewModels;
using Supervision.ViewModels.EntityViewModels.DetailViewModels.Valve;
using Supervision.ViewModels.EntityViewModels.DetailViewModels.WeldGateValve;
using Supervision.ViewModels.EntityViewModels.Materials.AnticorrosiveCoating;
using Supervision.Views.EntityViews;
using Supervision.Views.EntityViews.DetailViews;
using Supervision.Views.EntityViews.DetailViews.Valve;
using Supervision.Views.EntityViews.DetailViews.WeldGateValve;
using Supervision.Views.EntityViews.MaterialViews.AnticorrosiveCoating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace Supervision.ViewModels.EntityViewModels.AssemblyUnit
{
    public class SheetGateValveEditVM : ViewModelBase
    {
        #region Main
        private readonly BaseTable parentEntity;
        private readonly SheetGateValveRepository repo;
        private readonly InspectorRepository inspectorRepo;
        private readonly JournalNumberRepository journalRepo;
        private readonly SheetGateValveCaseRepository caseRepo;
        private readonly SheetGateValveCoverRepository coverRepo;
        private readonly GateRepository gateRepo;
        private readonly SaddleRepository saddleRepo;
        private readonly ShearPinRepository shearPinRepo;
        private readonly ScrewStudRepository screwStudRepo;
        private readonly ScrewNutRepository screwNutRepo;
        private readonly SpringRepository springRepo;
        //private readonly AssemblyUnitSealingRepository sealRepo;
        private readonly MainFlangeSealingRepository sealRepo;
        private readonly WeldingPeriodicalRepository repoWeld;

        private readonly DataContext db;
        private SheetGateValve selectedItem;
        private readonly BaseAnticorrosiveCoatingRepository materialRepo;
        private readonly PIDRepository pIDRepo;
        private readonly CounterFlangeRepository flangeRepo;
        private SheetGateValveJournal operation;
        private CoatingJournal coatingOperation;
        private IEnumerable<CounterFlange> counterFlanges;
        private CounterFlange selectedCounterFlange;
        private CounterFlange selectedCounterFlangeFromList;
        private readonly NozzleRepository nozzleRepo;
        private IEnumerable<Nozzle> nozzles;
        private IList<WeldingProcedures> Welding;
        private Nozzle selectedNozzle;
        private Nozzle selectedNozzleFromList;

        private PID AddedPID;
        private WeldGateValveCase AddedWeldGateValveCase;
        private WeldGateValveCover AddedWeldGateValveCover;
        private Gate AddedGate;

        private DateTime? date;
        private int? inspectorId;
        private string journalNumber;

        private string filePath;

        public DateTime? Date
        {
            get => date;
            set
            {
                date = value;
                RaisePropertyChanged();
            }
        }

        public int? InspectorId
        {
            get => inspectorId;
            set
            {
                inspectorId = value;
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

        public ICommand AllTestOperationCloseCommand { get; private set; }
        private void AllTestOperationClose()
        {
            if (TestJournal != null)
            {
                if (Date != null && InspectorId != null && JournalNumber != null)
                {
                    foreach (SheetGateValveJournal psi in TestJournal)
                    {
                        if (psi.PointId != 108)
                        {
                            psi.Date = Date;
                            psi.InspectorId = InspectorId;
                            psi.JournalNumber = JournalNumber;
                        }                        
                    }
                }
                else
                {
                    MessageBox.Show("Не все параметры текущей комманды заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Журнал ПСИ пуст!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Commands.IAsyncCommand CreatePassportBlankCommand { get; private set; }
        public async Task CreatePassportBlank()
        {
            try
            {
                IsBusy = true;
                using (FileStream templateDocumentStream = File.OpenRead("BaseBlankPassport.xlsx"))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {
                        // Grab the sheet with the template.
                        if (SelectedItem.PIDId != null)
                        {
                            if (SelectedItem.PID.SpecificationId != null)
                            {
                                if (SelectedItem.PID.Designation != null && SelectedItem.PID.Specification.Number != null)
                                {
                                    string[] designationSplit = SelectedItem.PID.Designation.Split('-');
                                    string[] specificationSplit = SelectedItem.PID.Specification.Number.Split('-');
                                    if (designationSplit.Length == 8)
                                    {
                                        if (specificationSplit.Length == 4)
                                        {
                                            try
                                            {
                                                await Task.Run(() => repo.GetByIdIncludeAsyncForBlank(SelectedItem.Id));
                                                ExcelWorksheet sheet = package.Workbook.Worksheets.Add("БЛАНК", package.Workbook.Worksheets["PassportBlank"]);
                                                sheet.Cells[2, 2].Value = designationSplit[1];
                                                sheet.Cells[2, 4].Value = SelectedItem.PID.Number;
                                                sheet.Cells[2, 9].Value = specificationSplit[2];
                                                sheet.Cells[3, 9].Value = "Поставщик: " + SelectedItem.PID.Specification.Supplier;
                                                sheet.Cells[2, 13].Value = "О" + SelectedItem.Number;
                                                sheet.Cells[38, 2].Value = designationSplit[1];
                                                sheet.Cells[38, 3].Value = designationSplit[2];
                                                sheet.Cells[38, 5].Value = designationSplit[3];
                                                sheet.Cells[38, 6].Value = designationSplit[4];
                                                sheet.Cells[38, 7].Value = designationSplit[5];
                                                sheet.Cells[38, 8].Value = designationSplit[6];
                                                sheet.Cells[38, 9].Value = designationSplit[7];
                                                sheet.Cells[38, 11].Value = "О" + SelectedItem.Number;
                                                sheet.Cells[38, 14].Value = SelectedItem.PID.Number;

                                                int recordIndex = 42;

                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCover?.Number;
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCover?.CaseBottom?.Number;
                                                sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCover?.CaseBottom?.ZK;
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCover?.CaseBottom?.Melt;
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCover?.CoverFlange?.Number;
                                                //sheet.Cells[46, 9].Value = SelectedItem.WeldGateValveCover?.CoverFlange?.ZK;
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCover?.CoverFlange?.Melt;
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCover?.CoverSleeve?.Number;
                                                sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCover?.CoverSleeve?.ZK;
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCover?.CoverSleeve?.Melt;
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCover?.CoverSleeve008?.Number;
                                                sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCover?.CoverSleeve008?.ZK;
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCover?.CoverSleeve008?.Melt;
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCase?.Number;
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCase?.Number;
                                                //sheet.Cells[50, 9].Value = SelectedItem.WeldGateValveCase?.ZK;
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCase?.Melt;
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCase?.CaseBottom?.Number;
                                                sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCase?.CaseBottom?.ZK;
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCase?.CaseBottom?.Melt;
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCase?.CoverFlange?.Number;
                                                //sheet.Cells[52, 9].Value = SelectedItem.WeldGateValveCase?.CoverFlange?.ZK;
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCase?.CoverFlange?.Melt;
                                                recordIndex++;
                                                if (SelectedItem.WeldGateValveCase?.CoverSleeve008 != null)
                                                {
                                                    sheet.Cells[recordIndex, 1].Value = "Штуцер №";
                                                    sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCase.CoverSleeve008.Number;
                                                    sheet.Cells[recordIndex, 8].Value = "ЗК";
                                                    sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCase.CoverSleeve008.ZK;
                                                    sheet.Cells[recordIndex, 13].Value = "пл.";
                                                    sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCase.CoverSleeve008.Melt;
                                                    recordIndex++;
                                                }
                                                if (SelectedItem.WeldGateValveCase?.Rings != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.WeldGateValveCase?.Rings.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Value = "КОЛЬЦО №";
                                                        sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCase?.Rings[i].Number;
                                                        sheet.Cells[recordIndex, 8].Value = "ЗК";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCase?.Rings[i].ZK;
                                                        sheet.Cells[recordIndex, 13].Value = "пл.";
                                                        sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCase?.Rings[i].Melt;
                                                        recordIndex++;
                                                    }
                                                }
                                                sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Row(recordIndex).Style.Font.Size = 12;
                                                sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                sheet.Cells[recordIndex, 1].Value = "ШИБЕР №";
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.Gate?.Number;
                                                sheet.Cells[recordIndex, 8].Value = "ЗК";
                                                sheet.Cells[recordIndex, 9].Value = SelectedItem.Gate?.ZK;
                                                sheet.Cells[recordIndex, 13].Value = "пл.";
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.Gate?.Melt;
                                                recordIndex++;

                                                sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Row(recordIndex).Style.Font.Size = 12;
                                                sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                sheet.Cells[recordIndex, 1].Value = "ШПИНДЕЛЬ №";
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCover?.Spindle?.Number;
                                                sheet.Cells[recordIndex, 8].Value = "ЗК";
                                                sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCover?.Spindle?.ZK;
                                                sheet.Cells[recordIndex, 13].Value = "пл.";
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCover?.Spindle?.Melt;
                                                recordIndex++;

                                                sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Row(recordIndex).Style.Font.Size = 12;
                                                sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                sheet.Cells[recordIndex, 1].Value = "СТОЙКА №";
                                                sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCover?.Column?.Number;
                                                recordIndex++;
                                                if (SelectedItem.Saddles != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.Saddles.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                        sheet.Cells[recordIndex, 1].Value = "ОБОЙМА №";
                                                        sheet.Cells[recordIndex, 5].Value = SelectedItem.Saddles[i].Number;
                                                        sheet.Cells[recordIndex, 8].Value = "ЗК";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.Saddles[i].ZK;
                                                        sheet.Cells[recordIndex, 13].Value = "пл.";
                                                        sheet.Cells[recordIndex, 14].Value = SelectedItem.Saddles[i].Melt;
                                                        recordIndex++;
                                                        if (SelectedItem.Saddles[i].SaddleWithSealings != null)
                                                        {
                                                            for (int k = 0; k < SelectedItem.Saddles[i].SaddleWithSealings.Count; k++)
                                                            {
                                                                sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                                sheet.Row(recordIndex).Style.Font.Size = 12;
                                                                sheet.Cells[recordIndex, 1].Value = "Уплотнение" + (k+1);
                                                                sheet.Cells[recordIndex, 4].Value = SelectedItem.Saddles[i].SaddleWithSealings[k].FrontalSaddleSealing.Certificate;
                                                                sheet.Cells[recordIndex, 8].Value = "парт.";
                                                                sheet.Cells[recordIndex, 9].Value = SelectedItem.Saddles[i].SaddleWithSealings[k].FrontalSaddleSealing.Batch;
                                                                sheet.Cells[recordIndex, 13].Value = SelectedItem.Saddles[i].SaddleWithSealings[k].FrontalSaddleSealing.Drawing;
                                                                recordIndex++;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (SelectedItem.Nozzles != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.Nozzles.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Row(recordIndex+1).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex+1).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                        sheet.Cells[recordIndex, 1].Value = "КАТУШКА №";
                                                        sheet.Cells[recordIndex, 5].Value = SelectedItem.Nozzles[i].Number;
                                                        sheet.Cells[recordIndex, 8].Value = "ЗК";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.Nozzles[i].ZK;
                                                        sheet.Cells[recordIndex, 13].Value = "пл.";
                                                        sheet.Cells[recordIndex, 14].Value = SelectedItem.Nozzles[i].Melt;
                                                        sheet.Cells[recordIndex + 1, 5].Value = SelectedItem.Nozzles[i].Grooving;
                                                        sheet.Cells[recordIndex + 1, 9].Value = SelectedItem.Nozzles[i].TensileStrength;
                                                        recordIndex += 2;
                                                    }
                                                }
                                                sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Row(recordIndex).Style.Font.Size = 12;
                                                sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                sheet.Cells[recordIndex, 1].Value = "КРАН ШАРОВЫЙ";
                                                sheet.Cells[recordIndex, 8].Value = "Др";
                                                sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCover?.BallValveDrainage;
                                                sheet.Cells[recordIndex, 13].Value = "Сп";
                                                sheet.Cells[recordIndex, 14].Value = SelectedItem.WeldGateValveCover?.BallValveDraining;
                                                recordIndex++;
                                                if (SelectedItem.BaseValveWithShearPins != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.BaseValveWithShearPins.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                        sheet.Cells[recordIndex, 1].Value = "ШТИФТ";
                                                        sheet.Cells[recordIndex, 5].Value = "ЗК";
                                                        sheet.Cells[recordIndex, 6].Value = SelectedItem.BaseValveWithShearPins[i].ShearPin.Number;
                                                        sheet.Cells[recordIndex, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                                        sheet.Cells[recordIndex, 8].Value = "Ф";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.BaseValveWithShearPins[i].ShearPin.Diameter;
                                                        sheet.Cells[recordIndex, 10].Value = "Тяга:";
                                                        sheet.Cells[recordIndex, 11].Value = SelectedItem.BaseValveWithShearPins[i].ShearPin.Pull;
                                                        recordIndex++;
                                                    }
                                                }
                                                if (SelectedItem.BaseValveWithScrewStuds != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.BaseValveWithScrewStuds.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                        sheet.Cells[recordIndex, 1].Value = "ШПИЛЬКА №";
                                                        sheet.Cells[recordIndex, 5].Value = SelectedItem.BaseValveWithScrewStuds[i].ScrewStud.Certificate;
                                                        sheet.Cells[recordIndex, 8].Value = "парт.";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.BaseValveWithScrewStuds[i].ScrewStud.Number;
                                                        sheet.Cells[recordIndex, 13].Value = "пл.";
                                                        sheet.Cells[recordIndex, 14].Value = SelectedItem.BaseValveWithScrewStuds[i].ScrewStud.Melt;
                                                        recordIndex++;
                                                    }
                                                }
                                                if (SelectedItem.BaseValveWithScrewNuts != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.BaseValveWithScrewNuts.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                        sheet.Cells[recordIndex, 1].Value = "ГАЙКА №";
                                                        sheet.Cells[recordIndex, 5].Value = SelectedItem.BaseValveWithScrewNuts[i].ScrewNut.Certificate;
                                                        sheet.Cells[recordIndex, 8].Value = "парт.";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.BaseValveWithScrewNuts[i].ScrewNut.Number;
                                                        sheet.Cells[recordIndex, 13].Value = "пл.";
                                                        sheet.Cells[recordIndex, 14].Value = SelectedItem.BaseValveWithScrewNuts[i].ScrewNut.Melt;
                                                        recordIndex++;
                                                    }
                                                }
                                                if (SelectedItem.BaseValveWithSeals != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.BaseValveWithSeals.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                        sheet.Cells[recordIndex, 1].Value = "УПЛОТНИТЕЛЬ:";
                                                        sheet.Cells[recordIndex, 5].Value = SelectedItem.BaseValveWithSeals[i].MainFlangeSealing.Certificate;
                                                        sheet.Cells[recordIndex, 8].Value = "парт.";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.BaseValveWithSeals[i].MainFlangeSealing.Batch;
                                                        //sheet.Cells[recordIndex, 13].Value = "чертеж";
                                                        sheet.Cells[recordIndex, 13].Value = SelectedItem.BaseValveWithSeals[i].MainFlangeSealing.Drawing;
                                                        recordIndex++;
                                                    }
                                                }
                                                if (SelectedItem.WeldGateValveCover?.BaseValveSCoverWithSeals != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.WeldGateValveCover?.BaseValveSCoverWithSeals.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                        sheet.Cells[recordIndex, 1].Value = "УПЛОТНИТЕЛЬ:";
                                                        sheet.Cells[recordIndex, 5].Value = SelectedItem.WeldGateValveCover?.BaseValveSCoverWithSeals[i].AssemblyUnitSealing.Certificate;
                                                        sheet.Cells[recordIndex, 8].Value = "парт.";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.WeldGateValveCover?.BaseValveSCoverWithSeals[i].AssemblyUnitSealing.Batch;
                                                        sheet.Cells[recordIndex, 13].Value = SelectedItem.WeldGateValveCover?.BaseValveSCoverWithSeals[i].AssemblyUnitSealing.Drawing;
                                                        recordIndex++;
                                                    }
                                                }
                                                if (SelectedItem.BaseValveWithCoatings != null)
                                                {
                                                    for (int i = 0; i < SelectedItem.BaseValveWithCoatings.Count; i++)
                                                    {
                                                        sheet.Row(recordIndex).Style.Font.Name = "Franklin Gothic Book";
                                                        sheet.Row(recordIndex).Style.Font.Size = 12;
                                                        sheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                                                        if (SelectedItem.BaseValveWithCoatings[i].BaseAnticorrosiveCoating is UndergroundCoating)
                                                            sheet.Cells[recordIndex, 1].Value = "ПОДЗЕМНОЕ АКП:";
                                                        else sheet.Cells[recordIndex, 1].Value = "НАДЗЕМНОЕ АКП:";
                                                        sheet.Cells[recordIndex, 5].Value = SelectedItem.BaseValveWithCoatings[i].BaseAnticorrosiveCoating.Certificate;
                                                        sheet.Cells[recordIndex, 8].Value = "парт.";
                                                        sheet.Cells[recordIndex, 9].Value = SelectedItem.BaseValveWithCoatings[i].BaseAnticorrosiveCoating.Batch;
                                                        recordIndex++;
                                                    }
                                                }
                                                sheet.Cells[recordIndex, 7].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex, 7].Style.Font.Size = 14;
                                                sheet.Cells[recordIndex, 7].Style.Font.Bold = true;
                                                sheet.Cells[recordIndex, 7].Style.Font.UnderLine = true;
                                                sheet.Cells[recordIndex, 7].Value = "ИСПЫТАНИЯ:";
                                                recordIndex++;
                                                sheet.Cells[recordIndex, 1].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex, 1].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex, 1].Value = "СБОРКА ЗШ";
                                                sheet.Cells[recordIndex, 10].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex, 10].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex, 10].Value = "ПСИ";
                                                sheet.Cells[recordIndex + 2, 10].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 2, 10].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 2, 10].Value = "ВИК после ПСИ";
                                                sheet.Cells[recordIndex + 4, 1].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 4, 1].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 4, 1].Value = "АКП";
                                                sheet.Cells[recordIndex + 5, 1].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 5, 1].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 5, 1].Value = "Фото дроби";
                                                sheet.Cells[recordIndex + 3, 10].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 3, 10].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 3, 10].Value = "Адгезия";
                                                sheet.Cells[recordIndex + 1, 1].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 1, 1].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 1, 1].Value = "Мкр";
                                                sheet.Cells[recordIndex + 2, 1].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 2, 1].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 2, 1].Value = "Ход шибера";
                                                sheet.Cells[recordIndex + 3, 1].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 3, 1].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 3, 1].Value = "Положение шибера";
                                                sheet.Cells[recordIndex + 1, 10].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 1, 10].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 1, 10].Value = "Авт.Сброс";
                                                sheet.Cells[recordIndex + 6, 1].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 6, 1].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 6, 1].Value = "Фото промывки корпуса";
                                                sheet.Cells[recordIndex + 4, 10].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 4, 10].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 4, 10].Value = "ЗИП";
                                                if (SelectedItem.SheetGateValveJournals != null)
                                                {
                                                    foreach (SheetGateValveJournal journal in SelectedItem.SheetGateValveJournals)
                                                    {
                                                        if (journal.PointId == 113 && journal.InspectorId != null && journal.Date != null)
                                                        {                                                            
                                                            sheet.Cells[recordIndex, 6].Style.Font.Name = "Franklin Gothic Book";
                                                            sheet.Cells[recordIndex, 6].Style.Font.Size = 12;
                                                            sheet.Cells[recordIndex, 6].Value = journal.Date.Value.ToShortDateString();
                                                        }
                                                        if (journal.PointId == 111 && journal.InspectorId != null && journal.Date != null)
                                                        {                                                            
                                                            sheet.Cells[recordIndex, 14].Style.Font.Name = "Franklin Gothic Book";
                                                            sheet.Cells[recordIndex, 14].Style.Font.Size = 12;
                                                            sheet.Cells[recordIndex, 14].Value = journal.Date.Value.ToShortDateString();
                                                        }
                                                        if (journal.PointId == 112 && journal.InspectorId != null && journal.Date != null)
                                                        {                                                            
                                                            sheet.Cells[recordIndex + 2, 14].Style.Font.Name = "Franklin Gothic Book";
                                                            sheet.Cells[recordIndex + 2, 14].Style.Font.Size = 12;
                                                            sheet.Cells[recordIndex + 2, 14].Value = journal.Date.Value.ToShortDateString();
                                                        }
                                                    }
                                                }
                                                if (SelectedItem.CoatingJournals != null)
                                                {
                                                    foreach (CoatingJournal journal in SelectedItem.CoatingJournals)
                                                    {
                                                        if (journal.PointId == 6 && journal.InspectorId != null && journal.Date != null)
                                                        {                                                            
                                                            sheet.Cells[recordIndex + 4, 6].Style.Font.Name = "Franklin Gothic Book";
                                                            sheet.Cells[recordIndex + 4, 6].Style.Font.Size = 12;
                                                            sheet.Cells[recordIndex + 4, 6].Value = journal.Date.Value.ToShortDateString();
                                                        }
                                                        if (journal.PointId == 4 && journal.InspectorId != null && journal.Date != null)
                                                        {                                                            
                                                            sheet.Cells[recordIndex + 5, 6].Style.Font.Name = "Franklin Gothic Book";
                                                            sheet.Cells[recordIndex + 5, 6].Style.Font.Size = 12;
                                                            sheet.Cells[recordIndex + 5, 6].Value = journal.Date.Value.ToShortDateString();
                                                        }
                                                        if (journal.PointId == 7 && journal.InspectorId != null && journal.Date != null)
                                                        {                                                            
                                                            sheet.Cells[recordIndex + 3, 14].Style.Font.Name = "Franklin Gothic Book";
                                                            sheet.Cells[recordIndex + 3, 14].Style.Font.Size = 12;
                                                            sheet.Cells[recordIndex + 3, 14].Value = journal.Date.Value.ToShortDateString();
                                                        }
                                                    }
                                                }
                                                
                                                sheet.Cells[recordIndex + 1, 6].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 1, 6].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 1, 6].Value = SelectedItem.Moment + " Н*м";
                                                
                                                sheet.Cells[recordIndex + 2, 6].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 2, 6].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 2, 6].Value = SelectedItem.Time + " c";
                                                
                                                sheet.Cells[recordIndex + 3, 6].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 3, 6].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 3, 6].Value = SelectedItem.GatePlace + " мм";
                                                if (SelectedItem.WeldGateValveCase?.DateOfWashing != null)
                                                {
                                                    
                                                    sheet.Cells[recordIndex + 6, 6].Style.Font.Name = "Franklin Gothic Book";
                                                    sheet.Cells[recordIndex + 6, 6].Style.Font.Size = 12;
                                                    sheet.Cells[recordIndex + 6, 6].Value = SelectedItem.WeldGateValveCase.DateOfWashing.Value.ToShortDateString();
                                                }   
                                                if (SelectedItem.ZIP != null)
                                                {
                                                    
                                                    sheet.Cells[recordIndex + 4, 14].Style.Font.Name = "Franklin Gothic Book";
                                                    sheet.Cells[recordIndex + 4, 14].Style.Font.Size = 12;
                                                    sheet.Cells[recordIndex + 4, 14].Value = SelectedItem.ZIP.Value.ToShortDateString();
                                                }                                                
                                                sheet.Cells[recordIndex + 1, 14].Style.Font.Name = "Franklin Gothic Book";
                                                sheet.Cells[recordIndex + 1, 14].Style.Font.Size = 12;
                                                sheet.Cells[recordIndex + 1, 14].Value = SelectedItem.AutomaticReset + " МПа";
                                                
                                                ExcelWorksheet sheet2 = package.Workbook.Worksheets.Add("ЗАМЕЧАНИЯ");
                                                sheet2.PrinterSettings.PaperSize = ePaperSize.A4;
                                                sheet2.PrinterSettings.Orientation = eOrientation.Landscape;
                                                sheet2.Cells["A1:H1"].Merge = true;
                                                sheet2.Column(1).Width = 5.5;
                                                sheet2.Column(1).Style.Font.Name = "Franklin Gothic Book";
                                                sheet2.Column(1).Style.Font.Size = 12;
                                                sheet2.Column(1).Style.WrapText = true;
                                                sheet2.Column(2).Width = 16;
                                                sheet2.Column(2).Style.Font.Name = "Franklin Gothic Book";
                                                sheet2.Column(2).Style.Font.Size = 12;
                                                sheet2.Column(2).Style.WrapText = true;
                                                sheet2.Column(3).Width = 15;
                                                sheet2.Column(3).Style.Font.Name = "Franklin Gothic Book";
                                                sheet2.Column(3).Style.Font.Size = 12;
                                                sheet2.Column(3).Style.WrapText = true;
                                                sheet2.Column(4).Width = 63;
                                                sheet2.Column(4).Style.Font.Name = "Franklin Gothic Book";
                                                sheet2.Column(4).Style.Font.Size = 12;
                                                sheet2.Column(4).Style.WrapText = true;
                                                sheet2.Column(5).Width = 24;
                                                sheet2.Column(5).Style.Font.Name = "Franklin Gothic Book";
                                                sheet2.Column(5).Style.Font.Size = 12;
                                                sheet2.Column(5).Style.WrapText = true;
                                                sheet2.Column(6).Width = 25;
                                                sheet2.Column(6).Style.Font.Name = "Franklin Gothic Book";
                                                sheet2.Column(6).Style.Font.Size = 12;
                                                sheet2.Column(6).Style.WrapText = true;
                                                sheet2.Column(7).Width = 25;
                                                sheet2.Column(7).Style.Font.Name = "Franklin Gothic Book";
                                                sheet2.Column(7).Style.Font.Size = 12;
                                                sheet2.Column(7).Style.WrapText = true;
                                                sheet2.Column(8).Width = 12;
                                                sheet2.Column(8).Style.Font.Name = "Franklin Gothic Book";
                                                sheet2.Column(8).Style.Font.Size = 12;
                                                sheet2.Column(8).Style.WrapText = true;

                                                sheet2.Row(1).Style.Font.Bold = true;
                                                sheet2.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                sheet2.Row(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                sheet2.Row(1).CustomHeight = true;
                                                sheet2.Row(1).Height = 30;

                                                sheet2.Row(2).Style.Font.Bold = true;
                                                sheet2.Row(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                sheet2.Row(2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                sheet2.Row(2).CustomHeight = true;
                                                sheet2.Row(2).Height = 60;

                                                sheet2.Row(3).Style.Font.Bold = true;
                                                sheet2.Row(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                sheet2.Row(3).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                sheet2.Row(3).CustomHeight = true;
                                                sheet2.Row(3).Height = 16.5;
                                                sheet2.Cells["A1"].Value = $"ЗАМЕЧАНИЯ ЗШ №О"+SelectedItem.Number;
                                                sheet2.Cells["A2"].Value = "№ п/п";
                                                sheet2.Cells["B2"].Value = "Дата выдачи";
                                                sheet2.Cells["C2"].Value = "Пункты ПТК";
                                                sheet2.Cells["D2"].Value = "Операция";
                                                sheet2.Cells["E2"].Value = "Статус";
                                                sheet2.Cells["F2"].Value = "Номер замечания";
                                                sheet2.Cells["G2"].Value = "Инженер";
                                                sheet2.Cells["H2"].Value = "Примечание";
                                                sheet2.Cells["A3"].Value = "1";
                                                sheet2.Cells["B3"].Value = "2";
                                                sheet2.Cells["C3"].Value = "3";
                                                sheet2.Cells["D3"].Value = "4";
                                                sheet2.Cells["E3"].Value = "5";
                                                sheet2.Cells["F3"].Value = "6";
                                                sheet2.Cells["G3"].Value = "7";
                                                sheet2.Cells["H3"].Value = "8";
                                                int recordIndex2 = 4;
                                                int orderIndex = 1;
                                                foreach (SheetGateValveJournal journal in SelectedItem.SheetGateValveJournals)
                                                {
                                                    if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                    {
                                                        sheet2.Row(recordIndex2).CustomHeight = false;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                        sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString(); 
                                                        sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                        sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                        sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                        sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                        sheet2.Cells[recordIndex2, 6].Value = "№"+journal.RemarkIssued;
                                                        sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                        sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                        recordIndex2++;
                                                        orderIndex++;
                                                    }
                                                }
                                                foreach (CoatingJournal journal in SelectedItem.CoatingJournals)
                                                {
                                                    if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                    {
                                                        sheet2.Row(recordIndex2).CustomHeight = false;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                        sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                        sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                        sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                        sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                        sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                        sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                        sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                        sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                        recordIndex2++;
                                                        orderIndex++;
                                                    }
                                                }
                                                if (SelectedItem.WeldGateValveCase != null)
                                                {
                                                    SheetGateValveCase valveCase = await Task.Run(() =>  db.SheetGateValveCases
                                                            .Include(i => i.SheetGateValveCaseJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCaseId));

                                                    sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                    sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                    sheet2.Cells["A" + recordIndex2].Value = $"КОРПУС №" + SelectedItem.WeldGateValveCase.Number;
                                                    recordIndex2++;
                                                    foreach (SheetGateValveCaseJournal journal in valveCase.SheetGateValveCaseJournals)
                                                    {
                                                        if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                        {
                                                            sheet2.Row(recordIndex2).CustomHeight = false;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                            sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                            sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                            sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                            sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                            sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                            sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                            sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                            sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                            recordIndex2++;
                                                            orderIndex++;
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCase.CoverFlange != null)
                                                    {
                                                        CoverFlange flange = await Task.Run(() => db.CoverFlanges
                                                            .Include(i => i.CoverFlangeJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCase.CoverFlangeId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"ФЛАНЕЦ №" + SelectedItem.WeldGateValveCase.CoverFlange.Number;
                                                        recordIndex2++;
                                                        foreach (CoverFlangeJournal journal in flange.CoverFlangeJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCase.CaseBottom != null)
                                                    {
                                                        CaseBottom bottom = await Task.Run(() => db.CaseBottoms
                                                            .Include(i => i.CaseBottomJournals)
                                                            .Include(i => i.MetalMaterial)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCase.CaseBottomId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"ДНИЩЕ №" + SelectedItem.WeldGateValveCase.CaseBottom.Number;
                                                        recordIndex2++;
                                                        foreach (CaseBottomJournal journal in bottom.CaseBottomJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                        if (SelectedItem.WeldGateValveCase.CaseBottom.MetalMaterial != null)
                                                        {
                                                            SheetMaterial material = await Task.Run(() => db.SheetMaterials
                                                            .Include(i => i.SheetMaterialJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCase.CaseBottom.MetalMaterialId));

                                                            sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                            sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ ДНИЩА: ЛИСТ ПЛАВКА №" + SelectedItem.WeldGateValveCase.CaseBottom.MetalMaterial.Melt;
                                                            recordIndex2++;
                                                            foreach (SheetMaterialJournal journal in material.SheetMaterialJournals)
                                                            {
                                                                if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                {
                                                                    sheet2.Row(recordIndex2).CustomHeight = false;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                    sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                    sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                    sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                    sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                    sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                    sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                    sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                    sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                    recordIndex2++;
                                                                    orderIndex++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCase.Rings != null)
                                                    {
                                                        foreach (Ring043 ring in SelectedItem.WeldGateValveCase.Rings)
                                                        {
                                                            if (ring.Status == "НЕ СООТВ.")
                                                            {
                                                                Ring043 ringTemp = await Task.Run(() => db.Rings043
                                                                        .Include(i => i.Ring043Journals)
                                                                        .Include(i => i.MetalMaterial)
                                                                        .SingleOrDefaultAsync(i => i.Id == ring.Id));

                                                                sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                                sheet2.Cells["A" + recordIndex2].Value = $"КОЛЬЦО №" + ring.ZK + "-" + ring.Number;
                                                                recordIndex2++;
                                                                foreach (Ring043Journal journal in ringTemp.Ring043Journals)
                                                                {
                                                                    if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                    {
                                                                        sheet2.Row(recordIndex2).CustomHeight = false;
                                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                        sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                        sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                        sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                        sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                        sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                        sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                        sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                        sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                        sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                        recordIndex2++;
                                                                        orderIndex++;
                                                                    }
                                                                }
                                                                if (ring.MetalMaterial != null)
                                                                {
                                                                    RolledMaterial material = await Task.Run(() => db.RolledMaterials
                                                                                .Include(i => i.RolledMaterialJournals)
                                                                                .SingleOrDefaultAsync(i => i.Id == ring.MetalMaterialId));

                                                                    sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                                    sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ КОЛЬЦА: КРУГ/ТРУБА ПЛАВКА №" + ring.MetalMaterial.Melt;
                                                                    recordIndex2++;
                                                                    foreach (RolledMaterialJournal journal in material.RolledMaterialJournals)
                                                                    {
                                                                        if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                        {
                                                                            sheet2.Row(recordIndex2).CustomHeight = false;
                                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                            sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                            sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                            sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                            sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                            sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                            sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                            sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                            sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                            sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                            recordIndex2++;
                                                                            orderIndex++;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                if (SelectedItem.WeldGateValveCover != null)
                                                {
                                                    SheetGateValveCover valveCase = await Task.Run(() => db.SheetGateValveCovers
                                                            .Include(i => i.SheetGateValveCoverJournals)
                                                            .Include(i => i.MetalMaterial)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCoverId));

                                                    sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                    sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                    sheet2.Cells["A" + recordIndex2].Value = $"КРЫШКА №" + SelectedItem.WeldGateValveCover.Number;
                                                    recordIndex2++;
                                                    foreach (SheetGateValveCoverJournal journal in valveCase.SheetGateValveCoverJournals)
                                                    {
                                                        if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                        {
                                                            sheet2.Row(recordIndex2).CustomHeight = false;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                            sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                            sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                            sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                            sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                            sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                            sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                            sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                            sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                            recordIndex2++;
                                                            orderIndex++;
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCover.CoverFlange != null)
                                                    {
                                                        CoverFlange flange = await Task.Run(() => db.CoverFlanges
                                                            .Include(i => i.CoverFlangeJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.CoverFlangeId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"ФЛАНЕЦ №" + SelectedItem.WeldGateValveCover.CoverFlange.Number;
                                                        recordIndex2++;
                                                        foreach (CoverFlangeJournal journal in flange.CoverFlangeJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCover.CaseBottom != null)
                                                    {
                                                        CaseBottom bottom = await Task.Run(() => db.CaseBottoms
                                                            .Include(i => i.CaseBottomJournals)
                                                            .Include(i => i.MetalMaterial)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.CaseBottomId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"ДНИЩЕ №" + SelectedItem.WeldGateValveCover.CaseBottom.Number;
                                                        recordIndex2++;
                                                        foreach (CaseBottomJournal journal in bottom.CaseBottomJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                        if (SelectedItem.WeldGateValveCover.CaseBottom.MetalMaterial != null)
                                                        {
                                                            SheetMaterial material = await Task.Run(() => db.SheetMaterials
                                                            .Include(i => i.SheetMaterialJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.CaseBottom.MetalMaterialId));

                                                            sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                            sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ ДНИЩА: ЛИСТ ПЛАВКА №" + SelectedItem.WeldGateValveCover.CaseBottom.MetalMaterial.Melt;
                                                            recordIndex2++;
                                                            foreach (SheetMaterialJournal journal in material.SheetMaterialJournals)
                                                            {
                                                                if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                {
                                                                    sheet2.Row(recordIndex2).CustomHeight = false;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                    sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                    sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                    sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                    sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                    sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                    sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                    sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                    sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                    recordIndex2++;
                                                                    orderIndex++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCover.MetalMaterial != null)
                                                    {
                                                        SheetMaterial material = await Task.Run(() => db.SheetMaterials
                                                            .Include(i => i.SheetMaterialJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.MetalMaterialId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ КРЫШКИ: ЛИСТ ПЛАВКА №" + SelectedItem.WeldGateValveCover.MetalMaterial.Melt;
                                                        recordIndex2++;
                                                        foreach (SheetMaterialJournal journal in material.SheetMaterialJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCover.CoverSleeve != null)
                                                    {
                                                        CoverSleeve sleeve = await Task.Run(() => db.CoverSleeves
                                                            .Include(i => i.CoverSleeveJournals)
                                                            .Include(i => i.MetalMaterial)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.CoverSleeveId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"ВТУЛКА ЦЕНТРАЛЬНАЯ №" + SelectedItem.WeldGateValveCover.CoverSleeve.ZK + "-" + SelectedItem.WeldGateValveCover.CoverSleeve.Number;
                                                        recordIndex2++;
                                                        foreach (CoverSleeveJournal journal in sleeve.CoverSleeveJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                        if (SelectedItem.WeldGateValveCover.CoverSleeve.MetalMaterial != null)
                                                        {
                                                            RolledMaterial material = await Task.Run(() => db.RolledMaterials
                                                            .Include(i => i.RolledMaterialJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.CoverSleeve.MetalMaterialId));

                                                            sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                            sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ ВТУЛКИ ЦЕНТРАЛЬНОЙ: КРУГ ПЛАВКА №" + SelectedItem.WeldGateValveCover.CoverSleeve.MetalMaterial.Melt;
                                                            recordIndex2++;
                                                            foreach (RolledMaterialJournal journal in material.RolledMaterialJournals)
                                                            {
                                                                if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                {
                                                                    sheet2.Row(recordIndex2).CustomHeight = false;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                    sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                    sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                    sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                    sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                    sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                    sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                    sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                    sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                    recordIndex2++;
                                                                    orderIndex++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCover.CoverSleeve008 != null)
                                                    {
                                                        CoverSleeve008 sleeve = await Task.Run(() => db.CoverSleeves008
                                                            .Include(i => i.CoverSleeve008Journals)
                                                            .Include(i => i.MetalMaterial)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.CoverSleeve008Id));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"ВТУЛКА ДРЕНАЖНАЯ №" + SelectedItem.WeldGateValveCover.CoverSleeve008.ZK + "-" + SelectedItem.WeldGateValveCover.CoverSleeve008.Number;
                                                        recordIndex2++;
                                                        foreach (CoverSleeve008Journal journal in sleeve.CoverSleeve008Journals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                        if (SelectedItem.WeldGateValveCover.CoverSleeve008.MetalMaterial != null)
                                                        {
                                                            RolledMaterial material = await Task.Run(() => db.RolledMaterials
                                                            .Include(i => i.RolledMaterialJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.CoverSleeve008.MetalMaterialId));

                                                            sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                            sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ ВТУЛКИ ДРЕНАЖНОЙ: КРУГ ПЛАВКА №" + SelectedItem.WeldGateValveCover.CoverSleeve008.MetalMaterial.Melt;
                                                            recordIndex2++;
                                                            foreach (RolledMaterialJournal journal in material.RolledMaterialJournals)
                                                            {
                                                                if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                {
                                                                    sheet2.Row(recordIndex2).CustomHeight = false;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                    sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                    sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                    sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                    sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                    sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                    sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                    sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                    sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                    recordIndex2++;
                                                                    orderIndex++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCover.Spindle != null)
                                                    {
                                                        Spindle sleeve = await Task.Run(() => db.Spindles
                                                            .Include(i => i.SpindleJournals)
                                                            .Include(i => i.MetalMaterial)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.SpindleId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"ШПИНДЕЛЬ №" + SelectedItem.WeldGateValveCover.Spindle.ZK + "-" + SelectedItem.WeldGateValveCover.Spindle.Number;
                                                        recordIndex2++;
                                                        foreach (SpindleJournal journal in sleeve.SpindleJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                        if (SelectedItem.WeldGateValveCover.Spindle.MetalMaterial != null)
                                                        {
                                                            RolledMaterial material = await Task.Run(() => db.RolledMaterials
                                                            .Include(i => i.RolledMaterialJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.Spindle.MetalMaterialId));

                                                            sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                            sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ ШПИНДЕЛЯ: КРУГ ПЛАВКА №" + SelectedItem.WeldGateValveCover.Spindle.MetalMaterial.Melt;
                                                            recordIndex2++;
                                                            foreach (RolledMaterialJournal journal in material.RolledMaterialJournals)
                                                            {
                                                                if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                {
                                                                    sheet2.Row(recordIndex2).CustomHeight = false;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                    sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                    sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                    sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                    sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                    sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                    sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                    sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                    sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                    recordIndex2++;
                                                                    orderIndex++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (SelectedItem.WeldGateValveCover.Column != null)
                                                    {
                                                        Column column = await Task.Run(() => db.Columns
                                                            .Include(i => i.ColumnJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.ColumnId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"СТОЙКА №" +  SelectedItem.WeldGateValveCover.Column.Number;
                                                        recordIndex2++;
                                                        foreach (ColumnJournal journal in SelectedItem.WeldGateValveCover.Column.ColumnJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                        if (SelectedItem.WeldGateValveCover.Column.RunningSleeve != null)
                                                        {
                                                            RunningSleeve sleeve = await Task.Run(() => db.RunningSleeves
                                                            .Include(i => i.RunningSleeveJournals)
                                                            .SingleOrDefaultAsync(i => i.Id == SelectedItem.WeldGateValveCover.Column.RunningSleeveId));

                                                            sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                            sheet2.Cells["A" + recordIndex2].Value = $"ВТУЛКА ХОДОВАЯ №" + SelectedItem.WeldGateValveCover.Column.RunningSleeve.ZK + "-" + SelectedItem.WeldGateValveCover.Column.RunningSleeve.Number;
                                                            recordIndex2++;
                                                            foreach (RunningSleeveJournal journal in sleeve.RunningSleeveJournals)
                                                            {
                                                                if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                {
                                                                    sheet2.Row(recordIndex2).CustomHeight = false;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                    sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                    sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                    sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                    sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                    sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                    sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                    sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                    sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                    recordIndex2++;
                                                                    orderIndex++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                if (SelectedItem.Saddles != null)
                                                {
                                                    foreach (Saddle saddle in SelectedItem.Saddles)
                                                    {
                                                        if (saddle.Status == "НЕ СООТВ.")
                                                        {
                                                            Saddle saddleTemp = await Task.Run(() => db.Saddles
                                                                    .Include(i => i.SaddleJournals)
                                                                    .Include(i => i.MetalMaterial)
                                                                    .SingleOrDefaultAsync(i => i.Id == saddle.Id));

                                                            sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                            sheet2.Cells["A" + recordIndex2].Value = $"ОБОЙМА №" + saddle.ZK + "-" + saddle.Number;
                                                            recordIndex2++;
                                                            foreach (SaddleJournal journal in saddleTemp.SaddleJournals)
                                                            {
                                                                if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                {
                                                                    sheet2.Row(recordIndex2).CustomHeight = false;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                    sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                    sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                    sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                    sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                    sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                    sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                    sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                    sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                    recordIndex2++;
                                                                    orderIndex++;
                                                                }
                                                            }
                                                            if (saddle.MetalMaterial != null)
                                                            {
                                                                SheetMaterial material = await Task.Run(() => db.SheetMaterials
                                                                            .Include(i => i.SheetMaterialJournals)
                                                                            .SingleOrDefaultAsync(i => i.Id == saddle.MetalMaterialId));

                                                                sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                                sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ ОБОЙМЫ: ЛИСТ ПЛАВКА №" + saddle.MetalMaterial.Melt;
                                                                recordIndex2++;
                                                                foreach (SheetMaterialJournal journal in material.SheetMaterialJournals)
                                                                {
                                                                    if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                    {
                                                                        sheet2.Row(recordIndex2).CustomHeight = false;
                                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                        sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                        sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                        sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                        sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                        sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                        sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                        sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                        sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                        sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                        recordIndex2++;
                                                                        orderIndex++;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                if (SelectedItem.Gate != null)
                                                {
                                                    Gate gate = await Task.Run(() => db.Gates
                                                        .Include(i => i.GateJournals)
                                                        .Include(i => i.MetalMaterial)
                                                        .SingleOrDefaultAsync(i => i.Id == SelectedItem.GateId));

                                                    sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                    sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                    sheet2.Cells["A" + recordIndex2].Value = $"ШИБЕР №" + SelectedItem.Gate.Number;
                                                    recordIndex2++;
                                                    foreach (GateJournal journal in gate.GateJournals)
                                                    {
                                                        if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                        {
                                                            sheet2.Row(recordIndex2).CustomHeight = false;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                            sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                            sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                            sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                            sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                            sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                            sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                            sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                            sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                            recordIndex2++;
                                                            orderIndex++;
                                                        }
                                                    }
                                                    if (SelectedItem.Gate.MetalMaterial != null)
                                                    {
                                                        SheetMaterial material = await Task.Run(() => db.SheetMaterials
                                                        .Include(i => i.SheetMaterialJournals)
                                                        .SingleOrDefaultAsync(i => i.Id == SelectedItem.Gate.MetalMaterialId));

                                                        sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                        sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                        sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ ШИБЕРА: ЛИСТ ПЛАВКА №" + material.Melt;
                                                        recordIndex2++;
                                                        foreach (SheetMaterialJournal journal in material.SheetMaterialJournals)
                                                        {
                                                            if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                            {
                                                                sheet2.Row(recordIndex2).CustomHeight = false;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                recordIndex2++;
                                                                orderIndex++;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (SelectedItem.Nozzles != null)
                                                {
                                                    foreach (Nozzle Nozzle in SelectedItem.Nozzles)
                                                    {
                                                        if (Nozzle.Status == "НЕ СООТВ.")
                                                        {
                                                            Nozzle NozzleTemp = await Task.Run(() => db.Nozzles
                                                                    .Include(i => i.NozzleJournals)
                                                                    .Include(i => i.MetalMaterial)
                                                                    .SingleOrDefaultAsync(i => i.Id == Nozzle.Id));

                                                            sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                            sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                            sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                            sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                            sheet2.Cells["A" + recordIndex2].Value = $"КАТУШКА №" + Nozzle.ZK + "-" + Nozzle.Number;
                                                            recordIndex2++;
                                                            foreach (NozzleJournal journal in NozzleTemp.NozzleJournals)
                                                            {
                                                                if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                {
                                                                    sheet2.Row(recordIndex2).CustomHeight = false;
                                                                    sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                    sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                    sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                    sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                    sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                    sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                    sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                    sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                    sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                    sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                    sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                    recordIndex2++;
                                                                    orderIndex++;
                                                                }
                                                            }
                                                            if (Nozzle.MetalMaterial != null)
                                                            {
                                                                SheetMaterial material = await Task.Run(() => db.SheetMaterials
                                                                            .Include(i => i.SheetMaterialJournals)
                                                                            .SingleOrDefaultAsync(i => i.Id == Nozzle.MetalMaterialId));

                                                                sheet2.Row(recordIndex2).Style.Font.Bold = true;
                                                                sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                sheet2.Cells["A" + recordIndex2 + ":" + "H" + recordIndex2].Merge = true;
                                                                sheet2.Cells["A" + recordIndex2].Value = $"МАТЕРИАЛ КАТУШКИ: ЛИСТ ПЛАВКА №" + Nozzle.MetalMaterial.Melt;
                                                                recordIndex2++;
                                                                foreach (SheetMaterialJournal journal in material.SheetMaterialJournals)
                                                                {
                                                                    if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                                                                    {
                                                                        sheet2.Row(recordIndex2).CustomHeight = false;
                                                                        sheet2.Row(recordIndex2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                                        sheet2.Row(recordIndex2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                                                        sheet2.Cells[recordIndex2, 1].Value = orderIndex.ToString();
                                                                        sheet2.Cells[recordIndex2, 2].Value = journal.Date.Value.ToShortDateString();
                                                                        sheet2.Cells[recordIndex2, 3].Value = journal.Point;
                                                                        sheet2.Cells[recordIndex2, 4].Value = journal.Description;
                                                                        sheet2.Cells[recordIndex2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                                                                        sheet2.Cells[recordIndex2, 5].Value = journal.Status;
                                                                        sheet2.Cells[recordIndex2, 6].Value = "№" + journal.RemarkIssued;
                                                                        sheet2.Cells[recordIndex2, 7].Value = journal.Inspector?.Name;
                                                                        sheet2.Cells[recordIndex2, 8].Value = !String.IsNullOrWhiteSpace(journal.Comment) ? journal.Comment : "-";
                                                                        recordIndex2++;
                                                                        orderIndex++;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                sheet2.PrinterSettings.PrintArea = sheet2.Cells[1, 1, recordIndex2 - 1, 8];
                                                sheet2.Cells[2, 1, recordIndex2 - 1, 8].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                sheet2.Cells[2, 1, recordIndex2 - 1, 8].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                sheet2.Cells[2, 1, recordIndex2 - 1, 8].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                sheet2.Cells[2, 1, recordIndex2 - 1, 8].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                package.Workbook.Worksheets.MoveToEnd("PassportBlank");
                                                package.Workbook.Worksheets["PassportBlank"].Hidden = eWorkSheetHidden.VeryHidden;

                                                byte[] bin = package.GetAsByteArray();
                                                filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Бланк проверки паспорта.xlsx";
                                                if (File.Exists(filePath))
                                                    File.Delete(filePath);
                                                File.WriteAllBytes(filePath, bin);
                                                FileInfo fi = new FileInfo(filePath);

                                                if (filePath != null & File.Exists(filePath))
                                                    Process.Start(filePath);
                                            }
                                            catch (Exception e)
                                            {
                                                MessageBox.Show(e.Message, "Ошибка");
                                            }
                                        }
                                        else MessageBox.Show("В формате номера спецификации есть ошибка. Номер должен содержать 4 параметра и разделители в виде \"-\". Пример: 00007936-ТПР-ТСИБ-2020", "Ошибка");
                                    }
                                    else MessageBox.Show("В формате обозначения ЗШ есть ошибка. Обозначение должно содержать 8 параметров и разделители в виде \"-\". Пример: ЗШ-700-1,6-1,6-Св-ЭП-С0-ХЛ1", "Ошибка");
                                }
                                else MessageBox.Show("Отсутствует обозначение ЗШ или номер спецификации!", "Ошибка");
                            }
                            else MessageBox.Show("Спецификация отсутствует!", "Ошибка");
                        }
                        else MessageBox.Show("PID не выбран!", "Ошибка");
                    }
                    templateDocumentStream.Close();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private IEnumerable<SheetGateValve> sheetGateValves;

        public IEnumerable<SheetGateValve> SheetGateValves
        {
            get => sheetGateValves;
            set
            {
                sheetGateValves = value;
                RaisePropertyChanged();
            }
        }

        public Nozzle SelectedNozzle
        {
            get => selectedNozzle;
            set
            {
                selectedNozzle = value;
                RaisePropertyChanged();
            }
        }

        public Nozzle SelectedNozzleFromList
        {
            get => selectedNozzleFromList;
            set
            {
                selectedNozzleFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<Nozzle> Nozzles
        {
            get => nozzles;
            set
            {
                nozzles = value;
                RaisePropertyChanged();
            }
        }

        public Supervision.Commands.IAsyncCommand AddNozzleToCaseCommand { get; private set; }
        private async Task AddNozzleToCase()
        {
            try
            {
                IsBusy = true;
                if (SelectedItem.Nozzles?.Count() < 2 || SelectedItem.Nozzles == null)
                {
                    if (SelectedNozzle != null)
                    {
                        if (!await Task.Run(() => nozzleRepo.IsAssembliedAsync(SelectedNozzle, SelectedItem)))
                        {
                            Nozzle AddedNozzle = SelectedNozzle;
                            AddedNozzle.BaseValveId = SelectedItem.Id;
                            SelectedItem.Nozzles.Add(AddedNozzle);
                            int value = await Task.Run(() => nozzleRepo.Update(AddedNozzle));                            
                            if (value == 0)
                            {                                
                                SelectedItem.Nozzles.Remove(AddedNozzle);                                
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз, либо перезайдите в текущую ЗШ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedNozzle = null;
                                Nozzles = nozzleRepo.UpdateList();
                            }                                                      
                        }
                    }
                    else MessageBox.Show("Объект не выбран!", "Ошибка");
                }
                else MessageBox.Show("Невозможно привязать более 2 катушек!", "Ошибка");
            }            
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand DeleteNozzleFromCaseCommand { get; private set; }
        private async Task DeleteNozzleFromCase()
        {
            try
            {
                IsBusy = true;
                if (SelectedNozzleFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        Nozzle DeletedNozzle = SelectedNozzleFromList;
                        DeletedNozzle.BaseValveId = null;
                        SelectedItem.Nozzles.Remove(DeletedNozzle);
                        int value = await Task.Run(() =>  nozzleRepo.Update(DeletedNozzle));
                        if (value == 0)
                        {                            
                            SelectedItem.Nozzles.Add(DeletedNozzle);                            
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз, либо перезайдите в текущую ЗШ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            Nozzles = nozzleRepo.UpdateList();
                        }                        
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditNozzleCommand { get; private set; }
        private void EditNozzle()
        {
            if (SelectedNozzleFromList != null)
            {
                _ = new NozzleEditView
                {
                    DataContext = NozzleEditVM.LoadVM(SelectedNozzleFromList.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        public CounterFlange SelectedCounterFlange
        {
            get => selectedCounterFlange;
            set
            {
                selectedCounterFlange = value;
                RaisePropertyChanged();
            }
        }
        public CounterFlange SelectedCounterFlangeFromList
        {
            get => selectedCounterFlangeFromList;
            set
            {
                selectedCounterFlangeFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<CounterFlange> CounterFlanges
        {
            get => counterFlanges;
            set
            {
                counterFlanges = value;
                RaisePropertyChanged();
            }
        }

        public CoatingJournal CoatingOperation
        {
            get => coatingOperation;
            set
            {
                coatingOperation = value;
                RaisePropertyChanged();
            }
        }

        public SheetGateValveJournal Operation
        {
            get => operation;
            set
            {
                operation = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<string> journalNumbers;
        private IEnumerable<string> drawings;

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
        private IEnumerable<Inspector> inspectors;
        private IList<SheetGateValveJournal> assemblyPreparationJournal;
        private IList<SheetGateValveJournal> assemblyJournal;
        private IList<SheetGateValveJournal> testJournal;
        private IList<SheetGateValveJournal> afterTestJournal;
        private IList<SheetGateValveJournal> documentationJournal;
        private IList<SheetGateValveJournal> shippingJournal;
        private IList<CoatingJournal> coatingJournal;

        public SheetGateValve SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public IList<SheetGateValveJournal> AssemblyPreparationJournal
        {
            get => assemblyPreparationJournal;
            set
            {
                assemblyPreparationJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveJournal> AssemblyJournal
        {
            get => assemblyJournal;
            set
            {
                assemblyJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveJournal> TestJournal
        {
            get => testJournal;
            set
            {
                testJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveJournal> AfterTestJournal
        {
            get => afterTestJournal;
            set
            {
                afterTestJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveJournal> DocumentationJournal
        {
            get => documentationJournal;
            set
            {
                documentationJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<SheetGateValveJournal> ShippingJournal
        {
            get => shippingJournal;
            set
            {
                shippingJournal = value;
                RaisePropertyChanged();
            }
        }
        public IList<CoatingJournal> CoatingJournal
        {
            get => coatingJournal;
            set
            {
                coatingJournal = value;
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
        #endregion

        #region Case
        private IEnumerable<SheetGateValveCase> cases;

        public IEnumerable<SheetGateValveCase> Cases
        {
            get => cases;
            set
            {
                cases = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Cover
        private IEnumerable<SheetGateValveCover> covers;

        public IEnumerable<SheetGateValveCover> Covers
        {
            get => covers;
            set
            {
                covers = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Gate
        private IEnumerable<Gate> gates;

        public IEnumerable<Gate> Gates
        {
            get => gates;
            set
            {
                gates = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Saddle
        private IEnumerable<Saddle> saddles;
        private Saddle selectedSaddle;
        private Saddle selectedSaddleFromList;

        public Saddle SelectedSaddle
        {
            get => selectedSaddle;
            set
            {
                selectedSaddle = value;
                RaisePropertyChanged();
            }
        }
        public Saddle SelectedSaddleFromList
        {
            get => selectedSaddleFromList;
            set
            {
                selectedSaddleFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<Saddle> Saddles
        {
            get => saddles;
            set
            {
                saddles = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ShearPin
        private IEnumerable<ShearPin> shearPins;
        private ShearPin selectedShearPin;
        private BaseValveWithShearPin selectedShearPinFromList;

        public ShearPin SelectedShearPin
        {
            get => selectedShearPin;
            set
            {
                selectedShearPin = value;
                RaisePropertyChanged();
            }
        }
        public BaseValveWithShearPin SelectedShearPinFromList
        {
            get => selectedShearPinFromList;
            set
            {
                selectedShearPinFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<ShearPin> ShearPins
        {
            get => shearPins;
            set
            {
                shearPins = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ScrewStud
        private IEnumerable<ScrewStud> screwStuds;
        private ScrewStud selectedScrewStud;
        private BaseValveWithScrewStud selectedScrewStudFromList;

        public ScrewStud SelectedScrewStud
        {
            get => selectedScrewStud;
            set
            {
                selectedScrewStud = value;
                RaisePropertyChanged();
            }
        }
        public BaseValveWithScrewStud SelectedScrewStudFromList
        {
            get => selectedScrewStudFromList;
            set
            {
                selectedScrewStudFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<ScrewStud> ScrewStuds
        {
            get => screwStuds;
            set
            {
                screwStuds = value;
                RaisePropertyChanged();
            }
        }
        #endregion




        #region ScrewNut
        private IEnumerable<ScrewNut> screwNuts;
        private ScrewNut selectedScrewNut;
        private BaseValveWithScrewNut selectedScrewNutFromList;

        public ScrewNut SelectedScrewNut
        {
            get => selectedScrewNut;
            set
            {
                selectedScrewNut = value;
                RaisePropertyChanged();
            }
        }
        public BaseValveWithScrewNut SelectedScrewNutFromList
        {
            get => selectedScrewNutFromList;
            set
            {
                selectedScrewNutFromList = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<ScrewNut> ScrewNuts
        {
            get => screwNuts;
            set
            {
                screwNuts = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Spring
        private IEnumerable<Spring> springs;
        private Spring selectedSpring;
        private BaseValveWithSpring selectedSpringFromList;

        public Spring SelectedSpring
        {
            get => selectedSpring;
            set
            {
                selectedSpring = value;
                RaisePropertyChanged();
            }
        }
        public BaseValveWithSpring SelectedSpringFromList
        {
            get => selectedSpringFromList;
            set
            {
                selectedSpringFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<Spring> Springs
        {
            get => springs;
            set
            {
                springs = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Seal
        private IEnumerable<BaseSealing> seals;
        //private IEnumerable<BaseSealing> seals2;
        private BaseSealing selectedSeal;
        private BaseValveWithSealing selectedSealFromList;

        public BaseSealing SelectedSeal
        {
            get => selectedSeal;
            set
            {
                selectedSeal = value;
                RaisePropertyChanged();
            }
        }
        public BaseValveWithSealing SelectedSealFromList
        {
            get => selectedSealFromList;
            set
            {
                selectedSealFromList = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<BaseSealing> Seals
        {
            get => seals;
            set
            {
                seals = value;
                RaisePropertyChanged();
            }
        }

        //public IEnumerable<BaseSealing> Seals2
        //{
        //    get => seals2;
        //    set
        //    {
        //        seals2 = value;
        //        RaisePropertyChanged();
        //    }
        //}
        #endregion



        #region TCP
        private IEnumerable<SheetGateValveTCP> points;
        private SheetGateValveTCP selectedTCPPoint;
        private CoatingTCP selectedCoatingTCPPoint;
        private IEnumerable<CoatingTCP> coatingPoints;

        public IEnumerable<SheetGateValveTCP> Points
        {
            get => points;
            set
            {
                points = value;
                RaisePropertyChanged();
            }
        }
        public IEnumerable<CoatingTCP> CoatingPoints
        {
            get => coatingPoints;
            set
            {
                coatingPoints = value;
                RaisePropertyChanged();
            }
        }
        public SheetGateValveTCP SelectedTCPPoint
        {
            get => selectedTCPPoint;
            set
            {
                selectedTCPPoint = value;
                RaisePropertyChanged();
            }
        }
        public CoatingTCP SelectedCoatingTCPPoint
        {
            get => selectedCoatingTCPPoint;
            set
            {
                selectedCoatingTCPPoint = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region CoatingMaterial
        private IEnumerable<BaseAnticorrosiveCoating> anticorrosiveMaterials;
        private BaseAnticorrosiveCoating selectedMaterial;
        private BaseValveWithCoating selectedMaterialFromList;

        public IEnumerable<BaseAnticorrosiveCoating> AnticorrosiveMaterials
        {
            get => anticorrosiveMaterials;
            set
            {
                anticorrosiveMaterials = value;
                RaisePropertyChanged();
            }
        }
        public BaseAnticorrosiveCoating SelectedMaterial
        {
            get => selectedMaterial;
            set
            {
                selectedMaterial = value;
                RaisePropertyChanged();
            }
        }
        public BaseValveWithCoating SelectedMaterialFromList
        {
            get => selectedMaterialFromList;
            set
            {
                selectedMaterialFromList = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PID
        private IEnumerable<PID> pIDs;
        public IEnumerable<PID> PIDs
        {
            get => pIDs;
            set
            {
                pIDs = value;
                RaisePropertyChanged();
            }
        }
        #endregion




        public static SheetGateValveEditVM LoadVM(int id, BaseTable entity, DataContext context)
        {
            SheetGateValveEditVM vm = new SheetGateValveEditVM(entity, context);
            vm.LoadItemCommand.ExecuteAsync(id);
            return vm;
        }

        private bool CanExecute()
        {
            return true;
        }

        public ICommand AddMaterialToValveCommand { get; private set; }
        private void AddMaterialToValve()
        {
            try
            {
                if (SelectedMaterial != null)
                {
                    IsBusy = true;
                    SelectedItem.BaseValveWithCoatings.Add(new BaseValveWithCoating() { BaseValveId = SelectedItem.Id, BaseAnticorrosiveCoatingId = SelectedMaterial.Id, BaseAnticorrosiveCoating = SelectedMaterial });
                    //materialRepo.Update(SelectedMaterial));
                    SelectedMaterial = null;
                    //await SaveItemCommand.ExecuteAsync();                    
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand DeleteMaterialFromValveCommand { get; private set; }
        private void DeleteMaterialFromValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedMaterialFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        //SelectedMaterial = SelectedMaterialFromList.BaseAnticorrosiveCoating;
                        SelectedItem.BaseValveWithCoatings.Remove(SelectedMaterialFromList);
                        //materialRepo.Update(SelectedMaterial);
                        SelectedMaterial = null;
                        //await SaveItemCommand.ExecuteAsync();
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditMaterialCommand { get; private set; }
        private void EditMaterial()
        {
            if (SelectedMaterialFromList != null)
            {
                if (SelectedMaterialFromList.BaseAnticorrosiveCoating is Undercoat)
                {
                    _ = new BaseAnticorrosiveCoatingEditView
                    {
                        DataContext = UndercoatEditVM.LoadVM(SelectedMaterialFromList.BaseAnticorrosiveCoatingId, SelectedItem, db)
                    };
                }
                if (SelectedMaterialFromList.BaseAnticorrosiveCoating is AbovegroundCoating)
                {
                    _ = new BaseAnticorrosiveCoatingEditView
                    {
                        DataContext = AbovegroundCoatingEditVM.LoadVM(SelectedMaterialFromList.BaseAnticorrosiveCoatingId, SelectedItem, db)
                    };
                }
                if (SelectedMaterialFromList.BaseAnticorrosiveCoating is UndergroundCoating)
                {
                    _ = new BaseAnticorrosiveCoatingEditView
                    {
                        DataContext = UndergroundCoatingEditVM.LoadVM(SelectedMaterialFromList.BaseAnticorrosiveCoatingId, SelectedItem, db)
                    };
                }
                if (SelectedMaterialFromList.BaseAnticorrosiveCoating is AbrasiveMaterial)
                {
                    _ = new BaseAnticorrosiveCoatingEditView
                    {
                        DataContext = AbrasiveMaterialEditVM.LoadVM(SelectedMaterialFromList.BaseAnticorrosiveCoatingId, SelectedItem, db)
                    };
                }
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        public Supervision.Commands.IAsyncCommand AddCounterFlangeToValveCommand { get; private set; }
        private async Task AddCounterFlangeToValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedItem.CounterFlanges?.Count() < 2 || SelectedItem.CounterFlanges == null)
                {
                    if (SelectedCounterFlange != null)
                    {
                        if (!await flangeRepo.IsAssembliedAsync(SelectedCounterFlange))
                        {
                            SelectedCounterFlange.BaseValveId = SelectedItem.Id;
                            flangeRepo.Update(SelectedCounterFlange);
                            SelectedCounterFlange = null;
                            CounterFlanges = flangeRepo.UpdateList();
                        }
                    }
                    else MessageBox.Show("Объект не выбран!", "Ошибка");
                }
                else MessageBox.Show("Невозможно привязать более 2 фланцев!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand DeleteCounterFlangeFromValveCommand { get; private set; }
        private async Task DeleteCounterFlangeFromValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedCounterFlangeFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        SelectedCounterFlangeFromList.BaseValveId = null;
                        flangeRepo.Update(SelectedCounterFlangeFromList);
                        CounterFlanges = flangeRepo.UpdateList();
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditCounterFlangeCommand { get; private set; }
        private void EditCounterFlange()
        {
            if (SelectedCounterFlangeFromList != null)
            {
                _ = new CounterFlangeEditView
                {
                    DataContext = CounterFlangeEditVM.LoadVM(SelectedCounterFlangeFromList.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        

        public ICommand AddSealToValveCommand { get; private set; }
        private void AddSealToValve()
        {
            try
            {

                //if (SelectedSeal != null)
                //{
                //    IsBusy = true;
                //    SelectedItem.BaseValveWithSeals.Add(new BaseValveWithSealing() { BaseValveId = SelectedItem.Id, AssemblyUnitSealingId = SelectedSeal.Id });
                //    sealRepo.Update(SelectedSeal);
                //    SelectedSeal = null;
                //    await SaveItemCommand.ExecuteAsync();
                //}
                //else MessageBox.Show("Объект не выбран!", "Ошибка");


                //if (SelectedSeal != null)
                //{
                //    IsBusy = true;
                //    if (await sealRepo.IsAmountRemaining(SelectedSeal))
                //    {
                //        SelectedItem.BaseValveWithSeals.Add(new BaseValveWithSealing() { BaseValveId = SelectedItem.Id, AssemblyUnitSealingId = SelectedSeal.Id });
                //        SelectedSeal.AmountRemaining -= 1;
                //        sealRepo.Update(SelectedSeal);
                //        SelectedSeal = null;
                //        await SaveItemCommand.ExecuteAsync();
                //    }
                //}
                //else MessageBox.Show("Объект не выбран!", "Ошибка");

                if (SelectedSeal != null)
                {                    
                    //if (SelectedSeal is AssemblyUnitSealing)
                    //{
                    //    AssemblyUnitSealing SelectedSealAssemblyUnit = (AssemblyUnitSealing) SelectedSeal;
                    //    IsBusy = true;                        
                    //    if (await sealRepo2.IsAmountRemaining(SelectedSealAssemblyUnit))
                    //    {
                    //        SelectedItem.BaseValveWithSeals.Add(new BaseValveWithSealing() { BaseValveId = SelectedItem.Id, AssemblyUnitSealingId = SelectedSealAssemblyUnit.Id });
                    //        SelectedSealAssemblyUnit.AmountRemaining -= 1;
                    //        sealRepo2.Update(SelectedSealAssemblyUnit);
                    //        SelectedSeal = null;                            
                    //        await SaveItemCommand.ExecuteAsync();
                    //    }
                    //}
                    if (SelectedSeal is MainFlangeSealing)
                    {
                        MainFlangeSealing SelectedSealMainFlange = (MainFlangeSealing) SelectedSeal;
                        IsBusy = true;
                        SelectedItem.BaseValveWithSeals.Add(new BaseValveWithSealing() { BaseValveId = SelectedItem.Id, MainFlangeSealingId = SelectedSealMainFlange.Id, MainFlangeSealing = SelectedSealMainFlange });
                        //sealRepo.Update(SelectedSealMainFlange);
                        SelectedSeal = null;
                        //await SaveItemCommand.ExecuteAsync();
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");

            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand DeleteSealFromValveCommand { get; private set; }
        private void DeleteSealFromValve()
        {
            try
            {

                //IsBusy = true;
                //if (SelectedSealFromList != null)
                //{
                //    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                //    if (result == MessageBoxResult.Yes)
                //    {
                //        SelectedSeal = SelectedSealFromList.AssemblyUnitSealing;
                //        SelectedItem.BaseValveWithSeals.Remove(SelectedSealFromList);
                //        sealRepo.Update(SelectedSeal);
                //        SelectedSeal = null;
                //        await SaveItemCommand.ExecuteAsync();
                //    }
                //}
                //else MessageBox.Show("Объект не выбран!", "Ошибка");


                //IsBusy = true;
                //if (SelectedSealFromList != null)
                //{
                //    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                //    if (result == MessageBoxResult.Yes)
                //    {
                //        SelectedSeal = SelectedSealFromList.AssemblyUnitSealing;
                //        SelectedSeal.AmountRemaining += 1;
                //        SelectedItem.BaseValveWithSeals.Remove(SelectedSealFromList);
                //        sealRepo.Update((AssemblyUnitSealing) SelectedSeal);
                //        SelectedSeal = null;
                //        await SaveItemCommand.ExecuteAsync();
                //    }
                //}
                //else MessageBox.Show("Объект не выбран!", "Ошибка");

                IsBusy = true;
                if (SelectedSealFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        //if (SelectedSealFromList.AssemblyUnitSealing != null)
                        //{
                        //    AssemblyUnitSealing SelectedSealAssemblyUnit;
                        //    SelectedSealAssemblyUnit = SelectedSealFromList.AssemblyUnitSealing;
                        //    SelectedSealAssemblyUnit.AmountRemaining += 1;
                        //    SelectedItem.BaseValveWithSeals.Remove(SelectedSealFromList);
                        //    sealRepo.Update(SelectedSealAssemblyUnit);
                        //    SelectedSealAssemblyUnit = null;
                        //    await SaveItemCommand.ExecuteAsync();
                        //}
                        if (SelectedSealFromList.MainFlangeSealing != null)
                        {
                            //MainFlangeSealing SelectedSealMainFlange;
                            //SelectedSealMainFlange = SelectedSealFromList.MainFlangeSealing;
                            SelectedItem.BaseValveWithSeals.Remove(SelectedSealFromList);
                            //sealRepo.Update(SelectedSealMainFlange);
                            SelectedSealFromList = null;
                            //await SaveItemCommand.ExecuteAsync();
                        }
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditSealCommand { get; private set; }
        private void EditSeal()
        {
            //if (SelectedSealFromList != null)
            //{
            //    _ = new AssemblyUnitSealingEditView
            //    {
            //        DataContext = AssemblyUnitSealingEditVM.LoadVM(SelectedSealFromList.AssemblyUnitSealing.Id, SelectedItem, db)
            //    };
            //}
            //else MessageBox.Show("Объект не выбран", "Ошибка");

            if (SelectedSealFromList != null)
            {
                //if (SelectedSealFromList.AssemblyUnitSealing != null)
                //{
                //    _ = new AssemblyUnitSealingEditView
                //    {
                //        DataContext = AssemblyUnitSealingEditVM.LoadVM(SelectedSealFromList.AssemblyUnitSealing.Id, SelectedItem, db)
                //    };
                //}
                if (SelectedSealFromList.MainFlangeSealing != null)
                {
                    _ = new MainFlangeSealingEditView
                    {
                        DataContext = MainFlangeSealingEditVM.LoadVM(SelectedSealFromList.MainFlangeSealing.Id, SelectedItem, db)
                    };
                }
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        public Supervision.Commands.IAsyncCommand AddSpringToValveCommand { get; private set; }
        private async Task AddSpringToValve()
        {
            try
            {
                if (SelectedSpring != null)
                {
                    bool success = Int32.TryParse(Microsoft.VisualBasic.Interaction.InputBox("Введите количество пружин:"), out int tempAmount);
                    if (success && tempAmount > 0)
                    {
                        IsBusy = true;
                        if (await springRepo.IsAmountRemaining(SelectedSpring, tempAmount))
                        {
                            BaseValveWithSpring baseValveWithSpring = new BaseValveWithSpring() { BaseValveId = SelectedItem.Id, SpringId = SelectedSpring.Id, Spring = SelectedSpring, SpringAmount = tempAmount };
                            SelectedItem.BaseValveWithSprings.Add(baseValveWithSpring);
                            SelectedSpring.AmountRemaining -= tempAmount;                            

                            int value = await Task.Run(() => springRepo.Update(SelectedSpring));
                            if (value == 0)
                            {
                                SelectedSpring.AmountRemaining += tempAmount;
                                SelectedItem.BaseValveWithSprings.Remove(baseValveWithSpring);
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз, либо перезайдите в текущую ЗШ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedSpring = null;                                
                            }

                            
                            //await SaveItemCommand.ExecuteAsync();
                        }
                    }
                    else MessageBox.Show("Введено некорректное знаение", "Ошибка");
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand DeleteSpringFromValveCommand { get; private set; }
        private async Task DeleteSpringFromValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedSpringFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        BaseValveWithSpring baseValveWithSpring = SelectedSpringFromList;
                        SelectedSpring = baseValveWithSpring.Spring;
                        SelectedSpring.AmountRemaining += baseValveWithSpring.SpringAmount;
                        SelectedItem.BaseValveWithSprings.Remove(baseValveWithSpring);

                        int value = await Task.Run(() => springRepo.Update(SelectedSpring));
                        if (value == 0)
                        {
                            SelectedSpring.AmountRemaining -= SelectedSpringFromList.SpringAmount;
                            SelectedItem.BaseValveWithSprings.Add(baseValveWithSpring);
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз, либо перезайдите в текущую ЗШ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            SelectedSpring = null;
                        }
                        //await SaveItemCommand.ExecuteAsync();
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditSpringCommand { get; private set; }
        private void EditSpring()
        {
            if (SelectedSpringFromList != null)
            {
                _ = new SpringEditView
                {
                    DataContext = SpringEditVM.LoadVM(SelectedSpringFromList.Spring.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        public Supervision.Commands.IAsyncCommand AddScrewNutToValveCommand { get; private set; }
        private async Task AddScrewNutToValve()
        {
            try
            {
                if (SelectedScrewNut != null)
                {
                    bool success = Int32.TryParse(Microsoft.VisualBasic.Interaction.InputBox("Введите количество гаек:"), out int tempAmount);
                    if (success && tempAmount > 0)
                    {
                        IsBusy = true;
                        if (await screwNutRepo.IsAmountRemaining(SelectedScrewNut, tempAmount))
                        {
                            //SelectedItem.BaseValveWithScrewNuts.Add(new BaseValveWithScrewNut() { BaseValveId = SelectedItem.Id, ScrewNutId = SelectedScrewNut.Id, ScrewNutAmount = tempAmount });
                            //SelectedScrewNut.AmountRemaining -= tempAmount;
                            //screwNutRepo.Update(SelectedScrewNut);
                            //SelectedScrewNut = null;
                            //await SaveItemCommand.ExecuteAsync();

                            BaseValveWithScrewNut baseValveWithScrewNut = new BaseValveWithScrewNut() { BaseValveId = SelectedItem.Id, ScrewNutId = SelectedScrewNut.Id, ScrewNut = SelectedScrewNut, ScrewNutAmount = tempAmount };
                            SelectedItem.BaseValveWithScrewNuts.Add(baseValveWithScrewNut);
                            SelectedScrewNut.AmountRemaining -= tempAmount;

                            int value = await Task.Run(() => screwNutRepo.Update(SelectedScrewNut));
                            if (value == 0)
                            {
                                SelectedScrewNut.AmountRemaining += tempAmount;
                                SelectedItem.BaseValveWithScrewNuts.Remove(baseValveWithScrewNut);
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedScrewNut = null;
                            }                            
                        }
                    }
                    else MessageBox.Show("Введено некорректное знаение", "Ошибка");
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand DeleteScrewNutFromValveCommand { get; private set; }
        private async Task DeleteScrewNutFromValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedScrewNutFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        //SelectedScrewNut = SelectedScrewNutFromList.ScrewNut;
                        //SelectedScrewNut.AmountRemaining += SelectedScrewNutFromList.ScrewNutAmount;
                        //SelectedItem.BaseValveWithScrewNuts.Remove(SelectedScrewNutFromList);
                        //screwNutRepo.Update(SelectedScrewNut);
                        //SelectedScrewNut = null;
                        //await SaveItemCommand.ExecuteAsync();

                        BaseValveWithScrewNut baseValveWithScrewNut = SelectedScrewNutFromList;
                        SelectedScrewNut = baseValveWithScrewNut.ScrewNut;
                        SelectedScrewNut.AmountRemaining += baseValveWithScrewNut.ScrewNutAmount;
                        SelectedItem.BaseValveWithScrewNuts.Remove(baseValveWithScrewNut);

                        int value = await Task.Run(() => screwNutRepo.Update(SelectedScrewNut));
                        if (value == 0)
                        {
                            SelectedScrewNut.AmountRemaining -= baseValveWithScrewNut.ScrewNutAmount;
                            SelectedItem.BaseValveWithScrewNuts.Add(baseValveWithScrewNut);
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            SelectedScrewNut = null;
                        }
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditScrewNutCommand { get; private set; }
        private void EditScrewNut()
        {
            if (SelectedScrewNutFromList != null)
            {
                _ = new ScrewNutEditView
                {
                    DataContext = ScrewNutEditVM.LoadVM(SelectedScrewNutFromList.ScrewNut.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        public Supervision.Commands.IAsyncCommand AddScrewStudToValveCommand { get; private set; }
        private async Task AddScrewStudToValve()
        {
            try
            {
                if (SelectedScrewStud != null)
                {
                    bool success = Int32.TryParse(Microsoft.VisualBasic.Interaction.InputBox("Введите количество шпилек:"), out int tempAmount);
                    if (success && tempAmount > 0)
                    {
                        IsBusy = true;
                        if (await screwStudRepo.IsAmountRemaining(SelectedScrewStud, tempAmount))
                        {
                            //SelectedItem.BaseValveWithScrewStuds.Add(new BaseValveWithScrewStud() { BaseValveId = SelectedItem.Id, ScrewStudId = SelectedScrewStud.Id, ScrewStudAmount = tempAmount });
                            //SelectedScrewStud.AmountRemaining -= tempAmount;
                            //screwStudRepo.Update(SelectedScrewStud);
                            //SelectedScrewStud = null;
                            //await SaveItemCommand.ExecuteAsync();

                            BaseValveWithScrewStud baseValveWithScrewStud = new BaseValveWithScrewStud() { BaseValveId = SelectedItem.Id, ScrewStudId = SelectedScrewStud.Id, ScrewStud = SelectedScrewStud, ScrewStudAmount = tempAmount };
                            SelectedItem.BaseValveWithScrewStuds.Add(baseValveWithScrewStud);
                            SelectedScrewStud.AmountRemaining -= tempAmount;

                            int value = await Task.Run(() => screwStudRepo.Update(SelectedScrewStud));
                            if (value == 0)
                            {
                                SelectedScrewStud.AmountRemaining += tempAmount;
                                SelectedItem.BaseValveWithScrewStuds.Remove(baseValveWithScrewStud);
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedScrewStud = null;
                            }
                        }
                    }
                    else MessageBox.Show("Введено некорректное знаение", "Ошибка");
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand DeleteScrewStudFromValveCommand { get; private set; }
        private async Task DeleteScrewStudFromValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedScrewStudFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        BaseValveWithScrewStud baseValveWithScrewStud = SelectedScrewStudFromList;
                        SelectedScrewStud = baseValveWithScrewStud.ScrewStud;
                        SelectedScrewStud.AmountRemaining += baseValveWithScrewStud.ScrewStudAmount;
                        SelectedItem.BaseValveWithScrewStuds.Remove(baseValveWithScrewStud);

                        int value = await Task.Run(() => screwStudRepo.Update(SelectedScrewStud));
                        if (value == 0)
                        {
                            SelectedScrewStud.AmountRemaining -= baseValveWithScrewStud.ScrewStudAmount;
                            SelectedItem.BaseValveWithScrewStuds.Add(baseValveWithScrewStud);
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            SelectedScrewStud = null;
                        }
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditScrewStudCommand { get; private set; }
        private void EditScrewStud()
        {
            if (SelectedScrewStudFromList != null)
            {
                _ = new ScrewStudEditView
                {
                    DataContext = ScrewStudEditVM.LoadVM(SelectedScrewStudFromList.ScrewStud.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }



        public Supervision.Commands.IAsyncCommand AddShearPinToValveCommand { get; private set; }
        private async Task AddShearPinToValve()
        {
            try
            {
                if (SelectedShearPin != null)
                {
                    bool success = Int32.TryParse(Microsoft.VisualBasic.Interaction.InputBox("Введите количество штифтов:"), out int tempAmount);
                    if (success && tempAmount > 0)
                    {
                        IsBusy = true;
                        if (await shearPinRepo.IsAmountRemaining(SelectedShearPin, tempAmount))
                        {
                            BaseValveWithShearPin baseValveWithShearPin = new BaseValveWithShearPin() { BaseValveId = SelectedItem.Id, ShearPinId = SelectedShearPin.Id, ShearPin = SelectedShearPin, ShearPinAmount = tempAmount };
                            SelectedItem.BaseValveWithShearPins.Add(baseValveWithShearPin);
                            SelectedShearPin.AmountRemaining -= tempAmount;

                            int value = await Task.Run(() => shearPinRepo.Update(SelectedShearPin));
                            if (value == 0)
                            {
                                SelectedShearPin.AmountRemaining += tempAmount;
                                SelectedItem.BaseValveWithShearPins.Remove(baseValveWithShearPin);
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedShearPin = null;
                            }
                        }
                    }
                    else MessageBox.Show("Введено некорректное знаение", "Ошибка");
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand DeleteShearPinFromValveCommand { get; private set; }
        private async Task DeleteShearPinFromValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedShearPinFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        BaseValveWithShearPin baseValveWithShearPin = SelectedShearPinFromList;
                        SelectedShearPin = baseValveWithShearPin.ShearPin;
                        SelectedShearPin.AmountRemaining += baseValveWithShearPin.ShearPinAmount;
                        SelectedItem.BaseValveWithShearPins.Remove(baseValveWithShearPin);

                        int value = await Task.Run(() => shearPinRepo.Update(SelectedShearPin));
                        if (value == 0)
                        {
                            SelectedShearPin.AmountRemaining -= baseValveWithShearPin.ShearPinAmount;
                            SelectedItem.BaseValveWithShearPins.Add(baseValveWithShearPin);
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            SelectedShearPin = null;
                        }
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditShearPinCommand { get; private set; }
        private void EditShearPin()
        {
            if (SelectedShearPinFromList != null)
            {
                _ = new ShearPinEditView
                {
                    DataContext = ShearPinEditVM.LoadVM(SelectedShearPinFromList.ShearPin.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }



        public Supervision.Commands.IAsyncCommand AddSaddleToValveCommand { get; private set; }
        private async Task AddSaddleToValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedItem.Saddles?.Count() < 2 || SelectedItem.Saddles == null)
                {
                    if (SelectedSaddle != null)
                    {
                        //if (!await saddleRepo.IsAssembliedAsync(SelectedSaddle))
                        //{
                        //    SelectedSaddle.BaseValveId = SelectedItem.Id;
                        //    saddleRepo.Update(SelectedSaddle);
                        //    SelectedSaddle = null;
                        //    Saddles = saddleRepo.UpdateList();
                        //}
                        if (!await Task.Run(() => saddleRepo.IsAssembliedAsync(SelectedSaddle, SelectedItem)))
                        {
                            Saddle AddedSaddle = SelectedSaddle;
                            AddedSaddle.BaseValveId = SelectedItem.Id;
                            SelectedItem.Saddles.Add(AddedSaddle);
                            int value = await Task.Run(() => saddleRepo.Update(AddedSaddle));
                            if (value == 0)
                            {
                                SelectedItem.Saddles.Remove(AddedSaddle);
                                MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз, либо перезайдите в текущую ЗШ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                SelectedSaddle = null;
                                Saddles = saddleRepo.UpdateList();
                            }
                        }
                    }
                    else MessageBox.Show("Объект не выбран!", "Ошибка");
                }
                else MessageBox.Show("Невозможно привязать более 2 обойм!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Supervision.Commands.IAsyncCommand DeleteSaddleFromValveCommand { get; private set; }
        private async Task DeleteSaddleFromValve()
        {
            try
            {
                IsBusy = true;
                if (SelectedSaddleFromList != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        //SelectedSaddleFromList.BaseValveId = null;
                        //saddleRepo.Update(SelectedSaddleFromList);
                        //Saddles = saddleRepo.UpdateList();

                        Saddle DeletedSaddle = SelectedSaddleFromList;
                        DeletedSaddle.BaseValveId = null;
                        SelectedItem.Saddles.Remove(DeletedSaddle);
                        int value = await Task.Run(() => saddleRepo.Update(DeletedSaddle));
                        if (value == 0)
                        {
                            SelectedItem.Saddles.Add(DeletedSaddle);
                            MessageBox.Show("Сервер не отвечает. Подождите и попытайтесь еще раз, либо перезайдите в текущую ЗШ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            Saddles = saddleRepo.UpdateList();
                        }
                    }
                }
                else MessageBox.Show("Объект не выбран!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand EditSaddleCommand { get; private set; }
        private void EditSaddle()
        {
            if (SelectedSaddleFromList != null)
            {
                _ = new SaddleEditView
                {
                    DataContext = SaddleEditVM.LoadSaddleEditVM(SelectedSaddleFromList.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Объект не выбран", "Ошибка");
        }

        public ICommand EditGateCommand { get; private set; }
        private void EditGate()
        {
            if (SelectedItem.GateId != null)
            {
                _ = new GateEditView
                {
                    DataContext = GateEditVM.LoadVM(SelectedItem.Gate.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите шибер", "Ошибка");
        }

        public ICommand EditCoverCommand { get; private set; }
        private void EditCover()
        {
            if (SelectedItem.WeldGateValveCover != null)
            {
                _ = new WeldGateValveCoverEditView
                {
                    DataContext = SheetGateValveCoverEditVM.LoadVM(SelectedItem.WeldGateValveCover.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите крышку", "Ошибка");
        }

        public ICommand EditCaseCommand { get; private set; }
        private void EditCase()
        {
            if (SelectedItem.WeldGateValveCase != null)
            {
                _ = new WeldGateValveCaseEditView
                {
                    DataContext = SheetGateValveCaseEditVM.LoadVM(SelectedItem.WeldGateValveCase.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите корпус", "Ошибка");
        }

        public ICommand EditSpindleCommand { get; private set; }
        private void EditSpindle()
        {
            if (SelectedItem.WeldGateValveCover.Spindle != null)
            {
                _ = new SpindleEditView
                {
                    DataContext = SpindleEditVM.LoadVM(SelectedItem.WeldGateValveCover.Spindle.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите шпиндель", "Ошибка");
        }

        public ICommand EditColumnCommand { get; private set; }
        private void EditColumn()
        {
            if (SelectedItem.WeldGateValveCover.Column != null)
            {
                _ = new ColumnEditView
                {
                    DataContext = ColumnEditVM.LoadVM(SelectedItem.WeldGateValveCover.Column.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите стойку", "Ошибка");
        }

        public ICommand EditRunningSleeveCommand { get; private set; }
        private void EditRunningSleeve()
        {
            if (SelectedItem.WeldGateValveCover.Column.RunningSleeve != null)
            {
                _ = new RunningSleeveEditView
                {
                    DataContext = RunningSleeveEditVM.LoadVM(SelectedItem.WeldGateValveCover.Column.RunningSleeve.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите втулку", "Ошибка");
        }

        public ICommand EditCaseFlangeCommand { get; private set; }
        private void EditCaseFlange()
        {
            if (SelectedItem.WeldGateValveCase.CoverFlange != null)
            {
                _ = new CoverFlangeEditView
                {
                    DataContext = CoverFlangeEditVM.LoadVM(SelectedItem.WeldGateValveCase.CoverFlange.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите втулку", "Ошибка");
        }

        public ICommand EditCaseBottomCaseCommand { get; private set; }
        private void EditCaseBottomCase()
        {
            if (SelectedItem.WeldGateValveCase.CaseBottom != null)
            {
                _ = new CaseBottomEditView
                {
                    DataContext = CaseBottomEditVM.LoadVM(SelectedItem.WeldGateValveCase.CaseBottom.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите втулку", "Ошибка");
        }

        public ICommand EditCoverFlangeCommand { get; private set; }
        private void EditCoverFlange()
        {
            if (SelectedItem.WeldGateValveCover.CoverFlange != null)
            {
                _ = new CoverFlangeEditView
                {
                    DataContext = CoverFlangeEditVM.LoadVM(SelectedItem.WeldGateValveCover.CoverFlange.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите втулку", "Ошибка");
        }

        public ICommand EditCaseBottomCoverCommand { get; private set; }
        private void EditCaseBottomCover()
        {
            if (SelectedItem.WeldGateValveCover.CaseBottom != null)
            {
                _ = new CaseBottomEditView
                {
                    DataContext = CaseBottomEditVM.LoadVM(SelectedItem.WeldGateValveCover.CaseBottom.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите втулку", "Ошибка");
        }



        public ICommand EditPIDCommand { get; private set; }
        private void EditPID()
        {
            if (SelectedItem.PID != null)
            {
                _ = new PIDEditView
                {
                    DataContext = PIDEditVM.LoadPIDEditVM(SelectedItem.PID.Id, SelectedItem, db)
                };
            }
            else MessageBox.Show("Для просмотра привяжите PID", "Ошибка");
        }

        public Supervision.Commands.IAsyncCommand<object> SaveItemCommand { get; private set; }
        private async Task SaveItem(object obj)
        {
            try
            {
                IsBusy = true;
                int value = await Task.Run(() => repo.Update(SelectedItem));
                if (value != 0)
                    base.CloseWindow(obj);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public new Commands.IAsyncCommand<object> CloseWindowCommand { get; private set; }
        protected new async Task CloseWindow(object obj)
        {
            if(IsBusy)
            {
                MessageBoxResult result = MessageBox.Show("Процесс сохранения уже запущен, теперь все в \"руках\" сервера. Попробовать отправить запрос на сохранение повторно? (Возможен вылет программы и не сохранение результата)", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await SaveItemCommand.ExecuteAsync(obj);
                }
            }
            else
            {
                bool check = true;
                bool flag = true;
                bool flag1 = false;
                bool flag2 = false;

                if (SelectedItem.Number == null || SelectedItem.Number == "")
                {
                    MessageBox.Show("Не оставляйте номер пустым, либо напишите \"удалить\".", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    check = false;
                }
                else if(!SelectedItem.Number.ToLower().Contains("удалить"))
                {
                    if (SelectedItem.Number != null)
                    {
                        int count = 0;
                        foreach (SheetGateValve entity in SheetGateValves)
                        {
                            if (SelectedItem.Number.Equals(entity.Number))
                            {
                                count++;
                                if (count > 1)
                                {
                                    MessageBox.Show("Задвижка под таким номером уже существует! Действия сохранены не будут! В поле \"Номер\" напишите \"удалить\". Впредь будьте внимательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    count = 0;
                                    check = false;
                                }
                            }
                        }
                    }
                }                

                if (SelectedItem.SheetGateValveJournals != null)
                {
                    foreach (SheetGateValveJournal journal in SelectedItem.SheetGateValveJournals)
                    {
                        if (journal.InspectorId != null)
                        {
                            if (journal.Date == null)
                            {
                                check = false;
                                journal.Status = null;
                                MessageBox.Show("Не выбрана дата!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                            if (journal.JournalNumber == null)
                            {
                                check = false;
                                journal.Status = null;
                                MessageBox.Show("Не выбран номер журнала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                        }
                        if (journal.Date != null)
                        {
                            if (journal.RemarkIssued != null && journal.RemarkIssued != "")
                            {
                                if (!Regex.IsMatch(journal.RemarkIssued, @"^\d+$"))
                                {
                                    check = false;
                                    MessageBox.Show("Введите только номер замечания! Без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            if (journal.RemarkClosed != null && journal.RemarkClosed != "")
                            {
                                if (!Regex.IsMatch(journal.RemarkClosed, @"^\d+$"))
                                {
                                    check = false;
                                    MessageBox.Show("Введите только номер замечания! Без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            if (journal.RemarkIssued != null && journal.RemarkIssued != "" && (journal.RemarkClosed == null || journal.RemarkClosed == ""))
                            {
                                journal.Status = "Не соответствует";
                                journal.DateOfRemark = journal.Date;
                                SelectedItem.Status = "НЕ СООТВ.";
                                flag = false;
                                if (journal.Inspector != null)
                                {
                                    journal.RemarkInspector = journal.Inspector.Name;
                                }
                            }
                            else
                            {
                                journal.Status = "Cоответствует";
                            }
                            if (journal.PointId == 114 && journal.Status == "Cоответствует")
                            {
                                flag1 = true;
                            }
                            if (journal.PointId == 115 && journal.Status == "Cоответствует")
                            {
                                flag2 = true;
                            }
                            if (journal.PointId == 98 && journal.Status == "Cоответствует")
                            {
                                if (String.IsNullOrEmpty(SelectedItem.GatePlace))
                                {
                                    check = false;
                                    MessageBox.Show("Не указано положение шибера!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 102 && journal.Status == "Cоответствует")
                            {
                                if (String.IsNullOrEmpty(SelectedItem.Time))
                                {
                                    check = false;
                                    MessageBox.Show("Не указано время открытия/закрытия ЗШ!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                if (String.IsNullOrEmpty(SelectedItem.Moment))
                                {
                                    check = false;
                                    MessageBox.Show("Не указаны моменты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 103 && journal.Status == "Cоответствует")
                            {
                                if (String.IsNullOrEmpty(SelectedItem.AutomaticReset))
                                {
                                    check = false;
                                    MessageBox.Show("Не указано давление сброса!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 116 && journal.InspectorId != null)
                            {
                                if (SelectedItem.Nozzles == null || SelectedItem.Nozzles.IsEmptyOrSingle())
                                {
                                    check = false;
                                    MessageBox.Show("Катушки не выбраны или выбрана только лишь одна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            if (journal.PointId == 126 && journal.InspectorId != null && Welding[2].Id == 3 && Welding[2].LastControl != null)
                            {
                                if (journal.Date > Welding[2].LastControl)
                                {
                                    Welding[2] = await Task.Run(() => repoWeld.GetByIdIncludeAsync(3));
                                    Welding[2].WeldingProceduresJournals.Add(new WeldingProceduresJournal() { DetailId = 3,
                                    PointId = 120,
                                    Point = journal.Point,
                                    Description = journal.Description,
                                    JournalNumber = journal.JournalNumber,
                                    Date = journal.Date,
                                    Status = journal.Status,
                                    RemarkClosed = journal.RemarkClosed,
                                    RemarkIssued = journal.RemarkIssued,
                                    Comment = journal.Comment,
                                    InspectorId = journal.InspectorId,
                                    PlaceOfControl = journal.PlaceOfControl,
                                    Documents = journal.Documents,
                                    DateOfRemark = journal.DateOfRemark,
                                    RemarkInspector = journal.RemarkInspector
                                });
                                    Welding[2].LastControl = journal.Date;
                                    Welding[2].NextControl = Convert.ToDateTime(journal.Date).AddDays(7);
                                    int value = await Task.Run(() => repoWeld.Update(Welding[2]));
                                    if (value == 0)
                                    {
                                        MessageBox.Show("Режимы РД не были сохранены в периодический контроль из-за ошибки сервера!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }                                
                            }
                            if (journal.PointId == 187 && journal.InspectorId != null && Welding[3].Id == 5 && Welding[3].LastControl != null)
                            {
                                if (journal.Date > Welding[3].LastControl)
                                {
                                    Welding[3] = await Task.Run(() => repoWeld.GetByIdIncludeAsync(5));
                                    Welding[3].WeldingProceduresJournals.Add(new WeldingProceduresJournal()
                                    {
                                        DetailId = 5,
                                        PointId = 120,
                                        Point = journal.Point,
                                        Description = journal.Description,
                                        JournalNumber = journal.JournalNumber,
                                        Date = journal.Date,
                                        Status = journal.Status,
                                        RemarkClosed = journal.RemarkClosed,
                                        RemarkIssued = journal.RemarkIssued,
                                        Comment = journal.Comment,
                                        InspectorId = journal.InspectorId,
                                        PlaceOfControl = journal.PlaceOfControl,
                                        Documents = journal.Documents,
                                        DateOfRemark = journal.DateOfRemark,
                                        RemarkInspector = journal.RemarkInspector
                                    });
                                    Welding[3].LastControl = journal.Date;
                                    Welding[3].NextControl = Convert.ToDateTime(journal.Date).AddDays(7);
                                    int value = await Task.Run(() => repoWeld.Update(Welding[3]));
                                    if (value == 0)
                                    {
                                        MessageBox.Show("Режимы АПИ не были сохранены в периодический контроль из-за ошибки сервера!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            if (SelectedItem.WeldGateValveCaseId != null)
                            {
                                if (journal.PointId == 112 && journal.InspectorId != null && SelectedItem.WeldGateValveCase.DN != null)
                                {
                                    if (Welding[2].Id == 3 && Welding[2].NextControl != null && Int32.Parse(SelectedItem.WeldGateValveCase.DN) > 250)
                                    {
                                        if (Welding[2].NextControl < DateTime.Now && journal.Date > Convert.ToDateTime(Welding[2].NextControl).AddDays(2))
                                        {
                                            journal.Status = "Не соответствует";
                                            SelectedItem.Status = "НЕ СООТВ.";
                                            flag = false;
                                            MessageBox.Show("Просрочен контроль режимов сварки РД. Статус контроля ВИК после ПСИ установлен на \"Не соответствует\"." +
                                                "Обратитесь в службу ОТК завода для выяснения обстоятельств.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    if (Welding[3].Id == 5 && Welding[3].NextControl != null && Int32.Parse(SelectedItem.WeldGateValveCase.DN) > 250)
                                    {
                                        if (Welding[3].NextControl < DateTime.Now && journal.Date > Convert.ToDateTime(Welding[3].NextControl).AddDays(2))
                                        {
                                            journal.Status = "Не соответствует";
                                            SelectedItem.Status = "НЕ СООТВ.";
                                            flag = false;
                                            MessageBox.Show("Просрочен контроль режимов сварки АПИ. Статус контроля ВИК после ПСИ установлен на \"Не соответствует\"." +
                                                "Обратитесь в службу ОТК завода для выяснения обстоятельств.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                }
                if (TestJournal != null)
                {
                    int count = 0;
                    foreach (SheetGateValveJournal journal in TestJournal)
                    {
                        if (!String.IsNullOrWhiteSpace(journal.RemarkIssued))
                        {
                            count++;
                        }
                        if (count > 1)
                        {
                            MessageBox.Show("Обнаружено второе замечание на ПСИ. Возможно нарушение п.13.4.4 ОТТ-108", "ВНИМАНИЕ", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                    }
                }

                bool coatingFlag1 = false;
                bool coatingFlag2 = false;
                if (SelectedItem.CoatingJournals != null)
                {
                    foreach (CoatingJournal journal in SelectedItem.CoatingJournals)
                    {
                        if (journal.InspectorId != null)
                        {
                            if (journal.Date == null)
                            {
                                check = false;
                                journal.Status = null;
                                MessageBox.Show("Не выбрана дата!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                            if (journal.JournalNumber == null)
                            {
                                check = false;
                                journal.Status = null;
                                MessageBox.Show("Не выбран номер журнала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                        }
                        if (journal.Date != null)
                        {
                            if (journal.RemarkIssued != null && journal.RemarkIssued != "")
                            {
                                if (!Regex.IsMatch(journal.RemarkIssued, @"^\d+$"))
                                {
                                    check = false;
                                    MessageBox.Show("Введите только номер замечания! Без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            if (journal.RemarkClosed != null && journal.RemarkClosed != "")
                            {
                                if (!Regex.IsMatch(journal.RemarkClosed, @"^\d+$"))
                                {
                                    check = false;
                                    MessageBox.Show("Введите только номер замечания! Без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                }
                            }
                            if (journal.PointId == 3 && journal.InspectorId != null)
                            {
                                coatingFlag1 = true;
                            }
                            if (journal.PointId == 4 && journal.InspectorId != null)
                            {
                                coatingFlag2 = true;
                            }
                            if (journal.PointId == 6 && journal.InspectorId != null)
                            {
                                if (SelectedItem.BaseValveWithCoatings.IsEmptyOrSingle())
                                {
                                    check = false;
                                    MessageBox.Show("Не добавлены материалы АКП!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    break;
                                } 
                            }
                            if (journal.RemarkIssued != null && journal.RemarkIssued != "" && (journal.RemarkClosed == null || journal.RemarkClosed == ""))
                            {
                                journal.Status = "Не соответствует";
                                journal.DateOfRemark = journal.Date;
                                SelectedItem.Status = "НЕ СООТВ.";
                                flag = false;
                                if (journal.Inspector != null)
                                {
                                    journal.RemarkInspector = journal.Inspector.Name;
                                }
                            }
                            else
                            {
                                journal.Status = "Cоответствует";
                            }
                        }
                    }
                }               

                if ((coatingFlag1 && !coatingFlag2) || (!coatingFlag1 && coatingFlag2))
                {
                    check = false;
                    MessageBox.Show("П.6(АКП) должен быть закрыт вместе с п.7(АКП)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);                    
                }

                void CheckCaseStatus()
                {
                    if (SelectedItem.WeldGateValveCase.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранный корпус имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей ЗШ.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                void CheckCoverStatus()
                {
                    if (SelectedItem.WeldGateValveCover.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранная крышка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей ЗШ.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                void CheckGateStatus()
                {
                    if (SelectedItem.Gate.Status == "НЕ СООТВ.")
                    {
                        SelectedItem.Status = "НЕ СООТВ.";
                        flag = false;
                        MessageBox.Show("Выбранный шибер имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей ЗШ.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                if (SelectedItem.PIDId != null)
                {
                    if (AddedPID == null || !SelectedItem.PID.Equals(AddedPID))
                    {
                        if (!await Task.Run(() => pIDRepo.IsAmountRemaining(SelectedItem)))
                        {
                            SelectedItem.PID = null;
                            check = false;
                        }
                    }                    
                }
                if (SelectedItem.WeldGateValveCaseId != null)
                {
                    if (AddedWeldGateValveCase == null || !SelectedItem.WeldGateValveCase.Equals(AddedWeldGateValveCase))
                    {
                        if (await Task.Run(() => caseRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.WeldGateValveCase = null;
                            check = false;
                        }
                        else CheckCaseStatus();
                    }
                    else CheckCaseStatus();
                }
                if (SelectedItem.WeldGateValveCoverId != null)
                {
                    if (AddedWeldGateValveCover == null || !SelectedItem.WeldGateValveCover.Equals(AddedWeldGateValveCover))
                    {
                        if (await Task.Run(() => coverRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.WeldGateValveCover = null;
                            check = false;
                        }
                        else CheckCoverStatus();
                    }
                    else CheckCoverStatus();
                }
                if (SelectedItem.GateId != null)
                {
                    if (AddedGate == null || !SelectedItem.Gate.Equals(AddedGate))
                    {
                        if (await Task.Run(() => gateRepo.IsAssembliedAsync(SelectedItem)))
                        {
                            SelectedItem.Gate = null;
                            check = false;
                        }
                        else CheckGateStatus();
                    }
                    else CheckGateStatus();
                }
                if (SelectedItem.Nozzles != null)
                {
                    if (SelectedItem.Nozzles.Count != 0)
                    {
                        foreach (SheetGateValveJournal journal in AssemblyPreparationJournal)
                        {
                            if (journal.PointId == 116 && journal.InspectorId == null)
                            {
                                check = false;
                                MessageBox.Show("Не выбрана операция СпС (шов №44)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    foreach (Nozzle nozzle in SelectedItem.Nozzles)
                    {
                        if (nozzle.Status == "НЕ СООТВ.")
                        {
                            SelectedItem.Status = "НЕ СООТВ.";
                            flag = false;
                            MessageBox.Show("Выбранная катушка имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей ЗШ.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }
                    }
                }
                if (SelectedItem.Saddles != null)
                {
                    foreach (Saddle saddle in SelectedItem.Saddles)
                    {
                        if (saddle.Status == "НЕ СООТВ.")
                        {
                            SelectedItem.Status = "НЕ СООТВ.";
                            flag = false;
                            MessageBox.Show("Выбранная обойма имеет статус \"Не соотв\", поэтому этот же статус будет применен к текущей ЗШ.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }
                    }
                }

                if (flag1 && flag2 && flag)
                {
                    SelectedItem.Status = "Отгружен";
                }
                else if (flag)
                {
                    SelectedItem.Status = "Cоотв.";
                }

                if (check)
                {
                    try
                    {
                        IsBusy = true;
                        int value = await Task.Run(() => repo.Update(SelectedItem));
                        if (value != 0)
                            base.CloseWindow(obj);
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
            }            
        }

        public ICommand AddOperationCommand { get; private set; }
        public void AddOperation()
        {
            if (SelectedTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.SheetGateValveJournals.Add(new SheetGateValveJournal(SelectedItem, SelectedTCPPoint));
                
                    AssemblyPreparationJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                    AssemblyJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка").OrderBy(x => x.PointId).ToList();
                    TestJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "ПСИ").OrderBy(x => x.PointId).ToList();
                    AfterTestJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "ВИК после ПСИ").OrderBy(x => x.PointId).ToList();
                    DocumentationJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                    ShippingJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Отгрузка").OrderBy(x => x.PointId).ToList();
                    SelectedTCPPoint = null;                               
            }
        }

        public ICommand RemoveOperationCommand { get; private set; }
        private void RemoveOperation()
        {
            try
            {
                IsBusy = true;
                if (Operation != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {                        
                        SelectedItem.SheetGateValveJournals.Remove(Operation);
                                                   
                            AssemblyPreparationJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                            AssemblyJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка").OrderBy(x => x.PointId).ToList();
                            TestJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "ПСИ").OrderBy(x => x.PointId).ToList();
                            AfterTestJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "ВИК после ПСИ").OrderBy(x => x.PointId).ToList();
                            DocumentationJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                            ShippingJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Отгрузка").OrderBy(x => x.PointId).ToList();
                            Operation = null;                                                  
                    }
                }
                else MessageBox.Show("Выберите операцию!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand AddCoatingOperationCommand { get; private set; }
        public void AddCoatingOperation()
        {
            if (SelectedCoatingTCPPoint == null) MessageBox.Show("Выберите пункт ПТК!", "Ошибка");
            else
            {
                SelectedItem.CoatingJournals.Add(new CoatingJournal(SelectedItem, SelectedCoatingTCPPoint));
                
                    CoatingJournal = SelectedItem.CoatingJournals.OrderBy(x => x.PointId).ToList();
                    SelectedCoatingTCPPoint = null;                               
            }
        }

        public ICommand RemoveCoatingOperationCommand { get; private set; }
        private void RemoveCoatingOperation()
        {
            try
            {
                IsBusy = true;
                if (CoatingOperation != null)
                {
                    MessageBoxResult result = MessageBox.Show("Подтвердите удаление", "Удаление", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        SelectedItem.CoatingJournals.Remove(CoatingOperation);
                        
                            CoatingJournal = SelectedItem.CoatingJournals.OrderBy(x => x.PointId).ToList();                                              
                    }
                }
                else MessageBox.Show("Выберите операцию!", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Commands.IAsyncCommand LoadItemForAssemblyCommand { get; private set; }
        public async Task LoadItemForAssembly()
        {
            try
            {
                IsBusy = true;

                await Task.Run(() => saddleRepo.Load());
                await Task.Run(() => nozzleRepo.Load());
                await Task.Run(() => flangeRepo.Load());
                await Task.Run(() => shearPinRepo.Load());
                await Task.Run(() => screwStudRepo.Load());
                await Task.Run(() => screwNutRepo.Load());
                await Task.Run(() => springRepo.Load());
                await Task.Run(() => gateRepo.Load());
                await Task.Run(() => sealRepo.Load());
                await Task.Run(() => caseRepo.Load());
                await Task.Run(() => coverRepo.Load());

                CounterFlanges = flangeRepo.UpdateList();
                Nozzles = nozzleRepo.UpdateList();
                Saddles = saddleRepo.UpdateList();
                ShearPins = shearPinRepo.SortList();
                ScrewStuds = screwStudRepo.SortList();
                ScrewNuts = screwNutRepo.SortList();
                Springs = springRepo.SortList();
                Gates = gateRepo.SortList();
                Seals = sealRepo.SortList();
                Cases = caseRepo.SortList();
                Covers = coverRepo.SortList();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Commands.IAsyncCommand<int> LoadItemCommand { get; private set; }
        public async Task Load(int id)
        {
            try
            {
                IsBusy = true;
                SheetGateValves = await Task.Run(() => repo.GetAllAsyncForCompare());
                SelectedItem = await Task.Run(() => repo.GetByIdIncludeAsync(id));
                Welding = await Task.Run(() => repoWeld.GetAllAsync());

                //await Task.Run(() => saddleRepo.Load());                
                //await Task.Run(() => nozzleRepo.Load());
                //await Task.Run(() => flangeRepo.Load());
                //await Task.Run(() => shearPinRepo.Load());
                //await Task.Run(() => screwStudRepo.Load());
                //await Task.Run(() => screwNutRepo.Load());
                //await Task.Run(() => springRepo.Load());
                //await Task.Run(() => gateRepo.Load());
                //await Task.Run(() => sealRepo.Load());
                //await Task.Run(() => caseRepo.Load());
                //await Task.Run(() => coverRepo.Load());

                //CounterFlanges = flangeRepo.UpdateList();
                //Nozzles = nozzleRepo.UpdateList();
                //Saddles = saddleRepo.UpdateList();
                //ShearPins = shearPinRepo.SortList();
                //ScrewStuds = screwStudRepo.SortList();
                //ScrewNuts = screwNutRepo.SortList();
                //Springs = springRepo.SortList();
                //Gates = gateRepo.SortList();
                //Seals = sealRepo.SortList();
                //Cases = caseRepo.SortList();
                //Covers = coverRepo.SortList();

                PIDs = await Task.Run(() => pIDRepo.GetAllAsync());  
                AnticorrosiveMaterials = await Task.Run(() => materialRepo.GetAllAsync());
                Inspectors = await Task.Run(() => inspectorRepo.GetAllAsync());
                Drawings = await Task.Run(() => repo.GetPropertyValuesDistinctAsync(i => i.Drawing));
                Points = await Task.Run(() => repo.GetTCPsAsync());
                CoatingPoints = await Task.Run(() => repo.GetCoatingTCPsAsync());
                JournalNumbers = await Task.Run(() => journalRepo.GetActiveJournalNumbersAsync());              
                                
                
                AssemblyPreparationJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка/Сварка").OrderBy(x => x.PointId).ToList();
                AssemblyJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Сборка").OrderBy(x => x.PointId).ToList();
                TestJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "ПСИ").OrderBy(x => x.PointId).ToList();
                AfterTestJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "ВИК после ПСИ").OrderBy(x => x.PointId).ToList();
                CoatingJournal = SelectedItem.CoatingJournals.OrderBy(x => x.PointId).ToList();
                DocumentationJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Документация").OrderBy(x => x.PointId).ToList();
                ShippingJournal = SelectedItem.SheetGateValveJournals.Where(i => i.EntityTCP.OperationType.Name == "Отгрузка").OrderBy(x => x.PointId).ToList();

                if (SelectedItem.PIDId != null)
                {
                    AddedPID = SelectedItem.PID;
                }
                if (SelectedItem.WeldGateValveCaseId != null)
                {
                    AddedWeldGateValveCase = SelectedItem.WeldGateValveCase;
                }
                if (SelectedItem.WeldGateValveCoverId != null)
                {
                    AddedWeldGateValveCover = SelectedItem.WeldGateValveCover;
                }
                if (SelectedItem.GateId != null)
                {
                    AddedGate = SelectedItem.Gate;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public SheetGateValveEditVM(BaseTable entity, DataContext context)
        {
            db = context;
            parentEntity = entity;
            repo = new SheetGateValveRepository(db);
            inspectorRepo = new InspectorRepository(db);
            journalRepo = new JournalNumberRepository(db);
            caseRepo = new SheetGateValveCaseRepository(db);
            coverRepo = new SheetGateValveCoverRepository(db);
            gateRepo = new GateRepository(db);
            saddleRepo = new SaddleRepository(db);
            shearPinRepo = new ShearPinRepository(db);
            screwStudRepo = new ScrewStudRepository(db);
            screwNutRepo = new ScrewNutRepository(db);
            springRepo = new SpringRepository(db);
            //sealRepo = new AssemblyUnitSealingRepository(db);
            sealRepo = new MainFlangeSealingRepository(db);
            repoWeld = new WeldingPeriodicalRepository(db);

            materialRepo = new BaseAnticorrosiveCoatingRepository(db);
            pIDRepo = new PIDRepository(db);
            flangeRepo = new CounterFlangeRepository(db);
            nozzleRepo = new NozzleRepository(db);
            AddNozzleToCaseCommand = new Supervision.Commands.AsyncCommand(AddNozzleToCase);
            DeleteNozzleFromCaseCommand = new Supervision.Commands.AsyncCommand(DeleteNozzleFromCase);
            EditNozzleCommand = new Supervision.Commands.Command(o => EditNozzle());
            LoadItemCommand = new Supervision.Commands.AsyncCommand<int>(Load);
            LoadItemForAssemblyCommand = new Supervision.Commands.AsyncCommand(LoadItemForAssembly);
            SaveItemCommand = new Supervision.Commands.AsyncCommand<object>(SaveItem);
            CloseWindowCommand = new Supervision.Commands.AsyncCommand<object>(CloseWindow);
            AddOperationCommand = new Supervision.Commands.Command(o => AddOperation());
            RemoveOperationCommand = new Supervision.Commands.Command(o => RemoveOperation());
            AddCoatingOperationCommand = new Supervision.Commands.Command(o => AddCoatingOperation());
            RemoveCoatingOperationCommand = new Supervision.Commands.Command(o => RemoveCoatingOperation());
            
            AddSaddleToValveCommand = new Supervision.Commands.AsyncCommand(AddSaddleToValve);
            DeleteSaddleFromValveCommand = new Supervision.Commands.AsyncCommand(DeleteSaddleFromValve);
            EditSaddleCommand = new Supervision.Commands.Command(o => EditSaddle());
            AddShearPinToValveCommand = new Supervision.Commands.AsyncCommand(AddShearPinToValve);
            DeleteShearPinFromValveCommand = new Supervision.Commands.AsyncCommand(DeleteShearPinFromValve);
            EditShearPinCommand = new Supervision.Commands.Command(o => EditShearPin());
            AddScrewStudToValveCommand = new Supervision.Commands.AsyncCommand(AddScrewStudToValve);
            DeleteScrewStudFromValveCommand = new Supervision.Commands.AsyncCommand(DeleteScrewStudFromValve);
            EditScrewStudCommand = new Supervision.Commands.Command(o => EditScrewStud());
            AddScrewNutToValveCommand = new Supervision.Commands.AsyncCommand(AddScrewNutToValve);
            DeleteScrewNutFromValveCommand = new Supervision.Commands.AsyncCommand(DeleteScrewNutFromValve);
            EditScrewNutCommand = new Supervision.Commands.Command(o => EditScrewNut());
            AddSpringToValveCommand = new Supervision.Commands.AsyncCommand(AddSpringToValve);
            DeleteSpringFromValveCommand = new Supervision.Commands.AsyncCommand(DeleteSpringFromValve);
            EditSpringCommand = new Supervision.Commands.Command(o => EditSpring());
            AddSealToValveCommand = new Supervision.Commands.Command(o => AddSealToValve());
            DeleteSealFromValveCommand = new Supervision.Commands.Command(o => DeleteSealFromValve());
            EditSealCommand = new Supervision.Commands.Command(o => EditSeal());
            AddMaterialToValveCommand = new Supervision.Commands.Command(o => AddMaterialToValve());
            DeleteMaterialFromValveCommand = new Supervision.Commands.Command(o => DeleteMaterialFromValve());
            EditMaterialCommand = new Supervision.Commands.Command(o => EditMaterial());
            AddCounterFlangeToValveCommand = new Supervision.Commands.AsyncCommand(AddCounterFlangeToValve);
            DeleteCounterFlangeFromValveCommand = new Supervision.Commands.AsyncCommand(DeleteCounterFlangeFromValve);
            EditCounterFlangeCommand = new Supervision.Commands.Command(o => EditCounterFlange());
            EditCaseCommand = new Supervision.Commands.Command(o => EditCase());
            EditCoverCommand = new Supervision.Commands.Command(o => EditCover());
            EditGateCommand = new Supervision.Commands.Command(o => EditGate());
            EditSpindleCommand = new Supervision.Commands.Command(o => EditSpindle());
            EditColumnCommand = new Supervision.Commands.Command(o => EditColumn());
            EditRunningSleeveCommand = new Supervision.Commands.Command(o => EditRunningSleeve());
            EditCaseFlangeCommand = new Supervision.Commands.Command(o => EditCaseFlange());
            EditCaseBottomCaseCommand = new Supervision.Commands.Command(o => EditCaseBottomCase());
            EditCoverFlangeCommand = new Supervision.Commands.Command(o => EditCoverFlange());
            EditCaseBottomCoverCommand = new Supervision.Commands.Command(o => EditCaseBottomCover());
            EditPIDCommand = new Supervision.Commands.Command(o => EditPID());
            AllTestOperationCloseCommand = new Supervision.Commands.Command(o => AllTestOperationClose());
            CreatePassportBlankCommand = new Supervision.Commands.AsyncCommand(CreatePassportBlank);
        }
    }
}
