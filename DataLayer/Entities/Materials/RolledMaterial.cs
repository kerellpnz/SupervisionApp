using DataLayer.Journals.Materials;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Materials
{
    public class RolledMaterial : MetalMaterial
    {
        public RolledMaterial()
        {
            Name = "Прокат";
        }

        [NotMapped]
        public new string FullName => string.Format($"{Melt}пл./Ф{FirstSize}/{Material}| Сертификат: №{Certificate} - {Status}");

        [NotMapped]
        public new string NameForReport => string.Format($"Серт.№{Certificate}, пл.{Melt}, ф{FirstSize}, {Material}");


        public RolledMaterial(RolledMaterial material) : base(material)
        {
            Number = material.Number;
        }

        public ObservableCollection<RolledMaterialJournal> RolledMaterialJournals { get; set; }
    }
}
