using DataLayer;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using DataLayer.TechnicalControlPlans.Materials.AnticorrosiveCoating;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Repository.Implementations.Entities.Material
{
    public class AbovegroundCoatingRepository : RepositoryWithJournal<AbovegroundCoating, AbovegroundCoatingJournal, AnticorrosiveCoatingTCP>
    {
        public AbovegroundCoatingRepository(DataContext context) : base(context) { }

        public async Task<AbovegroundCoating> GetByIdIncludeAsync(int id)
        {
            return await db.AbovegroundCoatings.Include(i => i.AbovegroundCoatingJournals)
                .Include(i => i.BaseValveWithCoatings).ThenInclude(i => i.BaseValve)
                
                .SingleOrDefaultAsync(i => i.Id == id);
        }

        //public override async Task<IList<AbovegroundCoating>> GetAllAsync()
        //{
        //    await db.AbovegroundCoatings.Include(i => i.AbovegroundCoatingJournals).LoadAsync();
        //    return db.AbovegroundCoatings.Local.ToObservableCollection();
        //}
    }
}
