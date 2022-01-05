using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Materials;
using DataLayer.Files;
using DataLayer.Journals.Detailing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities.Detailing
{
    public class Gate : BaseDetail
    {

        public string ZK { get; set; }
        public string DN { get; set; }
        public System.DateTime? ProtocolControl { get; set; }
        public string ProtocolControlStatus { get; set; }

        [NotMapped]
        public new string FullName => string.Format($"{Number}/пл.{Melt}/DN{DN} - {Status}");

        [NotMapped]
        public string NameForReport => string.Format($"DN{DN}, {Number}, {ZK}, пл.{Melt}");


        public Gate()
        {
            Name = "Шибер";
        }

        public Gate(Gate gate) : base(gate)
        {
            MetalMaterialId = gate.MetalMaterialId;
            MetalMaterial = gate.MetalMaterial;
            ZK = Microsoft.VisualBasic.Interaction.InputBox("Введите номер ЗК:");
            DN = gate.DN;
            Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер:");
            Melt = gate.Melt;
            Material = gate.Material;
        }

        

        public int? MetalMaterialId { get; set; }
        public MetalMaterial MetalMaterial { get; set; }

        public int? PIDId { get; set; }
        public PID PID { get; set; }
        public BaseValve BaseValve { get; set; }

        public ObservableCollection<GateJournal> GateJournals { get; set; }
        public ObservableCollection<GateWithFile> Files { get; set; }
    }
}
