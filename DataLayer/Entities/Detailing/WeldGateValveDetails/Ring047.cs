using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class Ring047 : BaseDetail
    {
        public Ring047()
        {
            Name = "Кольцо";
        }

        public Ring047(Ring047 ring) : base(ring)
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

        public WeldGateValveCase WeldGateValveCase { get; set; }

        public ObservableCollection<Ring047Journal> Ring047Journals { get; set; }
        public ObservableCollection<Ring047WithFile> Files { get; set; }


        [NotMapped]
        public new string FullName => string.Format($"ЗК №{ZK}-{Number}/пл.{Melt}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {ZK}-{Number}, пл.{Melt}");
    }
}
