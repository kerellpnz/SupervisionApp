using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Materials;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Materials
{
    public class ForgingMaterial : BaseEntity
    {
        public ForgingMaterial()
        {
            Name = "Поковка/Отливка";
        }

        public ForgingMaterial(ForgingMaterial material) : base(material)
        {
            Number = material.Number;
            Name = material.Name;
            Batch = material.Batch;
            Certificate = material.Certificate;
            Comment = material.Comment;
            FirstSize = material.FirstSize;
            Material = material.Material;            
            Melt = material.Melt;            
            SecondSize = material.SecondSize;
            Status = material.Status;
            ThirdSize = material.ThirdSize;
            Target = material.Target;
        }

        public CoverSleeve CoverSleeve { get; set; }
        public Nozzle Nozzle { get; set; }
        public Saddle Saddle { get; set; }
        public Ring043 Ring043 { get; set; }
        public Ring047 Ring047 { get; set; }
        public CounterFlange CounterFlange { get; set; }
        public WeldGateValveCover WeldGateValveCover  { get; set; }
        public WeldGateValveCase WeldGateValveCase { get; set; }

        public ObservableCollection<ForgingMaterialJournal> ForgingMaterialJournals { get; set; }

        
        public string Material { get; set; }
        public string Melt { get; set; }        
        public string Batch { get; set; }      

        public string FirstSize { get; set; }
        public string SecondSize { get; set; }
        public string ThirdSize { get; set; }
        
        public string Target { get; set; }



        //[NotMapped]
        //public new string FullName => string.Format($"{Melt}пл./{Material}/№{Number}/{Target}");

        [NotMapped]
        public new string FullName => string.Format($"Плавка: {Melt}/{Material}| №{Number}| Серт.: №{Certificate}| Чертеж: {Drawing}| Применена в: {CoverSleeve?.ZK}{Nozzle?.ZK}" +
            $"{Saddle?.ZK}{Ring043?.ZK}{WeldGateValveCover?.Number}{WeldGateValveCase?.Number}{CounterFlange?.Number} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"Серт.№{Certificate}, пл.{Melt}, {Material}, №{Number}");
    }
}
