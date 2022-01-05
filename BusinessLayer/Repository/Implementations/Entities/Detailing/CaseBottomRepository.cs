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
    public class CaseBottomRepository : RepositoryWithJournal<CaseBottom, CaseBottomJournal, CaseBottomTCP>
    {
        public CaseBottomRepository(DataContext context) : base(context)
        {
        }


        public async Task<bool> IsAssembliedAsync(BaseWeldValveDetail weldCase)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.CaseBottoms.Include(i => i.BaseWeldValveDetail).SingleOrDefaultAsync(i => i.Id == weldCase.CaseBottom.Id);
                if (detail?.BaseWeldValveDetail != null && detail.BaseWeldValveDetail.Id != weldCase.Id)
                {
                    MessageBox.Show($"Днище применено в {detail.BaseWeldValveDetail.Name} № {detail.BaseWeldValveDetail.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }



        /*public async Task<bool> IsAssembliedAsync(WeldGateValveCase weldCase)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.CaseBottoms.Include(i => i.WeldGateValveCase).SingleOrDefaultAsync(i => i.Id == weldCase.CaseBottom.Id);
                if (detail?.WeldGateValveCase != null && detail.WeldGateValveCase.Id != weldCase.Id)
                {
                    MessageBox.Show($"Днище применено в {detail.WeldGateValveCase.Name} № {detail.WeldGateValveCase.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }*/

        /*public async Task<bool> IsAssembliedAsync(WeldGateValveCover weldCover)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.CaseBottoms.Include(i => i.WeldGateValveCover).SingleOrDefaultAsync(i => i.Id == weldCover.CaseBottom.Id);
                if (detail?.WeldGateValveCover != null && detail.WeldGateValveCover.Id != weldCover.Id)
                {
                    MessageBox.Show($"Днище применено в {detail.WeldGateValveCover.Name} № {detail.WeldGateValveCover.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }*/



        //public override async Task<IList<CaseBottom>> GetAllAsync()
        //{
        //    await db.CaseBottoms.Include(i => i.MetalMaterial).LoadAsync();
        //    var result = db.CaseBottoms.Local.ToObservableCollection();
        //    return result;
        //}

        public async Task Load()
        {
            await db.CaseBottoms.LoadAsync();
        }

        public IList<CaseBottom> SortList()
        {
            return db.CaseBottoms.Local.OrderBy(i => i.Number).ToList();
        }


        public async Task<CaseBottom> GetByIdIncludeAsync(int id)
        {
            var result = await db.CaseBottoms
                .Include(i => i.BaseWeldValveDetail)
                //.Include(i => i.WeldGateValveCover)
                .Include(i => i.CaseBottomJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .Include(i => i.MetalMaterial)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
