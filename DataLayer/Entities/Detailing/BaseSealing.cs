using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class BaseSealing : BaseTable
    {
        public BaseSealing() { }
        public BaseSealing(BaseSealing sealing) : base(sealing)
        {
            Material = sealing.Material;
            Batch = sealing.Batch;
            Series = sealing.Series;           
            Name = sealing.Name;            
            Drawing = sealing.Drawing;
            Certificate = sealing.Certificate;
            Status = sealing.Status;
            Comment = sealing.Comment;
        }

        public string  Number { get; set; }

        public string Name { get; set; }        

        public string Drawing { get; set; }

        public string Certificate { get; set; }

        public string Status { get; set; }

        public string Comment { get; set; }

        public string Material { get; set; }

        public string Batch { get; set; }       
        
        public string Series { get; set; }

        public float Amount { get; set; }

        public float? AmountRemaining { get; set; }

        [NotMapped]
        public string FullName => string.Format($"Сертификат: №{Certificate}| Партия: {Batch}| {Name}| Чертеж: {Drawing} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"пар.:{Batch}, серт: №{Certificate}, Кол-во: {Amount} шт.");
    }
}
