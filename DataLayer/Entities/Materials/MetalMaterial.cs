using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Files;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Materials
{
    public class MetalMaterial : BaseTable
    {
        public MetalMaterial() {  }

        public MetalMaterial(MetalMaterial material)
        {
            Name = material.Name;
            Batch = material.Batch;
            Certificate = material.Certificate;
            Comment = material.Comment;
            FirstSize = material.FirstSize;
            Material = material.Material;
            MaterialCertificateNumber = material.MaterialCertificateNumber;
            Melt = material.Melt;
            //Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер:");
            SecondSize = material.SecondSize;
            Status = material.Status;
            ThirdSize = material.ThirdSize;
        }

        public string Name { get; set; }
        public string Number { get; set; }
        public string Material { get; set; }
        public string Melt { get; set; }
        public string Certificate { get; set; }
        public string Batch { get; set; }
        public string MaterialCertificateNumber { get; set; }
        public string Status { get; set; }
        public string FirstSize { get; set; }
        public string SecondSize { get; set; }
        public string ThirdSize { get; set; }
        public string Comment { get; set; }

        //public CoverSleeve CoverSleeve { get; set; }

        


        [NotMapped] 
        public string FullName => string.Format($"{Melt}пл./{Material}| Лист №{Number}| Сертификат: №{Certificate} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"Серт.№{Certificate}, пл.{Melt}, №{Number}, {Material}");

        //public ObservableCollection<BaseValveCover> BaseValveCover { get; set; }
        public ObservableCollection<Ring043> Rings043 { get; set; }
        public ObservableCollection<Ring047> Rings047 { get; set; }
        public ObservableCollection<Spindle> Spindles { get; set; }
        public ObservableCollection<Saddle> Saddles { get; set; }
        public ObservableCollection<Nozzle> Nozzles { get; set; }
        
        public ObservableCollection<Gate> Gates { get; set; }
        
        
        public ObservableCollection<CoverSleeve> CoverSleeves { get; set; }
        public ObservableCollection<CoverSleeve008> CoverSleeves008 { get; set; }
        public ObservableCollection<CoverFlange> CoverFlanges { get; set; }        
        public ObservableCollection<CaseBottom> CaseBottoms { get; set; }
        
        public ObservableCollection<CounterFlange> CounterFlanges { get; set; }
        

        public ObservableCollection<MetalMaterialWithFile> Files { get; set; }
    }
}
