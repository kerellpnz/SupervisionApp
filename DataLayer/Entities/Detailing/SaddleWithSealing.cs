//using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class SaddleWithSealing : BaseTable
    {
        public int SaddleId { get; set; }
        public Saddle Saddle { get; set; }

        public int FrontalSaddleSealingId { get; set; }
        public FrontalSaddleSealing FrontalSaddleSealing { get; set; }

       /* [NotMapped]
        public new string FullName => string.Format($"{Saddle.FullName}");

        [NotMapped]
        public string NameForReport => string.Format($"{Saddle.FullName}");*/
    }
}
