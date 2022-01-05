using System.Collections.ObjectModel;
using DataLayer.Journals.Detailing.WeldGateValveDetails;

namespace DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails
{
    public class CoverSleeve008TCP : BaseTCP
    {
        public ObservableCollection<CoverSleeve008Journal> CoverSleeve008Journals { get; set; }
    }
}
