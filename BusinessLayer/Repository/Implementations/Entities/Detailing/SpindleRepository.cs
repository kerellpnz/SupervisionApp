using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class SpindleRepository : RepositoryWithJournal<Spindle, SpindleJournal, SpindleTCP>
    {
        public SpindleRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(BaseValveCover cover)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.Spindles.Include(i => i.BaseValveCover).SingleOrDefaultAsync(i => i.Id == cover.Spindle.Id);
                if (detail?.BaseValveCover != null && detail.BaseValveCover.Id != cover.Id)
                {
                    MessageBox.Show($"Шпиндель применен в {detail.BaseValveCover.Name} № {detail.BaseValveCover.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public override async Task<IList<Spindle>> GetAllAsync()
        {
            //await db.Spindles.Include(i => i.MetalMaterial).LoadAsync();
            await db.Spindles.Include(i => i.PID).LoadAsync();
            var result = db.Spindles.Local.ToObservableCollection();
            return result;
        }

        public async Task<IList<Spindle>> GetAllAsyncForCompare()
        {            
            await db.Spindles.LoadAsync();            
            return db.Spindles.Local.ToObservableCollection(); 
        }

        public async Task Load()
        {
            await db.Spindles.LoadAsync();
        }

        public IList<Spindle> SortList()
        {
            return db.Spindles.Local.OrderBy(i => i.ZK).ToList();
        }

        public async Task<Spindle> GetByIdIncludeAsync(int id)
        {
            var result = await db.Spindles
                .Include(i => i.BaseValveCover)
                .Include(i => i.SpindleJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .Include(i => i.MetalMaterial)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
