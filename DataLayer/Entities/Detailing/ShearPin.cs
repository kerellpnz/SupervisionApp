using DataLayer.Journals.Detailing;
using DataLayer.Entities.AssemblyUnits;
using System.Collections.ObjectModel;
using DataLayer.Files;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class ShearPin : BaseDetail
    {
        public ShearPin()
        {
            Name = "Штифт";
        }

        public ShearPin(ShearPin shearPin) : base(shearPin)
        {
            Diameter = Microsoft.VisualBasic.Interaction.InputBox("Введите диаметр штифта:");
            TensileStrength = Microsoft.VisualBasic.Interaction.InputBox("Введите предел прочности:");
            DN = shearPin.DN;
            Pull = Microsoft.VisualBasic.Interaction.InputBox("Введите номер тяги:");
            Number = shearPin.Number;
            System.Int32.TryParse(Microsoft.VisualBasic.Interaction.InputBox("Введите количество штифтов в тяге:"), out int tryparse);
            Amount = tryparse;
        }

        [NotMapped]
        public new string FullName => string.Format($"ЗК №{Number}/ф{Diameter}/Тяга: {Pull}/{TensileStrength} МПа/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, ЗК:{Number}, пл.{Melt}, тяга:{Pull}, кол-во:{Amount}");

        public string DN { get; set; }
        public string Pull { get; set; }

        public string Diameter { get; set; }
        public string TensileStrength { get; set; }

        //public int? BaseValveId { get; set; }
        //public BaseValve BaseValve { get; set; }

        public int Amount { get; set; }

        public int? AmountRemaining { get; set; }

        public ObservableCollection<ShearPinJournal> ShearPinJournals { get; set; }
        public ObservableCollection<BaseValveWithShearPin> BaseValveWithShearPins { get; set; }
        public ObservableCollection<ShearPinWithFile> Files { get; set; }
    }
}
