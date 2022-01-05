using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;

namespace DataLayer.Files
{
    public class CoverSleeve008WithFile : BaseTable
    {
        public int CoverSleeve008Id { get; set; }
        public CoverSleeve008 CoverSleeve008 { get; set; }

        public int ElectronicDocumentId { get; set; }
        public ElectronicDocument ElectronicDocument { get; set; }

        public CoverSleeve008WithFile()
        {
        }

        public CoverSleeve008WithFile(int id, ElectronicDocument file)
        {
            CoverSleeve008Id = id;
            ElectronicDocumentId = file.Id;
        }
    }
}
