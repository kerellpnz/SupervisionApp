using DataLayer.Journals.Detailing;
using System.Collections.ObjectModel;

namespace DataLayer.TechnicalControlPlans.Detailing
{
    public class MainFlangeSealingTCP : BaseTCP
    {
        public ObservableCollection<MainFlangeSealingJournal> MainFlangeSealingJournals { get; set; }
    }
}
