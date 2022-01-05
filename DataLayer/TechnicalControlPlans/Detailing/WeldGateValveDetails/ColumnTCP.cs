using DataLayer.Journals.Detailing.WeldGateValveDetails;
using System.Collections.ObjectModel;

namespace DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails
{
    public class ColumnTCP : BaseTCP
    {
        public ObservableCollection<ColumnJournal> ColumnJournals { get; set; }
    }
}
