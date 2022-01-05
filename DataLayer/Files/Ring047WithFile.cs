using DataLayer.Entities.Detailing.WeldGateValveDetails;

namespace DataLayer.Files
{
    public class Ring047WithFile : BaseTable
    {
        public int Ring047Id { get; set; }
        public Ring047 Ring047 { get; set; }

        public int ElectronicDocumentId { get; set; }
        public ElectronicDocument ElectronicDocument { get; set; }

        public Ring047WithFile()
        {
        }

        public Ring047WithFile(int id, ElectronicDocument file)
        {
            Ring047Id = id;
            ElectronicDocumentId = file.Id;
        }
    }
}
