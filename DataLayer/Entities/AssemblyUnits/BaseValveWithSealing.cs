using DataLayer.Entities.Detailing;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.AssemblyUnits
{
    public class BaseValveWithSealing
    {
        public int Id { get; set; }

        public int BaseValveId { get; set; }
        public BaseValve BaseValve { get; set; }

        //public int? AssemblyUnitSealingId { get; set; }
        //public AssemblyUnitSealing AssemblyUnitSealing { get; set; }

        public int? MainFlangeSealingId { get; set; }

        public MainFlangeSealing MainFlangeSealing { get; set; }

        [NotMapped]
        public string FullName => string.Format($"{MainFlangeSealing?.FullName}");
    }
}
