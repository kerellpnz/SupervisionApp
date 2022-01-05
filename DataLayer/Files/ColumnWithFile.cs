using DataLayer.Entities.Detailing.WeldGateValveDetails;

namespace DataLayer.Files
{
    public class ColumnWithFile : BaseTable
    {
        public int ColumnId { get; set; }
        public Column Column { get; set; }

        public int ElectronicDocumentId { get; set; }
        public ElectronicDocument ElectronicDocument { get; set; }

        public ColumnWithFile()
        {
        }

        public ColumnWithFile(int id, ElectronicDocument file)
        {
            ColumnId = id;
            ElectronicDocumentId = file.Id;
        }
    }
}
