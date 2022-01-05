using DataLayer.Journals.Materials;
using System.Collections.ObjectModel;


namespace DataLayer.TechnicalControlPlans.Materials
{
    public class ForgingMaterialTCP : BaseTCP
    {
        public ObservableCollection<ForgingMaterialJournal> ForgingMaterialJournals { get; set; }
    }
}
