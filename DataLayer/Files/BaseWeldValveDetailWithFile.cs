using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing;

namespace DataLayer.Files
{
    public class BaseWeldValveDetailWithFile : BaseTable
    {
        public int BaseWeldValveDetailId { get; set; }
        public BaseWeldValveDetail BaseWeldValveDetail { get; set; }

        public int ElectronicDocumentId { get; set; }
        public ElectronicDocument ElectronicDocument { get; set; }

        public BaseWeldValveDetailWithFile()
        {
        }

        public BaseWeldValveDetailWithFile(int id, ElectronicDocument file)
        {
            BaseWeldValveDetailId = id;
            ElectronicDocumentId = file.Id;
        }
    }
}
