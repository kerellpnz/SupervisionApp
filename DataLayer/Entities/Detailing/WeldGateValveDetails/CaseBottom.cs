using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.Entities.Detailing.WeldGateValveDetails
{
    public class CaseBottom : BaseDetail
    {

        public string DN { get; set; }
        public string ZK { get; set; }



        [NotMapped]
        public new string FullName => string.Format($"№{Number}/пл.{Melt}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, №{Number}, пл.{Melt}");


        public CaseBottom()
        {
            Name = "Днище";
        }
        public CaseBottom(CaseBottom bottom) : base(bottom)
        {
            MetalMaterialId = bottom.MetalMaterialId;
            Number = bottom.Number;
            DN = bottom.DN;
            ZK = bottom.ZK;
        }

        public CaseBottom(CaseBottom bottom, string number) : base(bottom)
        {
            MetalMaterialId = bottom.MetalMaterialId;
            Number = number;
            DN = bottom.DN;
            ZK = bottom.ZK;
        }

        public int? MetalMaterialId { get; set; }
        public MetalMaterial MetalMaterial { get; set; }

        /*public WeldGateValveCase WeldGateValveCase { get; set; }

        public WeldGateValveCover WeldGateValveCover { get; set; }*/

        public BaseWeldValveDetail BaseWeldValveDetail { get; set; }

        public ObservableCollection<CaseBottomJournal> CaseBottomJournals { get; set; }
        public ObservableCollection<CaseBottomWithFile> Files { get; set; }
    }
}
