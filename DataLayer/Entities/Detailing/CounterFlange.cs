using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class CounterFlange : BaseEntity
    {

       [NotMapped]
        public new string FullName => string.Format($"DN{DN}/№{Number}/пл.{ForgingMaterial?.Melt}-{Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, №{Number}, пл.{ForgingMaterial?.Melt}");

        public CounterFlange()
        {
            Name = "Фланец ответный";
        }

        public CounterFlange(CounterFlange flange) : base(flange)
        {
            //ForgingMaterialId = flange.ForgingMaterialId;
        }

        public string DN { get; set; }
        public string ThicknessJoining { get; set; }

        public int? ForgingMaterialId { get; set; }
        public ForgingMaterial ForgingMaterial { get; set; }

        public int? BaseValveId { get; set; }
        public BaseValve BaseValve { get; set; }

        public ObservableCollection<CounterFlangeJournal> CounterFlangeJournals { get; set; }
        public ObservableCollection<CounterFlangeWithFile> Files { get; set; }
    }
}
