using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Files;
using DataLayer.Journals.Detailing;

namespace DataLayer.Entities.Detailing
{
    public class ScrewNut : Fasteners
    {
        public ScrewNut()
        {
            Name = "Гайка";
            Material = "40Х";
        }

        public ScrewNut(ScrewNut screwNut) : base(screwNut)
        {
            
        }

        

        [NotMapped]
        public new string FullName => string.Format($"Сертификат №{Certificate}/Партия: {Number}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {Number}, серт.№{Certificate}");

        public ObservableCollection<BaseValveWithScrewNut> BaseValveWithScrewNuts { get; set; }

        public ObservableCollection<ScrewNutJournal> ScrewNutJournals { get; set; }
        public ObservableCollection<ScrewNutWithFile> Files { get; set; }
    }
}
