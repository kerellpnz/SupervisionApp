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
    public class CoverFlangeRepository : RepositoryWithJournal<CoverFlange, CoverFlangeJournal, CoverFlangeTCP>
    {
        public CoverFlangeRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(BaseWeldValveDetail basevalvedetail)
        {
            using (DataContext context = new DataContext())
            {
                var flange = await context.CoverFlanges.Include(i => i.BaseWeldValveDetail).SingleOrDefaultAsync(i => i.Id == basevalvedetail.CoverFlange.Id);
                if (flange?.BaseWeldValveDetail != null && flange.BaseWeldValveDetail.Id != basevalvedetail.Id)
                {
                    MessageBox.Show($"Фланец применен в {flange.BaseWeldValveDetail.Name} № {flange.BaseWeldValveDetail.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        /*public override async Task<IList<CoverFlange>> GetAllAsync()
        {
            await db.CoverFlanges.Include(i => i.MetalMaterial).LoadAsync();
            var result = db.CoverFlanges.Local.ToObservableCollection();
            return result;
        }*/

        public async Task Load()
        {
            await db.CoverFlanges.LoadAsync();
        }

        public IList<CoverFlange> SortList()
        {
            return db.CoverFlanges.Local.OrderBy(i => i.Number).ToList();
        }

        public async Task<CoverFlange> GetByIdIncludeAsync(int id)
        {
            var result = await db.CoverFlanges
                .Include(i => i.BaseWeldValveDetail)
                .Include(i => i.CoverFlangeJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                //.Include(i => i.MetalMaterial)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
