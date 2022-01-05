using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataLayer;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class Ring047Repository : RepositoryWithJournal<Ring047, Ring047Journal, Ring047TCP>
    {
        public Ring047Repository(DataContext context) : base(context)
        {
        }

        //public async Task<bool> IsAssembliedAsync(WeldGateValveCase weldCase)
        //{
        //    using (DataContext context = new DataContext())
        //    {
        //        var detail = await context.Rings047.Include(i => i.WeldGateValveCase).SingleOrDefaultAsync(i => i.Id == weldCase.Ring047.Id);
        //        if (detail?.WeldGateValveCase != null && detail.WeldGateValveCase.Id != weldCase.Id)
        //        {
        //            MessageBox.Show($"Кольцо применено в {detail.WeldGateValveCase.Name} № {detail.WeldGateValveCase.Number}", "Ошибка");
        //            return true;
        //        }
        //        else return false;
        //    }
        //}

        //public override async Task<IList<Ring047>> GetAllAsync()
        //{
        //    await db.Rings047.Include(i => i.MetalMaterial).LoadAsync();
        //    var result = db.Rings047.Local.ToObservableCollection();
        //    return result;
        //}

        public async Task Load()
        {
            await db.Rings047.LoadAsync();
        }

        public IList<Ring047> SortList()
        {
            return db.Rings047.Local.OrderBy(i => i.ZK).ToList();
        }

        public async Task<Ring047> GetByIdIncludeAsync(int id)
        {
            var result = await db.Rings047
                //.Include(i => i.WeldGateValveCase)
                .Include(i => i.Ring047Journals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .Include(i => i.MetalMaterial)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
