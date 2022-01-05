using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class CoverSleeve008 : BaseDetail
    {

        [NotMapped]
        public new string FullName => string.Format($"ЗК №{ZK}-{Number}/пл.{Melt}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {ZK}-{Number}, пл.{Melt}");

        public CoverSleeve008()
        {
            Name = "Втулка(008)";
        }

        public CoverSleeve008(CoverSleeve008 sleeve008) : base(sleeve008)
        {
            MetalMaterialId = sleeve008.MetalMaterialId;
            MetalMaterial = sleeve008.MetalMaterial;
            ZK = sleeve008.ZK;
            DN = sleeve008.DN;
            Number = sleeve008.Number;
            Melt = sleeve008.Melt;
            Material = sleeve008.Material;
        }

        public string DN { get; set; }
        public string ZK { get; set; }
       
      
        public int? MetalMaterialId { get; set; }
        public MetalMaterial MetalMaterial { get; set; }

        public int? CoverSealingRingId { get; set; }
        

        public BaseWeldValveDetail BaseWeldValveDetail { get; set; }

        public ObservableCollection<CoverSleeve008Journal> CoverSleeve008Journals { get; set; }
        public ObservableCollection<CoverSleeve008WithFile> Files { get; set; }
    }
}
