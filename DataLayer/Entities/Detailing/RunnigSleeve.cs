using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Files;
using DataLayer.Journals.Detailing;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class RunningSleeve : BaseDetail
    {
        public RunningSleeve()
        {
            Name = "Втулка резьбовая";
        }

        public string DN { get; set; }
        public string ZK { get; set; }

        [NotMapped]
        public new string FullName => string.Format($"{ZK}-{Number}/DN{DN}-{Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {ZK}-{Number}");

        public RunningSleeve(RunningSleeve sleeve) : base(sleeve)
        {
            ZK = sleeve.ZK;
            DN = sleeve.DN;
            Number = sleeve.Number;
        }

        public Column Column { get; set; }

        public ObservableCollection<RunningSleeveJournal> RunningSleeveJournals { get; set; }
        public ObservableCollection<RunningSleeveWithFile> Files { get; set; }
    }
}
