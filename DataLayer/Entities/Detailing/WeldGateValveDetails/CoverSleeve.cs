using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class CoverSleeve : BaseEntity
    {

        [NotMapped]
        public new string FullName => string.Format($"ЗК №{ZK}-{Number}/пл.{Melt}/чер.{Drawing}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {ZK}-{Number}, чер.{Drawing}, пл.{Melt}");
        public CoverSleeve()
        {
            Name = "Втулка(016)";
            Material = "09Г2С";
        }

        public CoverSleeve(CoverSleeve sleeve) : base(sleeve)
        {
            MetalMaterialId = sleeve.MetalMaterialId;
            MetalMaterial = sleeve.MetalMaterial;
            //ForgingMaterial = sleeve.ForgingMaterial;
            //ForgingMaterialId = sleeve.ForgingMaterialId;
            ZK = sleeve.ZK;
            DN = sleeve.DN;
            Number = sleeve.Number;
            Material = sleeve.Material;
            Melt = sleeve.Melt;
        }

        public string Material { get; set; }
        public string Melt { get; set; }

        public string DN { get; set; }
        public string ZK { get; set; }

        public int? MetalMaterialId { get; set; }
        public MetalMaterial MetalMaterial { get; set; }

        public int? ForgingMaterialId { get; set; }
        public ForgingMaterial ForgingMaterial { get; set; }


        public int? CoverSealingRingId { get; set; }
        

        public WeldGateValveCover WeldGateValveCover { get; set; }

        public ObservableCollection<CoverSleeveJournal> CoverSleeveJournals { get; set; }
        public ObservableCollection<CoverSleeveWithFile> Files { get; set; }
    }
}
