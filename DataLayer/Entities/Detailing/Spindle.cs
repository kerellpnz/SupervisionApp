using DataLayer.Files;
using DataLayer.Journals.Detailing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class Spindle : ValveCoverAssemblyDetail
    {
        public int? PIDId { get; set; }
        public PID PID { get; set; }

        public string ZK { get; set; }

        public string DN { get; set; }

        public string Material { get; set; }

        public string Melt { get; set; }

        [NotMapped]
        public new string FullName => string.Format($"ЗК №{ZK}-{Number}/пл.{Melt}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {ZK}-{Number}, пл.{Melt}");

        public Spindle()
        {
            Name = "Шпиндель";
        }

        public Spindle(Spindle spindle) : base(spindle)
        {
            ZK = spindle.ZK;
            DN = spindle.DN;
            MetalMaterial = spindle.MetalMaterial;
            MetalMaterialId = spindle.MetalMaterialId;
            Number = spindle.Number;
            Melt = spindle.Melt;
            Material = spindle.Material;
        }

        public ObservableCollection<SpindleJournal> SpindleJournals { get; set; }
        public ObservableCollection<SpindleWithFile> Files { get; set; }
    }
}
