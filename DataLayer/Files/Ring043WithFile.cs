using DataLayer.Entities.Detailing.WeldGateValveDetails;

namespace DataLayer.Files
{
    public class Ring043WithFile : BaseTable
    {
        public int Ring043Id { get; set; }
        public Ring043 Ring043 { get; set; }

        public int ElectronicDocumentId { get; set; }
        public ElectronicDocument ElectronicDocument { get; set; }

        public Ring043WithFile()
        {
        }

        public Ring043WithFile(int id, ElectronicDocument file)
        {
            Ring043Id = id;
            ElectronicDocumentId = file.Id;
        }
    }
}
