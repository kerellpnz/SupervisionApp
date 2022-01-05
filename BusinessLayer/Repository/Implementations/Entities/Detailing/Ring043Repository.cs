using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class Ring043Repository : RepositoryWithJournal<Ring043, Ring043Journal, Ring043TCP>
    {
        public Ring043Repository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(Ring043 ring, BaseWeldValveDetail weldCase)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.Rings043.Include(i => i.BaseWeldValve).SingleOrDefaultAsync(i => i.Id == ring.Id);
                if (detail?.BaseWeldValve != null && detail.BaseWeldValve.Id != weldCase.Id)
                {
                    MessageBox.Show($"Кольцо применено в {detail.BaseWeldValve.Name} № {detail.BaseWeldValve.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        //public override async Task<IList<Ring043>> GetAllAsync()
        //{
        //    await db.Rings043.Include(i => i.MetalMaterial).LoadAsync();
        //    var result = db.Rings043.Local.ToObservableCollection();
        //    return result;
        //}

        public async Task Load()
        {
            await db.Rings043.LoadAsync();
        }

        public IList<Ring043> UpdateList()
        {
            return db.Rings043.Local.Where(i => i.BaseWeldValveId == null).OrderBy(i => i.ZK).ToList();
        }

        public async Task<Ring043> GetByIdIncludeAsync(int id)
        {
            var result = await db.Rings043
                .Include(i => i.BaseWeldValve)
                .Include(i => i.Ring043Journals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .Include(i => i.MetalMaterial)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}

