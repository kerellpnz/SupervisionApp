using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;

namespace DataLayer.Journals.Detailing.WeldGateValveDetails
{
    
    
        public class ColumnJournal : BaseJournal<Column, ColumnTCP>
        {
            public ColumnJournal() { }

            public ColumnJournal(Column entity, ColumnTCP entityTCP) : base(entity, entityTCP)
            { }

            public ColumnJournal(int id, ColumnJournal journal) : base(id, journal)
            { }
        }
    
}
