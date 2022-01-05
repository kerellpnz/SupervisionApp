using System;
using System.Collections.ObjectModel;

namespace DataLayer.Files
{
    public class ElectronicDocument : BaseTable
    {
        public FileType FileType { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string FilePath { get; set; }

        public ObservableCollection<AssemblyUnitSealsWithFile> AssemblyUnitSeals{ get; set; }
        
        public ObservableCollection<BaseAssemblyUnitWithFile> BaseAssemblyUnits { get; set; }
        public ObservableCollection<BaseAnticorrosiveCoatingWithFile> BaseAnticorrosiveCoatings { get; set; }
        
        public ObservableCollection<BaseValveCoverWithFile> BaseValveCovers { get; set; }
        
        public ObservableCollection<CaseBottomWithFile> CaseBottoms { get; set; }
        
        
        public ObservableCollection<ControlWeldWithFile> ControlWelds { get; set; }
        public ObservableCollection<CounterFlangeWithFile> CounterFlanges { get; set; }
        public ObservableCollection<CoverFlangeWithFile> CoverFlanges { get; set; }
        
        public ObservableCollection<CoverSleeveWithFile> CoverSleeves { get; set; }
        public ObservableCollection<CoverSleeve008WithFile> CoverSleeves008 { get; set; }
        public ObservableCollection<Ring043WithFile> Rings043 { get; set; }
        public ObservableCollection<Ring047WithFile> Rings047 { get; set; }
        public ObservableCollection<FrontalSaddleSealingWithFile> FrontalSaddleSeals { get; set; }
        
        public ObservableCollection<GateWithFile> Gates { get; set; }
        public ObservableCollection<MetalMaterialWithFile> MetalMaterials { get; set; }
        public ObservableCollection<NozzleWithFile> Nozzles { get; set; }
        public ObservableCollection<PIDWithFile> PIDs { get; set; }
        public ObservableCollection<RunningSleeveWithFile> RunningSleeves { get; set; }
        public ObservableCollection<SaddleWithFile> Saddles { get; set; }
        public ObservableCollection<ScrewNutWithFile> ScrewNuts { get; set; }
        public ObservableCollection<ScrewStudWithFile> ScrewStuds { get; set; }
        
        public ObservableCollection<ShearPinWithFile> ShearPins { get; set; }
        
        public ObservableCollection<SpecificationWithFile> Specifications { get; set; }
        public ObservableCollection<SpindleWithFile> Spindles { get; set; }
        public ObservableCollection<ColumnWithFile> Columns { get; set; }
        public ObservableCollection<SpringWithFile> Springs { get; set; }
        
        public ObservableCollection<WeldGateValveCaseWithFile> WeldGateValveCases { get; set; }
        public ObservableCollection<WeldingMaterialWithFile> WeldingMaterials { get; set; }
        

        public ElectronicDocument()
        {
        }

        public ElectronicDocument(string number, FileType fileType, DateTime date, string filePath)
        {
            Number = number;
            Date = date;
            FilePath = filePath;
            FileType = fileType;
        }
    }
}