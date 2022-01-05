using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class Saddle : BaseDetail
    {
        public string DN { get; set; }
        public string PN { get; set; }
        public string ZK { get; set; }

        public int? PIDId { get; set; }
        public PID PID { get; set; }

        [NotMapped]
        public new string FullName => string.Format($"ЗК №{ZK}-{Number}/пл.{Melt}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {ZK}-{Number}, чер.{Drawing}, пл.{Melt}");

        public Saddle()
        {
            Name = "Обойма";
            Material = "09Г2С";
        }

        public Saddle(Saddle saddle) : base(saddle)
        {
            MetalMaterialId = saddle.MetalMaterialId;
            MetalMaterial = saddle.MetalMaterial;
            //ForgingMaterial = saddle.ForgingMaterial;
            //ForgingMaterialId = saddle.ForgingMaterialId;
            PID = saddle.PID;
            PIDId = saddle.PIDId;
            DN = saddle.DN;
            PN = saddle.PN;
            ZK = saddle.ZK;
            Number = saddle.Number;
            Melt = saddle.Melt;
            Material = saddle.Material;
        }

        public int? MetalMaterialId { get; set; }
        public MetalMaterial MetalMaterial { get; set; }

        public int? ForgingMaterialId { get; set; }
        public ForgingMaterial ForgingMaterial { get; set; }

        public int? BaseValveId{ get; set; }
        public BaseValve BaseValve { get; set; }

        public ObservableCollection<SaddleWithSealing> SaddleWithSealings { get; set; }

        public ObservableCollection<SaddleJournal> SaddleJournals { get; set; }
        public ObservableCollection<SaddleWithFile> Files { get; set; }
    }
}
