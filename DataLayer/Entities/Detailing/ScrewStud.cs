using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Files;
using DataLayer.Journals.Detailing;

namespace DataLayer.Entities.Detailing
{
    public class ScrewStud : Fasteners
    {
        public ScrewStud()
        {
            Name = "Шпилька";
            Material = "30ХМА";
        }

        public ScrewStud(ScrewStud screwStud) : base(screwStud)
        {

        }

        

        [NotMapped]
        public new string FullName => string.Format($"Сертификат №{Certificate}/Партия: {Number}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {Number}, серт.№{Certificate}");

        public ObservableCollection<BaseValveWithScrewStud> BaseValveWithScrewStuds { get; set; }

        public ObservableCollection<ScrewStudJournal> ScrewStudJournals { get; set; }
        public ObservableCollection<ScrewStudWithFile> Files { get; set; }
    }
}
