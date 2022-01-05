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
    public class CoverSleeve008Repository : RepositoryWithJournal<CoverSleeve008, CoverSleeve008Journal, CoverSleeve008TCP>
    {
        public CoverSleeve008Repository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(BaseWeldValveDetail cover)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.CoverSleeves008.Include(i => i.BaseWeldValveDetail).SingleOrDefaultAsync(i => i.Id == cover.CoverSleeve008.Id);
                if (detail?.BaseWeldValveDetail != null && detail.BaseWeldValveDetail.Id != cover.Id)
                {
                    MessageBox.Show($"Втулка(008) применена в {detail.BaseWeldValveDetail.Name} № {detail.BaseWeldValveDetail.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        //public override async Task<IList<CoverSleeve008>> GetAllAsync()
        //{
        //    await db.CoverSleeves008.Include(i => i.MetalMaterial).LoadAsync();
        //    var result = db.CoverSleeves008.Local.ToObservableCollection();
        //    return result;
        //}

        public async Task Load()
        {
            await db.CoverSleeves008.LoadAsync();
        }

        public IList<CoverSleeve008> SortList()
        {
            return db.CoverSleeves008.Local.OrderBy(i => i.ZK).ToList();
        }

        public async Task<CoverSleeve008> GetByIdIncludeAsync(int id)
        {
            var result = await db.CoverSleeves008
                .Include(i => i.BaseWeldValveDetail)
                .Include(i => i.CoverSleeve008Journals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .Include(i => i.MetalMaterial)
                
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
