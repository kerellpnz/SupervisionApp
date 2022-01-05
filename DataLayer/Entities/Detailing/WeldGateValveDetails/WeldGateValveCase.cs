using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Files;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class WeldGateValveCase : BaseWeldValveDetail
    {   
        public string Diameter { get; set; }
        public string Thickness { get; set; }        

        //public int? Ring043Id { get; set; }
        //public Ring043 Ring043 { get; set; }

        //public int? Ring047Id { get; set; }
        //public Ring047 Ring047 { get; set; }


        public int? PIDId { get; set; }
        public PID PID { get; set; }

        public string DN { get; set; }


        [NotMapped]
        public new string FullName => string.Format($"№{Number}/пл.{Melt}/DN{DN} - {Status}");
        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, №{Number}, пл.{Melt}");

        
        public ObservableCollection<WeldGateValveCaseWithFile> Files { get; set; }

        public BaseWeldValve BaseWeldValve { get; set; }

        public WeldGateValveCase()
        {
        }
        public WeldGateValveCase(WeldGateValveCase weldCase) : base(weldCase)
        {
            DN = weldCase.DN;
            //Material = weldCase.Material;
            //Melt = weldCase.Melt;
        }
    }
}
