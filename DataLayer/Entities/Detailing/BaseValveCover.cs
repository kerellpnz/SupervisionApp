using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using System.Collections.ObjectModel;

namespace DataLayer.Entities.Detailing
{
    public class BaseValveCover : BaseWeldValveDetail
    {
        public int? SpindleId { get; set; }
        public int? ColumnId { get; set; }
        public Spindle Spindle { get; set; }
        public Column Column { get; set; }

        public string DN { get; set; }

        public int? PIDId { get; set; }
        public PID PID { get; set; }


        //public int? MetalMaterialId { get; set; }
        //public MetalMaterial MetalMaterial { get; set; }



        public ObservableCollection<BaseValveCoverWithFile> Files { get; set; }

        public BaseValveCover()
        {
        }

        public BaseValveCover(BaseValveCover cover) : base(cover)
        {
            DN = cover.DN;
            //MetalMaterialId = cover.MetalMaterialId;
            //Material = cover.Material;
            
        }
    }
}
