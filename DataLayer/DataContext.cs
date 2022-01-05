using System.Reflection;
using Microsoft.EntityFrameworkCore;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.SheetGateValveDetails;
using DataLayer.TechnicalControlPlans.AssemblyUnits;
using DataLayer.TechnicalControlPlans.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.Journals;
using DataLayer.Journals.AssemblyUnits;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using DataLayer.TechnicalControlPlans.Materials;
using DataLayer.Journals.Materials;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.TechnicalControlPlans.Materials.AnticorrosiveCoating;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using DataLayer.TechnicalControlPlans;
using DataLayer.Entities.Periodical;
using DataLayer.TechnicalControlPlans.Periodical;
using DataLayer.Journals.Periodical;
using DataLayer.Files;

namespace DataLayer
{
    public sealed class DataContext : DbContext
    {
        public DbSet<OperationType> OperationTypes { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Specification> Specifications{ get; set; }
        public DbSet<PID> PIDs { get; set; }
        public DbSet<PIDTCP> PIDTCPs { get; set; }
        public DbSet<PIDJournal> PIDJournals { get; set; }
        public DbSet<Inspector> Inspectors { get; set; }
        public DbSet<JournalNumber> JournalNumbers { get; set; }
        public DbSet<VersionControl> VersionControl { get; set; }

        public DbSet<MainFlangeSealControl> MainFlangeSealControl { get; set; }

        public DbSet<CoatingPlasticityTCP> CoatingPlasticityTCPs { get; set; }
        public DbSet<CoatingPlasticityJournal> CoatingPlasticityJournals { get; set; }

        public DbSet<CoatingPorosityTCP> CoatingPorosityTCPs { get; set; }
        public DbSet<CoatingPorosityJournal> CoatingPorosityJournals { get; set; }

        public DbSet<CoatingProtectivePropertiesTCP> CoatingProtectivePropertiesTCPs { get; set; }
        public DbSet<CoatingProtectivePropertiesJournal> CoatingProtectivePropertiesJournals { get; set; }

        public DbSet<DegreasingChemicalCompositionTCP> DegreasingChemicalCompositionTCPs { get; set; }
        public DbSet<DegreasingChemicalCompositionJournal> DegreasingChemicalCompositionJournals { get; set; }

        public DbSet<CoatingChemicalCompositionTCP> CoatingChemicalCompositionTCPs { get; set; }
        public DbSet<CoatingChemicalCompositionJournal> CoatingChemicalCompositionJournals { get; set; }

        public DbSet<StoresControlTCP> StoresControlTCPs { get; set; }
        public DbSet<StoresControlJournal> StoresControlJournals { get; set; }

        public DbSet<FactoryInspectionTCP> FactoryInspectionTCPs { get; set; }
        public DbSet<FactoryInspectionJournal> FactoryInspectionJournals { get; set; }

        public DbSet<WeldingProcedures> WeldingProcedures { get; set; }
        public DbSet<WeldingProceduresTCP> WeldingProceduresTCPs { get; set; }
        public DbSet<WeldingProceduresJournal> WeldingProceduresJournals { get; set; }

        public DbSet<NDTControl> NDTControls { get; set; }
        public DbSet<NDTControlTCP> NDTControlTCPs { get; set; }
        public DbSet<NDTControlJournal> NDTControlJournals { get; set; }

        public DbSet<ControlWeld> ControlWelds { get; set; }
        public DbSet<ControlWeldTCP> ControlWeldTCPs { get; set; }
        public DbSet<ControlWeldJournal> ControlWeldJournals { get; set; }

        public DbSet<CoatingTCP> CoatingTCPs { get; set; }
        public DbSet<CoatingJournal> CoatingJournals { get; set; }

        
        public DbSet<Nozzle> Nozzles { get; set; }
        public DbSet<NozzleTCP> NozzleTCPs { get; set; }
        public DbSet<NozzleJournal> NozzleJournals { get; set; }

        
        public DbSet<BaseValveWithCoating> BaseValveWithCoatings { get; set; }

        public DbSet<BaseAssemblyUnit> BaseAssemblyUnit { get; set; }
        public DbSet<SheetGateValve> SheetGateValves { get; set; }
        public DbSet<SheetGateValveTCP> SheetGateValveTCPs { get; set; }
        public DbSet<SheetGateValveJournal> SheetGateValveJournals { get; set; }

   

        public DbSet<Spindle> Spindles { get; set; }
        public DbSet<SpindleTCP> SpindleTCPs { get; set; }
        public DbSet<SpindleJournal> SpindleJournals { get; set; }

        public DbSet<Column> Columns { get; set; }
        public DbSet<ColumnTCP> ColumnTCPs { get; set; }
        public DbSet<ColumnJournal> ColumnJournals { get; set; }

        

        public DbSet<CoverSleeve> CoverSleeves { get; set; }
        public DbSet<CoverSleeveTCP> CoverSleeveTCPs { get; set; }
        public DbSet<CoverSleeveJournal> CoverSleeveJournals { get; set; }

        public DbSet<Ring047> Rings047 { get; set; }
        public DbSet<Ring047TCP> Ring047TCPs { get; set; }
        public DbSet<Ring047Journal> Ring047Journals { get; set; }

        public DbSet<Ring043> Rings043 { get; set; }
        public DbSet<Ring043TCP> Ring043TCPs { get; set; }
        public DbSet<Ring043Journal> Ring043Journals { get; set; }

        public DbSet<CoverSleeve008> CoverSleeves008 { get; set; }
        public DbSet<CoverSleeve008TCP> CoverSleeve008TCPs { get; set; }
        public DbSet<CoverSleeve008Journal> CoverSleeve008Journals { get; set; }


        

        

        public DbSet<RunningSleeve> RunningSleeves { get; set; }
        public DbSet<RunningSleeveTCP> RunningSleeveTCPs { get; set; }
        public DbSet<RunningSleeveJournal> RunningSleeveJournals { get; set; }

        public DbSet<CoverFlange> CoverFlanges { get; set; }
        public DbSet<CoverFlangeTCP> CoverFlangeTCPs { get; set; }
        public DbSet<CoverFlangeJournal> CoverFlangeJournals { get; set; }

        

        public DbSet<SheetGateValveCover> SheetGateValveCovers { get; set; }
        public DbSet<SheetGateValveCoverTCP> SheetGateValveCoverTCPs { get; set; }
        public DbSet<SheetGateValveCoverJournal> SheetGateValveCoverJournals { get; set; }

        public DbSet<Gate> Gates { get; set; }
        public DbSet<GateTCP> GateTCPs { get; set; }
        public DbSet<GateJournal> GateJournals { get; set; }

        public DbSet<Saddle> Saddles { get; set; }
        public DbSet<SaddleTCP> SaddleTCPs { get; set; }
        public DbSet<SaddleJournal> SaddleJournals { get; set; }

        public DbSet<CaseBottom> CaseBottoms { get; set; }
        public DbSet<CaseBottomTCP> CaseBottomTCPs { get; set; }
        public DbSet<CaseBottomJournal> CaseBottomJournals { get; set; }      
               

        public DbSet<SheetGateValveCase> SheetGateValveCases { get; set; }
        public DbSet<SheetGateValveCaseTCP> SheetGateValveCaseTCPs { get; set; }
        public DbSet<SheetGateValveCaseJournal> SheetGateValveCaseJournals { get; set; }

       

        public DbSet<FrontalSaddleSealing> FrontalSaddleSeals { get; set; }
        public DbSet<FrontalSaddleSealingTCP> FrontalSaddleSealingTCPs { get; set; }
        public DbSet<FrontalSaddleSealingJournal> FrontalSaddleSealingJournals { get; set; }
        public DbSet<SaddleWithSealing> SaddleWithSeals { get; set; }

        public DbSet<AssemblyUnitSealing> AssemblyUnitSeals{ get; set; }
        public DbSet<AssemblyUnitSealingTCP> AssemblyUnitSealingTCPs{ get; set; }
        public DbSet<AssemblyUnitSealingJournal> AssemblyUnitSealingJournals { get; set; }
        public DbSet<MainFlangeSealing> MainFlangeSeals { get; set; }
        public DbSet<MainFlangeSealingTCP> MainFlangeSealingTCPs { get; set; }
        public DbSet<MainFlangeSealingJournal> MainFlangeSealingJournals { get; set; }
        public DbSet<BaseValveWithSealing> BaseValveWithSeals { get; set; }

        public DbSet<ScrewStud> ScrewStuds { get; set; }
        public DbSet<ScrewStudTCP> ScrewStudTCPs { get; set; }
        public DbSet<ScrewStudJournal> ScrewStudJournals { get; set; }
        public DbSet<BaseValveWithScrewStud> BaseValveWithScrewStuds { get; set; }

        public DbSet<ScrewNut> ScrewNuts { get; set; }
        public DbSet<ScrewNutTCP> ScrewNutTCPs { get; set; }
        public DbSet<ScrewNutJournal> ScrewNutJournals { get; set; }
        public DbSet<BaseValveWithScrewNut> BaseValveWithScrewNuts { get; set; }

        public DbSet<Spring> Springs { get; set; }
        public DbSet<SpringTCP> SpringTCPs { get; set; }
        public DbSet<SpringJournal> SpringJournals { get; set; }
        public DbSet<BaseValveWithSpring> BaseValveWithSprings { get; set; }

        public DbSet<ShearPin> ShearPins { get; set; }
        public DbSet<ShearPinTCP> ShearPinTCPs { get; set; }
        public DbSet<ShearPinJournal> ShearPinJournals { get; set; }
        public DbSet<BaseValveWithShearPin> BaseValveWithShearPins { get; set; }

        public DbSet<CounterFlange> CounterFlanges { get; set; }
        public DbSet<CounterFlangeTCP> CounterFlangeTCPs { get; set; }
        public DbSet<CounterFlangeJournal> CounterFlangeJournals { get; set; }

        

        public DbSet<MetalMaterial> MetalMaterials { get; set; }
        public DbSet<MetalMaterialTCP> MetalMaterialTCPs { get; set; }

        public DbSet<SheetMaterial> SheetMaterials { get; set; }
        public DbSet<SheetMaterialJournal> SheetMaterialJournals { get; set; }

        public DbSet<ForgingMaterial> ForgingMaterials { get; set; }
        public DbSet<ForgingMaterialJournal> ForgingMaterialJournals { get; set; }
        public DbSet<ForgingMaterialTCP> ForgingMaterialTCPs { get; set; }

        public DbSet<RolledMaterial> RolledMaterials { get; set; }
        public DbSet<RolledMaterialJournal> RolledMaterialJournals { get; set; }

        public DbSet<PipeMaterial> PipeMaterials { get; set; }
        public DbSet<PipeMaterialJournal> PipeMaterialJournals { get; set; }

        public DbSet<BaseAnticorrosiveCoating> BaseAnticorrosiveCoatings { get; set; }
        public DbSet<AnticorrosiveCoatingTCP> AnticorrosiveCoatingTCPs { get; set; }

        public DbSet<AbovegroundCoating> AbovegroundCoatings { get; set; }
        public DbSet<AbovegroundCoatingJournal> AbovegroundCoatingJournals { get; set; }

        public DbSet<AbrasiveMaterial> AbrasiveMaterials { get; set; }
        public DbSet<AbrasiveMaterialJournal> AbrasiveMaterialJournals { get; set; }

        public DbSet<Undercoat> Undercoats { get; set; }
        public DbSet<UndercoatJournal> UndercoatJournals { get; set; }

        public DbSet<UndergroundCoating> UndergroundCoatings { get; set; }
        public DbSet<UndergroundCoatingJournal> UndergroundCoatingJournals { get; set; }

        public DbSet<WeldingMaterial> WeldingMaterials { get; set; }
        public DbSet<WeldingMaterialTCP> WeldingMaterialTCPs { get; set; }
        public DbSet<WeldingMaterialJournal> WeldingMaterialJournals { get; set; }

        public DbSet<ElectronicDocument> ElectronicDocuments { get; set; }
        public DbSet<SpecificationWithFile> SpecificationWithFiles { get; set; }
        public DbSet<BaseAssemblyUnitWithFile> BaseAssemblyUnitWithFiles { get; set; }
        
        public DbSet<AssemblyUnitSealsWithFile> AssemblyUnitSealsWithFiles { get; set; }

        public DbSet<MainFlangeSealsWithFile> MainFlangeSealsWithFiles { get; set; }
        public DbSet<BaseAnticorrosiveCoatingWithFile> BaseAnticorrosiveCoatingWithFiles { get; set; }
        public DbSet<BaseValveCoverWithFile> BaseValveCoverWithFiles { get; set; }
        
        public DbSet<CaseBottomWithFile> CaseBottomWithFiles { get; set; }
        
       
        public DbSet<ControlWeldWithFile> ControlWeldWithFiles { get; set; }
        public DbSet<CounterFlangeWithFile> CounterFlangeWithFiles { get; set; }
        public DbSet<CoverFlangeWithFile> CoverFlangeWithFiles { get; set; }
        
        public DbSet<CoverSleeveWithFile> CoverSleeveWithFiles { get; set; }
        public DbSet<CoverSleeve008WithFile> CoverSleeve008WithFiles { get; set; }
        public DbSet<Ring043WithFile> Ring043WithFiles { get; set; }
        public DbSet<Ring047WithFile> Ring047WithFiles { get; set; }
        public DbSet<FrontalSaddleSealingWithFile> FrontalSaddleSealingWithFiles { get; set; }
        
        public DbSet<GateWithFile> GateWithFiles { get; set; }
        public DbSet<MetalMaterialWithFile> MetalMaterialWithFiles { get; set; }
        public DbSet<NozzleWithFile> NozzleWithFiles { get; set; }
        
        public DbSet<PIDWithFile> PIDWithFiles { get; set; }
        public DbSet<RunningSleeveWithFile> RunningSleeveWithFiles { get; set; }
        public DbSet<ColumnWithFile> ColumnWithFiles { get; set; }
        public DbSet<SaddleWithFile> SaddleWithFiles { get; set; }
        public DbSet<ScrewNutWithFile> ScrewNutWithFiles { get; set; }
        public DbSet<ScrewStudWithFile> ScrewStudWithFiles { get; set; }
        
        public DbSet<ShearPinWithFile> ShearPinWithFiles { get; set; }
        
        
        public DbSet<SpindleWithFile> SpindleWithFiles { get; set; }
        public DbSet<SpringWithFile> SpringWithFiles { get; set; }
        
        public DbSet<WeldGateValveCaseWithFile> WeldGateValveCaseWithFiles { get; set; }

        //public DbSet<BaseWeldValveDetailWithFile> BaseWeldValveDetailWithFiles { get; set; }
        public DbSet<WeldingMaterialWithFile> WeldingMaterialWithFiles { get; set; }
        

        //TODO: Не забываем добавлять все таблицы

        public DataContext()
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
            //Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OperationType>().HasData(new OperationType[]
                {
                new OperationType { Id=1, Name="Входной контроль"},
                new OperationType { Id=2, Name="Механическая обработка"},
                new OperationType { Id=3, Name="Неразрушающий контроль"},
                new OperationType { Id=4, Name="Сборка"},
                new OperationType { Id=5, Name="ПСИ"},
                new OperationType { Id=6, Name="ВИК после ПСИ"},
                new OperationType { Id=7, Name="АКП"},
                new OperationType { Id=8, Name="Документация"},
                new OperationType { Id=9, Name="Отгрузка"},
                new OperationType { Id=10, Name="Входной контроль (НК)"},
                new OperationType { Id=11, Name="Сборка/Сварка"},
                new OperationType { Id=12, Name="Подготовка к сборке"},
                new OperationType { Id=13, Name="Наплавка"},
                new OperationType { Id=14, Name="Подготовка поверхности"},
                new OperationType { Id=15, Name="Покрытие"}
                });
        }

            
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.UseSqlite("Filename=SupervisionDataVol2.sqlite", options => options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName));
#else
            
            optionsBuilder.UseSqlite(@"Filename = T:\06-01-06 - БДУКП\СМТО ОП ПУТН\SupervisionData\SupervisionDataVol2.sqlite", options => options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName));

#endif
            base.OnConfiguring(optionsBuilder);
        }
    }
}
