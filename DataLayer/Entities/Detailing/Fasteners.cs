namespace DataLayer.Entities.Detailing
{
    public class Fasteners : BaseDetail
    {
        public string Thread { get; set; }

        public string DN { get; set; }

        public string Hardness { get; set; }

        public string Batch { get; set; }

        public int Amount { get; set; }

        public int? AmountRemaining { get; set; }

        public Fasteners()
        {

        }

        public Fasteners(Fasteners fastener) 
        {
            DN = fastener.DN;
            Material = fastener.Material;
            Melt = fastener.Melt;
            Name = fastener.Name;            
            Drawing = fastener.Drawing;
            Certificate = fastener.Certificate;
            Status = fastener.Status;
            Comment = fastener.Comment;
            Number = Microsoft.VisualBasic.Interaction.InputBox("Введите номер партии:");
        }
    }
}
