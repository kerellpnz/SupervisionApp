using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Files;
using DataLayer.Journals.Detailing;

namespace DataLayer.Entities.Detailing
{
    public class Spring : BaseDetail
    {
        public Spring()
        {
            Name = "Пружина";
            Material = "51ХФА";
        }

        public Spring(Spring spring) : base(spring)
        {
            Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер:");
        }

        [NotMapped]
        public new string FullName => string.Format($"Сертификат: №{Certificate}/Партия: {Batch}/{Name} - {Status}");

        public string Batch { get; set; }

        public int Amount { get; set; }

        public int? AmountRemaining { get; set; }

        public ObservableCollection<BaseValveWithSpring> BaseValveWithSprings { get; set; }

        public ObservableCollection<SpringJournal> SpringJournals { get; set; }
        public ObservableCollection<SpringWithFile> Files { get; set; }
    }
}