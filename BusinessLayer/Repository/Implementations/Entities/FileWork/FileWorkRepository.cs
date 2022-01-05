using DataLayer;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing;

using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.Files;
using System;

namespace BusinessLayer.Repository.Implementations.Entities.FileWork
{
    public class FileWorkRepository : Repository<ElectronicDocument>
    {
        public FileWorkRepository()
        {
        }

        public FileWorkRepository(DataContext context) : base(context)
        {
        }

        public ElectronicDocument Add(string number, FileType fileType, DateTime date, string filePath)
        {
            var file = new ElectronicDocument(number, fileType, date, filePath);
            table.Add(file);
            SaveChanges();
            return file;
        }

        public void AddFileToItem(BaseTable entity, ElectronicDocument file)
        {
            if (entity is BaseAssemblyUnit)
            {
                db.BaseAssemblyUnitWithFiles.Add(new BaseAssemblyUnitWithFile(entity.Id, file));
                SaveChanges();
            }
            
            if (entity is BaseAnticorrosiveCoating)
            {
                db.BaseAnticorrosiveCoatingWithFiles.Add(new BaseAnticorrosiveCoatingWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is AssemblyUnitSealing)
            {
                db.AssemblyUnitSealsWithFiles.Add(new AssemblyUnitSealsWithFile(entity.Id, file));
                SaveChanges();
            }
            
            if (entity is BaseValveCover)
            {
                db.BaseValveCoverWithFiles.Add(new BaseValveCoverWithFile(entity.Id, file));
                SaveChanges();
            }
            
            if (entity is CaseBottom)
            {
                db.CaseBottomWithFiles.Add(new CaseBottomWithFile(entity.Id, file));
                SaveChanges();
            }
            
            
            if (entity is ControlWeld)
            {
                db.ControlWeldWithFiles.Add(new ControlWeldWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is CounterFlange)
            {
                db.CounterFlangeWithFiles.Add(new CounterFlangeWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is CoverFlange)
            {
                db.CoverFlangeWithFiles.Add(new CoverFlangeWithFile(entity.Id, file));
                SaveChanges();
            }
            
            if (entity is CoverSleeve)
            {
                db.CoverSleeveWithFiles.Add(new CoverSleeveWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is CoverSleeve008)
            {
                db.CoverSleeve008WithFiles.Add(new CoverSleeve008WithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is Ring043)
            {
                db.Ring043WithFiles.Add(new Ring043WithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is Ring047)
            {
                db.Ring047WithFiles.Add(new Ring047WithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is FrontalSaddleSealing)
            {
                db.FrontalSaddleSealingWithFiles.Add(new FrontalSaddleSealingWithFile(entity.Id, file));
                SaveChanges();
            }
            
            if (entity is Gate)
            {
                db.GateWithFiles.Add(new GateWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is MetalMaterial)
            {
                db.MetalMaterialWithFiles.Add(new MetalMaterialWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is Nozzle)
            {
                db.NozzleWithFiles.Add(new NozzleWithFile(entity.Id, file));
                SaveChanges();
            }
            
            if (entity is PID)
            {
                db.PIDWithFiles.Add(new PIDWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is RunningSleeve)
            {
                db.RunningSleeveWithFiles.Add(new RunningSleeveWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is Column)
            {
                db.ColumnWithFiles.Add(new ColumnWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is Saddle)
            {
                db.SaddleWithFiles.Add(new SaddleWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is ScrewNut)
            {
                db.ScrewNutWithFiles.Add(new ScrewNutWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is ScrewStud)
            {
                db.ScrewStudWithFiles.Add(new ScrewStudWithFile(entity.Id, file));
                SaveChanges();
            }
            
            if (entity is ShearPin)
            {
                db.ShearPinWithFiles.Add(new ShearPinWithFile(entity.Id, file));
                SaveChanges();
            }
            
            
            
            if (entity is Specification)
            {
                db.SpecificationWithFiles.Add(new SpecificationWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is Spindle)
            {
                db.SpindleWithFiles.Add(new SpindleWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is Spring)
            {
                db.SpringWithFiles.Add(new SpringWithFile(entity.Id, file));
                SaveChanges();
            }
            
            if (entity is WeldGateValveCase)
            {
                db.WeldGateValveCaseWithFiles.Add(new WeldGateValveCaseWithFile(entity.Id, file));
                SaveChanges();
            }
            if (entity is WeldingMaterial)
            {
                db.WeldingMaterialWithFiles.Add(new WeldingMaterialWithFile(entity.Id, file));
                SaveChanges();
            }
            
        }
    }
}
