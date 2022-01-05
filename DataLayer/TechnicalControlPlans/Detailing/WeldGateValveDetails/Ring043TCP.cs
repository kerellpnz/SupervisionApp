using System.Collections.ObjectModel;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails
{
    public class Ring043TCP : BaseTCP
    {
        public ObservableCollection<Ring043Journal> Ring043Journals { get; set; }
    }
}
