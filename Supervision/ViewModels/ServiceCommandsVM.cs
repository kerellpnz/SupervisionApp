using BusinessLayer.Repository.Implementations.Entities.Detailing;
using BusinessLayer.Repository.Implementations.Entities.Material;
using DataLayer;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.SheetGateValveDetails;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.Journals.AssemblyUnits;
using DataLayer.Journals.Detailing;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Materials;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using Microsoft.EntityFrameworkCore;
using Supervision.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Supervision.ViewModels
{
    public class ServiceCommandsVM : ViewModelBase
    {
        public IAsyncCommand LoadCommand { get; private set; }
        public async Task Load()
        {
            IsBusy = true;

            CopyDbFileFromServer();

            using (DesktopDataContext context = new DesktopDataContext())
            {
                Task<int> Deleating = await Task.Run(async () =>
                {
                    List<AssemblyUnitSealing> list1 = await context.AssemblyUnitSeals.ToListAsync();
                    foreach (AssemblyUnitSealing entity in list1)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<AssemblyUnitSealing>().Remove(entity);
                        }
                    }
                    List<AssemblyUnitSealingJournal> list1jr = await context.AssemblyUnitSealingJournals.ToListAsync();
                    foreach (AssemblyUnitSealingJournal entity in list1jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<AssemblyUnitSealingJournal>().Remove(entity);
                        }
                    }

                    List<CaseBottom> list2 = await context.CaseBottoms.ToListAsync();
                    foreach (CaseBottom entity in list2)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<CaseBottom>().Remove(entity);
                        }
                    }
                    List<CaseBottomJournal> list2jr = await context.CaseBottomJournals.ToListAsync();
                    foreach (CaseBottomJournal entity in list2jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<CaseBottomJournal>().Remove(entity);
                        }
                    }

                    List<Column> list3 = await context.Columns.ToListAsync();
                    foreach (Column entity in list3)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<Column>().Remove(entity);
                        }
                    }
                    List<ColumnJournal> list3jr = await context.ColumnJournals.ToListAsync();
                    foreach (ColumnJournal entity in list3jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<ColumnJournal>().Remove(entity);
                        }
                    }

                    List<CounterFlange> list4 = await context.CounterFlanges.ToListAsync();
                    foreach (CounterFlange entity in list4)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<CounterFlange>().Remove(entity);
                        }
                    }
                    List<CounterFlangeJournal> list4jr = await context.CounterFlangeJournals.ToListAsync();
                    foreach (CounterFlangeJournal entity in list4jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<CounterFlangeJournal>().Remove(entity);
                        }
                    }

                    List<CoverSleeve> list5 = await context.CoverSleeves.ToListAsync();
                    foreach (CoverSleeve entity in list5)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<CoverSleeve>().Remove(entity);
                        }
                    }
                    List<CoverSleeveJournal> list5jr = await context.CoverSleeveJournals.ToListAsync();
                    foreach (CoverSleeveJournal entity in list5jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<CoverSleeveJournal>().Remove(entity);
                        }
                    }

                    List<CoverSleeve008> list6 = await context.CoverSleeves008.ToListAsync();
                    foreach (CoverSleeve008 entity in list6)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<CoverSleeve008>().Remove(entity);
                        }
                    }
                    List<CoverSleeve008Journal> list6jr = await context.CoverSleeve008Journals.ToListAsync();
                    foreach (CoverSleeve008Journal entity in list6jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<CoverSleeve008Journal>().Remove(entity);
                        }
                    }

                    List<CoverFlange> list7 = await context.CoverFlanges.ToListAsync();
                    foreach (CoverFlange entity in list7)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<CoverFlange>().Remove(entity);
                        }
                    }
                    List<CoverFlangeJournal> list7jr = await context.CoverFlangeJournals.ToListAsync();
                    foreach (CoverFlangeJournal entity in list7jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<CoverFlangeJournal>().Remove(entity);
                        }
                    }

                    List<FrontalSaddleSealing> list8 = await context.FrontalSaddleSeals.ToListAsync();
                    foreach (FrontalSaddleSealing entity in list8)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<FrontalSaddleSealing>().Remove(entity);
                        }
                    }
                    List<FrontalSaddleSealingJournal> list8jr = await context.FrontalSaddleSealingJournals.ToListAsync();
                    foreach (FrontalSaddleSealingJournal entity in list8jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<FrontalSaddleSealingJournal>().Remove(entity);
                        }
                    }

                    List<Gate> list9 = await context.Gates.ToListAsync();
                    foreach (Gate entity in list9)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<Gate>().Remove(entity);
                        }
                    }
                    List<GateJournal> list9jr = await context.GateJournals.ToListAsync();
                    foreach (GateJournal entity in list9jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<GateJournal>().Remove(entity);
                        }
                    }

                    List<MainFlangeSealing> list10 = await context.MainFlangeSeals.ToListAsync();
                    foreach (MainFlangeSealing entity in list10)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<MainFlangeSealing>().Remove(entity);
                        }
                    }
                    List<MainFlangeSealingJournal> list10jr = await context.MainFlangeSealingJournals.ToListAsync();
                    foreach (MainFlangeSealingJournal entity in list10jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<MainFlangeSealingJournal>().Remove(entity);
                        }
                    }

                    List<Nozzle> list11 = await context.Nozzles.ToListAsync();
                    foreach (Nozzle entity in list11)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<Nozzle>().Remove(entity);
                        }
                    }
                    List<NozzleJournal> list11jr = await context.NozzleJournals.ToListAsync();
                    foreach (NozzleJournal entity in list11jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<NozzleJournal>().Remove(entity);
                        }
                    }

                    List<Ring043> list12 = await context.Rings043.ToListAsync();
                    foreach (Ring043 entity in list12)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<Ring043>().Remove(entity);
                        }
                    }
                    List<Ring043Journal> list12jr = await context.Ring043Journals.ToListAsync();
                    foreach (Ring043Journal entity in list12jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<Ring043Journal>().Remove(entity);
                        }
                    }

                    List<RunningSleeve> list13 = await context.RunningSleeves.ToListAsync();
                    foreach (RunningSleeve entity in list13)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<RunningSleeve>().Remove(entity);
                        }
                    }
                    List<RunningSleeveJournal> list13jr = await context.RunningSleeveJournals.ToListAsync();
                    foreach (RunningSleeveJournal entity in list13jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<RunningSleeveJournal>().Remove(entity);
                        }
                    }

                    List<Saddle> list14 = await context.Saddles.ToListAsync();
                    foreach (Saddle entity in list14)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<Saddle>().Remove(entity);
                        }
                    }
                    List<SaddleJournal> list14jr = await context.SaddleJournals.ToListAsync();
                    foreach (SaddleJournal entity in list14jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<SaddleJournal>().Remove(entity);
                        }
                    }

                    List<ScrewNut> list15 = await context.ScrewNuts.ToListAsync();
                    foreach (ScrewNut entity in list15)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<ScrewNut>().Remove(entity);
                        }
                    }
                    List<ScrewNutJournal> list15jr = await context.ScrewNutJournals.ToListAsync();
                    foreach (ScrewNutJournal entity in list15jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<ScrewNutJournal>().Remove(entity);
                        }
                    }

                    List<ScrewStud> list16 = await context.ScrewStuds.ToListAsync();
                    foreach (ScrewStud entity in list16)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<ScrewStud>().Remove(entity);
                        }
                    }
                    List<ScrewStudJournal> list16jr = await context.ScrewStudJournals.ToListAsync();
                    foreach (ScrewStudJournal entity in list16jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<ScrewStudJournal>().Remove(entity);
                        }
                    }

                    List<ShearPin> list17 = await context.ShearPins.ToListAsync();
                    foreach (ShearPin entity in list17)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<ShearPin>().Remove(entity);
                        }
                    }
                    List<ShearPinJournal> list17jr = await context.ShearPinJournals.ToListAsync();
                    foreach (ShearPinJournal entity in list17jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<ShearPinJournal>().Remove(entity);
                        }
                    }

                    List<SheetGateValveCase> list18 = await context.SheetGateValveCases.ToListAsync();
                    foreach (SheetGateValveCase entity in list18)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<SheetGateValveCase>().Remove(entity);
                        }
                    }
                    List<SheetGateValveCaseJournal> list18jr = await context.SheetGateValveCaseJournals.ToListAsync();
                    foreach (SheetGateValveCaseJournal entity in list18jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<SheetGateValveCaseJournal>().Remove(entity);
                        }
                    }

                    List<SheetGateValveCover> list19 = await context.SheetGateValveCovers.ToListAsync();
                    foreach (SheetGateValveCover entity in list19)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<SheetGateValveCover>().Remove(entity);
                        }
                    }
                    List<SheetGateValveCoverJournal> list19jr = await context.SheetGateValveCoverJournals.ToListAsync();
                    foreach (SheetGateValveCoverJournal entity in list19jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<SheetGateValveCoverJournal>().Remove(entity);
                        }
                    }

                    List<SheetGateValve> list20 = await context.SheetGateValves.ToListAsync();
                    foreach (SheetGateValve entity in list20)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<SheetGateValve>().Remove(entity);
                        }
                    }
                    List<SheetGateValveJournal> list20jr = await context.SheetGateValveJournals.ToListAsync();
                    foreach (SheetGateValveJournal entity in list20jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<SheetGateValveJournal>().Remove(entity);
                        }
                    }

                    List<Spindle> list21 = await context.Spindles.ToListAsync();
                    foreach (Spindle entity in list21)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<Spindle>().Remove(entity);
                        }
                    }
                    List<SpindleJournal> list21jr = await context.SpindleJournals.ToListAsync();
                    foreach (SpindleJournal entity in list21jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<SpindleJournal>().Remove(entity);
                        }
                    }

                    List<Spring> list22 = await context.Springs.ToListAsync();
                    foreach (Spring entity in list22)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<Spring>().Remove(entity);
                        }
                    }
                    List<SpringJournal> list22jr = await context.SpringJournals.ToListAsync();
                    foreach (SpringJournal entity in list22jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<SpringJournal>().Remove(entity);
                        }
                    }

                    List<AbovegroundCoating> list23 = await context.AbovegroundCoatings.ToListAsync();
                    foreach (AbovegroundCoating entity in list23)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<AbovegroundCoating>().Remove(entity);
                        }
                    }
                    List<AbovegroundCoatingJournal> list23jr = await context.AbovegroundCoatingJournals.ToListAsync();
                    foreach (AbovegroundCoatingJournal entity in list23jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<AbovegroundCoatingJournal>().Remove(entity);
                        }
                    }

                    List<AbrasiveMaterial> list24 = await context.AbrasiveMaterials.ToListAsync();
                    foreach (AbrasiveMaterial entity in list24)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<AbrasiveMaterial>().Remove(entity);
                        }
                    }
                    List<AbrasiveMaterialJournal> list24jr = await context.AbrasiveMaterialJournals.ToListAsync();
                    foreach (AbrasiveMaterialJournal entity in list24jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<AbrasiveMaterialJournal>().Remove(entity);
                        }
                    }

                    List<Undercoat> list25 = await context.Undercoats.ToListAsync();
                    foreach (Undercoat entity in list25)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<Undercoat>().Remove(entity);
                        }
                    }
                    List<UndercoatJournal> list25jr = await context.UndercoatJournals.ToListAsync();
                    foreach (UndercoatJournal entity in list25jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<UndercoatJournal>().Remove(entity);
                        }
                    }

                    List<UndergroundCoating> list26 = await context.UndergroundCoatings.ToListAsync();
                    foreach (UndergroundCoating entity in list26)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<UndergroundCoating>().Remove(entity);
                        }
                    }
                    List<UndergroundCoatingJournal> list26jr = await context.UndergroundCoatingJournals.ToListAsync();
                    foreach (UndergroundCoatingJournal entity in list26jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<UndergroundCoatingJournal>().Remove(entity);
                        }
                    }

                    List<ControlWeld> list27 = await context.ControlWelds.ToListAsync();
                    foreach (ControlWeld entity in list27)
                    {
                        if (String.IsNullOrEmpty(entity.Number) || entity.Number.ToLower().Contains("удалить"))
                        {
                            context.Set<ControlWeld>().Remove(entity);
                        }
                    }
                    List<ControlWeldJournal> list27jr = await context.ControlWeldJournals.ToListAsync();
                    foreach (ControlWeldJournal entity in list27jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<ControlWeldJournal>().Remove(entity);
                        }
                    }

                    List<RolledMaterial> list28 = await context.RolledMaterials.ToListAsync();
                    foreach (RolledMaterial entity in list28)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<RolledMaterial>().Remove(entity);
                        }
                    }
                    List<RolledMaterialJournal> list28jr = await context.RolledMaterialJournals.ToListAsync();
                    foreach (RolledMaterialJournal entity in list28jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<RolledMaterialJournal>().Remove(entity);
                        }
                    }

                    List<SheetMaterial> list29 = await context.SheetMaterials.ToListAsync();
                    foreach (SheetMaterial entity in list29)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<SheetMaterial>().Remove(entity);
                        }
                    }
                    List<SheetMaterialJournal> list29jr = await context.SheetMaterialJournals.ToListAsync();
                    foreach (SheetMaterialJournal entity in list29jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<SheetMaterialJournal>().Remove(entity);
                        }
                    }

                    List<WeldingMaterial> list30 = await context.WeldingMaterials.ToListAsync();
                    foreach (WeldingMaterial entity in list30)
                    {
                        if (String.IsNullOrEmpty(entity.Certificate) || entity.Certificate.ToLower().Contains("удалить"))
                        {
                            context.Set<WeldingMaterial>().Remove(entity);
                        }
                    }
                    List<WeldingMaterialJournal> list30jr = await context.WeldingMaterialJournals.ToListAsync();
                    foreach (WeldingMaterialJournal entity in list30jr)
                    {
                        if (entity.DetailId is null)
                        {
                            context.Set<WeldingMaterialJournal>().Remove(entity);
                        }
                    }

                    return context.SaveChangesAsync();
                });
                
                if (Deleating.Result > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Очистка базы данных завершена. Очищенная база находится на рабочем столе. Разместить базу на сервере с заменой?", "SuperVision", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        CopyDbFileToServer();
                    }
                }
            }
            IsBusy = false;
        }

        private void CopyDbFileFromServer()
        {
            File.Copy(@"T:\06-01-06 - БДУКП\СМТО ОП ПУТН\SupervisionData\SupervisionDataVol2.sqlite",
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\SupervisionDataVol2.sqlite",
                true);
        }

        private void CopyDbFileToServer()
        {
            File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)+@"\SupervisionDataVol2.sqlite",
                @"T:\06-01-06 - БДУКП\СМТО ОП ПУТН\SupervisionData\SupervisionDataVol2.sqlite",
                true);
        }

        public static ServiceCommandsVM LoadVM()
        {
            ServiceCommandsVM vm = new ServiceCommandsVM();
            return vm;
        }

        public ServiceCommandsVM()
        {
            LoadCommand = new Supervision.Commands.AsyncCommand(Load);
        }
    }
}
