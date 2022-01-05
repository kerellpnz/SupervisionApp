using DataLayer.Entities.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;

namespace DataLayer.Journals.Detailing
{
    public class MainFlangeSealingJournal : BaseJournal<MainFlangeSealing, MainFlangeSealingTCP>
    {
        public MainFlangeSealingJournal() { }

        public MainFlangeSealingJournal(MainFlangeSealing entity, MainFlangeSealingTCP entityTCP) : base(entity, entityTCP)
        { }

        public MainFlangeSealingJournal(int id, MainFlangeSealingJournal journal) : base(id, journal)
        { }
    }
}
