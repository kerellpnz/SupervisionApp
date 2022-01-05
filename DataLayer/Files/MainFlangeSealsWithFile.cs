using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing;

namespace DataLayer.Files
{
    public class MainFlangeSealsWithFile : BaseTable
    {
        public int MainFlangeSealingId { get; set; }
        public MainFlangeSealing MainFlangeSealing { get; set; }

        public int ElectronicDocumentId { get; set; }
        public ElectronicDocument ElectronicDocument { get; set; }

        public MainFlangeSealsWithFile()
        {
        }

        public MainFlangeSealsWithFile(int id, ElectronicDocument file)
        {
            MainFlangeSealingId = id;
            ElectronicDocumentId = file.Id;
        }
    }
}
