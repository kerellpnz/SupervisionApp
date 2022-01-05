using DataLayer.Journals.Detailing;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Journals.AssemblyUnits;

using DataLayer.Journals;

using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using DataLayer.Journals.Materials;
using DataLayer.Journals.Periodical;
using System.Collections.ObjectModel;

namespace DataLayer
{
    public class Inspector : BaseTable
    {
        public string Name { get; set; }

        public string Apointment { get; set; }

        public string Subdivision { get; set; }

        public string Department { get; set; }

        [NotMapped] public string FullName => string.Format($"{Name}\n{Apointment}");

        
        public ObservableCollection<CoatingJournal> CoatingJournals { get; set; }
        
        public ObservableCollection<SheetGateValveJournal> SheetGateValveJournals { get; set; }
        
        public ObservableCollection<CaseBottomJournal> CaseBottomJournals { get; set; }
        
        
        public ObservableCollection<CoverFlangeJournal> CoverFlangeJournals { get; set; }
        public ObservableCollection<CoverSleeveJournal> CoverSleeveJournals { get; set; }
        public ObservableCollection<CoverSleeve008Journal> CoverSleeve008Journals { get; set; }
        public ObservableCollection<Ring043Journal> Ring043Journals { get; set; }
        public ObservableCollection<Ring047Journal> Ring047Journals { get; set; }
        public ObservableCollection<ColumnJournal> ColumnJournals { get; set; }


        public ObservableCollection<SheetGateValveCaseJournal> SheetGateValveCaseJournals { get; set; }
        public ObservableCollection<SheetGateValveCoverJournal> SheetGateValveCoverJournals { get; set; }
        
        
        public ObservableCollection<AssemblyUnitSealingJournal> AssemblyUnitSealingJournals { get; set; }
        public ObservableCollection<MainFlangeSealingJournal> MainFlangeSealingJournals { get; set; }

        public ObservableCollection<CounterFlangeJournal> CounterFlangeJournals { get; set; }
        
        public ObservableCollection<FrontalSaddleSealingJournal> FrontalSaddleSealingJournals { get; set; }
        public ObservableCollection<GateJournal> GateJournals { get; set; }
        public ObservableCollection<NozzleJournal> NozzleJournals { get; set; }
        public ObservableCollection<RunningSleeveJournal> RunningSleeveJournals { get; set; }
        public ObservableCollection<SaddleJournal> SaddleJournals { get; set; }
        public ObservableCollection<ScrewNutJournal> ScrewNutJournals { get; set; }
        public ObservableCollection<ScrewStudJournal> ScrewStudJournals { get; set; }
        public ObservableCollection<ShearPinJournal> ShearPinJournals { get; set; }
        public ObservableCollection<SpindleJournal> SpindleJournals { get; set; }
        public ObservableCollection<SpringJournal> SpringJournals { get; set; }
        public ObservableCollection<AbovegroundCoatingJournal> AbovegroundCoatingJournals { get; set; }
        public ObservableCollection<AbrasiveMaterialJournal> AbrasiveMaterialJournals { get; set; }
        public ObservableCollection<UndercoatJournal> UndercoatJournals { get; set; }
        public ObservableCollection<UndergroundCoatingJournal> UndergroundCoatingJournals{ get; set; }
        public ObservableCollection<ForgingMaterialJournal> ForgingMaterialJournals { get; set; }
        public ObservableCollection<PipeMaterialJournal> PipeMaterialJournals { get; set; }
        public ObservableCollection<RolledMaterialJournal> RolledMaterialJournals { get; set; }
        public ObservableCollection<SheetMaterialJournal> SheetMaterialJournals { get; set; }
        public ObservableCollection<WeldingMaterialJournal> WeldingMaterialJournals { get; set; }
        public ObservableCollection<ControlWeldJournal> ControlWeldJournals { get; set; }
        public ObservableCollection<StoresControlJournal> StoresControlJournals { get; set; }
        public ObservableCollection<FactoryInspectionJournal> FactoryInspectionJournals { get; set; }
        public ObservableCollection<NDTControlJournal> NDTControls { get; set; }
        public ObservableCollection<WeldingProceduresJournal> WeldingProceduresJournals { get; set; }
        public ObservableCollection<CoatingChemicalCompositionJournal> CoatingChemicalCompositionJournals { get; set; }
        public ObservableCollection<DegreasingChemicalCompositionJournal> DegreasingChemicalCompositionJournals { get; set; }
        public ObservableCollection<CoatingPlasticityJournal> CoatingPlasticityJournals { get; set; }
        public ObservableCollection<CoatingPorosityJournal> CoatingPorosityJournals { get; set; }
        public ObservableCollection<CoatingProtectivePropertiesJournal> CoatingProtectivePropertiesJournals { get; set; }
        public ObservableCollection<PIDJournal> PIDJournals { get; set; }
    }
}
