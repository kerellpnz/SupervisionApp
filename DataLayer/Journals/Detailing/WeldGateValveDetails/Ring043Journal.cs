using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;

namespace DataLayer.Journals.Detailing.WeldGateValveDetails
{
    
        public class Ring043Journal : BaseJournal<Ring043, Ring043TCP>
        {
            public Ring043Journal() { }

            public Ring043Journal(Ring043 entity, Ring043TCP entityTCP) : base(entity, entityTCP)
            { }

            public Ring043Journal(int id, Ring043Journal journal) : base(id, journal)
            { }
        }
   
}
