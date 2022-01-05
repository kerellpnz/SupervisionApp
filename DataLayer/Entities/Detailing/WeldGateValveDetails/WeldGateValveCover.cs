using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Materials;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class WeldGateValveCover : BaseValveCover
    {
        

        public int? CoverSleeveId { get; set; }
        public CoverSleeve CoverSleeve { get; set; }
/*
        public int? CoverSleeve008Id { get; set; }
        public CoverSleeve008 CoverSleeve008 { get; set; }*/

        public BaseWeldValve BaseWeldValve { get; set; }

        public string BallValveDrainage { get; set; }
        public string BallValveDraining { get; set; }

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, №{Number}, пл.{Melt}");

        [NotMapped]
        public new string FullName => string.Format($"№{Number}/пл.{Melt}/DN{DN} - {Status}");

        public WeldGateValveCover()
        {
        }
        public WeldGateValveCover(WeldGateValveCover cover) : base(cover)
        {

        }
    }
}
