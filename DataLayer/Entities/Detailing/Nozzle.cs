using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class Nozzle : BaseDetail
    {
        public Nozzle()
        {
            Name = "Катушка";
            Material = "09Г2С";
        }

        [NotMapped]
        public new string FullName => string.Format($"ЗК №{ZK}-{Number}/пл.{Melt}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {ZK}-{Number}, пл.{Melt}");

        public Nozzle(Nozzle nozzle) : base(nozzle)
        {
            Grooving = nozzle.Grooving;
            TensileStrength = nozzle.TensileStrength;
            MetalMaterialId = nozzle.MetalMaterialId;
            MetalMaterial = nozzle.MetalMaterial;
            //ForgingMaterial = nozzle.ForgingMaterial;
            //ForgingMaterialId = nozzle.ForgingMaterialId;
            PN = nozzle.PN;
            DN = nozzle.DN;
            ZK = nozzle.ZK;
            Diameter = nozzle.Diameter;
            PID = nozzle.PID;
            PIDId = nozzle.PIDId;
            Number = nozzle.Number;
            Melt = nozzle.Melt;
            Material = nozzle.Material;
        }

        public string PN { get; set; }
        public string ZK { get; set; }
        public string DN { get; set; }
        public string Diameter { get; set; }

        public int? PIDId { get; set; }
        public PID PID { get; set; }


        public string TensileStrength { get; set; }
        public string Grooving { get; set; }

        public int? MetalMaterialId { get; set; }
        public MetalMaterial MetalMaterial { get; set; }

        public int? ForgingMaterialId { get; set; }
        public ForgingMaterial ForgingMaterial { get; set; }

        public int? BaseValveId { get; set; }
        public BaseValve BaseValve { get; set; }

        public ObservableCollection<NozzleJournal> NozzleJournals { get; set; }
        public ObservableCollection<NozzleWithFile> Files { get; set; }

    }
}
