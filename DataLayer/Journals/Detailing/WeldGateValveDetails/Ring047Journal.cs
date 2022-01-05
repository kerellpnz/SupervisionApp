using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;

namespace DataLayer.Journals.Detailing.WeldGateValveDetails
{
    
        public class Ring047Journal : BaseJournal<Ring047, Ring047TCP>
        {
            public Ring047Journal() { }

            public Ring047Journal(Ring047 entity, Ring047TCP entityTCP) : base(entity, entityTCP)
            { }

            public Ring047Journal(int id, Ring047Journal journal) : base(id, journal)
            { }
        }
    
}
