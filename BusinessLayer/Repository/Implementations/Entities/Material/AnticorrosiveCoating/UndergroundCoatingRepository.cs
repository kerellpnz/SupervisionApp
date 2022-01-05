using DataLayer;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using DataLayer.TechnicalControlPlans.Materials.AnticorrosiveCoating;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Repository.Implementations.Entities.Material
{
    public class UndergroundCoatingRepository : RepositoryWithJournal<UndergroundCoating, UndergroundCoatingJournal, AnticorrosiveCoatingTCP>
    {
        public UndergroundCoatingRepository(DataContext context) : base(context) { }

        public async Task<UndergroundCoating> GetByIdIncludeAsync(int id)
        {
            return await db.UndergroundCoatings.Include(i => i.UndergroundCoatingJournals)
                .Include(i => i.BaseValveWithCoatings).ThenInclude(i => i.BaseValve)
                
                .SingleOrDefaultAsync(i => i.Id == id);
        }

        //public override async Task<IList<UndergroundCoating>> GetAllAsync()
        //{
        //    await db.UndergroundCoatings.Include(i => i.UndergroundCoatingJournals).LoadAsync();
        //    return db.UndergroundCoatings.Local.ToObservableCollection();
        //}
    }
}
