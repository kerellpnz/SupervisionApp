using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class BaseValveCoverWithSeals
    {
        public int Id { get; set; }

        public int BaseWeldValveId { get; set; }
        public BaseWeldValveDetail BaseWeldValve { get; set; }

        public int? AssemblyUnitSealingId { get; set; }
        public AssemblyUnitSealing AssemblyUnitSealing { get; set; }

        [NotMapped]
        public string FullName => string.Format($"{AssemblyUnitSealing?.FullName}");
    }
}
