using DataLayer.Entities.Materials;
using DataLayer.TechnicalControlPlans.Materials;

namespace DataLayer.Journals.Materials
{
    public class ForgingMaterialJournal : BaseJournal<ForgingMaterial, ForgingMaterialTCP>
    {
        public ForgingMaterialJournal() { }

        public ForgingMaterialJournal(ForgingMaterial entity, ForgingMaterialTCP entityTCP) : base(entity, entityTCP)
        { }

        public ForgingMaterialJournal(int id, ForgingMaterialJournal journal) : base(id, journal)
        { }
    }
}
