using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class CoverFlange : BaseDetail
    {

        public string DN { get; set; }

        [NotMapped]
        public new string FullName => string.Format($"№{Number}/пл.{Melt}/черт.{Drawing}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, №{Number}, пл.{Melt}");

        public CoverFlange ()
        {
            Name = "Фланец";
            Material = "09Г2С";
        }
        public CoverFlange(CoverFlange flange) : base(flange)
        {
            //MetalMaterialId = flange.MetalMaterialId;
            DN = flange.DN;
            Number = flange.Number;
        }

        public CoverFlange(CoverFlange flange, string number) : base(flange)
        {
            //MetalMaterialId = flange.MetalMaterialId;
            DN = flange.DN;
            Number = number;
        }

        //public int? MetalMaterialId { get; set; }
        //public MetalMaterial MetalMaterial { get; set; }

        public BaseWeldValveDetail BaseWeldValveDetail { get; set; }

        public ObservableCollection<CoverFlangeJournal> CoverFlangeJournals { get; set; }
        public ObservableCollection<CoverFlangeWithFile> Files { get; set; }
    }
}
