using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;

namespace DataLayer.Journals.Detailing.WeldGateValveDetails
{
    public class CoverSleeve008Journal : BaseJournal<CoverSleeve008, CoverSleeve008TCP>
    {
        public CoverSleeve008Journal() { }

        public CoverSleeve008Journal(CoverSleeve008 entity, CoverSleeve008TCP entityTCP) : base(entity, entityTCP)
        { }

        public CoverSleeve008Journal(int id, CoverSleeve008Journal journal) : base(id, journal)
        { }
    }
}
