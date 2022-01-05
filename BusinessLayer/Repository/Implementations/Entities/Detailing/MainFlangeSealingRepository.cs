using BusinessLayer.Repository.Interfaces.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class MainFlangeSealingRepository : RepositoryWithJournal<MainFlangeSealing, MainFlangeSealingJournal, MainFlangeSealingTCP>
    {
        public MainFlangeSealingRepository(DataContext context) : base(context)
        {
        }

        /// <summary>
        /// Возвращает true, если остаток больше "0".
        /// </summary>
        /// <param name="sealing"></param>
        /// <returns></returns>
        public async Task<bool> IsAmountRemaining(MainFlangeSealing sealing)
        {
            using (DataContext context = new DataContext())
            {
                var item = await context.MainFlangeSeals.Include(i => i.BaseValveWithSeals).SingleOrDefaultAsync(i => i.Id == sealing.Id);
                if (item.Amount > item.BaseValveWithSeals.Count)
                    return true;
                else
                {
                    MessageBox.Show("Уплотнения данной партии закончились", "Ошибка");
                    return false;
                }
            }
        }

        //public IList<MainFlangeSealing> UpdateList()
        //{
        //    return db.MainFlangeSeals.Local.Where(i => i.AmountRemaining > 0).ToList();
        //}

        public async Task Load()
        {
            await db.MainFlangeSeals.LoadAsync();
        }

        public IList<MainFlangeSealing> SortList()
        {
            return db.MainFlangeSeals.Local.OrderBy(i => i.Batch).ToList();
        }

        public async Task<MainFlangeSealing> GetByIdIncludeAsync(int id)
        {
            var result = await db.MainFlangeSeals
                .Include(i => i.BaseValveWithSeals)
                    .ThenInclude(i => i.BaseValve)
                .Include(i => i.MainFlangeSealingJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
