using System.Collections.ObjectModel;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails
{
    public class Ring047TCP : BaseTCP
    {
        public ObservableCollection<Ring047Journal> Ring047Journals { get; set; }
    }
}
