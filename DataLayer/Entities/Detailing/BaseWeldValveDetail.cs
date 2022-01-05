using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using System;
using System.Collections.ObjectModel;

namespace DataLayer.Entities.Detailing
{
    public class BaseWeldValveDetail : BaseDetail
    {
        public string PN { get; set; }

        public DateTime? DateOfWashing { get; set; }

        public int? CaseBottomId { get; set; }
        public CaseBottom CaseBottom { get; set; }

        public int? CoverFlangeId { get; set; }
        public CoverFlange CoverFlange { get; set; }

        public int? CoverSleeve008Id { get; set; }
        public CoverSleeve008 CoverSleeve008 { get; set; }

        public int? ForgingMaterialId { get; set; }
        public ForgingMaterial ForgingMaterial { get; set; }

        public int? MetalMaterialId { get; set; }
        public MetalMaterial MetalMaterial { get; set; }

        public ObservableCollection<Ring043> Rings { get; set; }

        public ObservableCollection<BaseValveCoverWithSeals> BaseValveSCoverWithSeals { get; set; }

        public BaseWeldValveDetail()
        {
        }

        public BaseWeldValveDetail(BaseWeldValveDetail cover) : base(cover)
        {
            //DN = cover.DN;
            //Material = cover.Material;
            //Melt = cover.Melt;
        }
    }
}
