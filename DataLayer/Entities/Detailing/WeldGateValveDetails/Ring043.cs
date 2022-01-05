using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class Ring043 : BaseDetail
    {
        public Ring043()
        {
            Name = "Кольцо";
            Material = "09Г2С";
        }

        public Ring043(Ring043 ring) : base(ring)
        {
            MetalMaterialId = ring.MetalMaterialId;
            MetalMaterial = ring.MetalMaterial;
            ZK = ring.ZK;
            DN = ring.DN;
            Number = ring.Number;
            Melt = ring.Melt;
            Material = ring.Material;
        }

        public string DN { get; set; }
        public string ZK { get; set; }

        public int? MetalMaterialId { get; set; }
        public MetalMaterial MetalMaterial { get; set; }

        public int? ForgingMaterialId { get; set; }
        public ForgingMaterial ForgingMaterial { get; set; }

        public int? BaseWeldValveId { get; set; }
        public BaseWeldValveDetail BaseWeldValve { get; set; }

        public ObservableCollection<Ring043Journal> Ring043Journals { get; set; }
        public ObservableCollection<Ring043WithFile> Files { get; set; }


        [NotMapped]
        public new string FullName => string.Format($"ЗК №{ZK}-{Number}/пл.{Melt}/чер.{Drawing}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {ZK}-{Number}, чер.{Drawing}, пл.{Melt}");
    }
}
