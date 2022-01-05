using DataLayer.Files;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class Column : ValveCoverAssemblyDetail
    {

        public int? RunningSleeveId { get; set; }
        public RunningSleeve RunningSleeve { get; set; }

        public int? PIDId { get; set; }
        public PID PID { get; set; }
        public string DN { get; set; }

        [NotMapped]
        public new string FullName => string.Format($"№{Number}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, №{Number}");

        public Column()
        {
            Name = "Стойка";
        }

        public Column(Column column) : base(column)
        {
            DN = column.DN;
            Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер:");
        }

        public ObservableCollection<ColumnJournal> ColumnJournals { get; set; }
        public ObservableCollection<ColumnWithFile> Files { get; set; }
    }
}
