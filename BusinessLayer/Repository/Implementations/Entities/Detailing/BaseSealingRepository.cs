//using BusinessLayer.Repository.Interfaces.Entities.Detailing;
//using DataLayer;
//using DataLayer.Entities.Detailing;
//using DataLayer.Journals.Detailing;
//using DataLayer.TechnicalControlPlans.Detailing;
//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows;

//namespace BusinessLayer.Repository.Implementations.Entities.Detailing
//{
//    public class BaseSealingRepository : RepositoryWithJournal<BaseSealing, BaseSealingJournal, BaseSealingTCP>
//    {
//        public BaseSealingRepository(DataContext context) : base(context)
//        {
//        }

//        /// <summary>
//        /// Возвращает true, если остаток больше "0".
//        /// </summary>
//        /// <param name="sealing"></param>
//        /// <returns></returns>
//        public async Task<bool> IsAmountRemaining(BaseSealing sealing)
//        {
//            using (DataContext context = new DataContext())
//            {
//                var item = await context.BaseSeals.Include(i => i.BaseValveWithSeals).SingleOrDefaultAsync(i => i.Id == sealing.Id);
//                if (item.Amount > item.BaseValveWithSeals.Count)
//                    return true;
//                else
//                {
//                    MessageBox.Show("Уплотнения данной партии закончились", "Ошибка");
//                    return false;
//                }
//            }
//        }

//        public IList<BaseSealing> UpdateList()
//        {
//            return db.BaseSeals.Local.Where(i => i.AmountRemaining > 0).ToList();
//        }

//        public async Task<BaseSealing> GetByIdIncludeAsync(int id)
//        {
//            var result = await db.BaseSeals
//                .Include(i => i.BaseValveWithSeals)
//                    .ThenInclude(i => i.BaseValve)
//                .Include(i => i.BaseSealingJournals)
//                    .ThenInclude(i => i.EntityTCP)
//                    .ThenInclude(i => i.ProductType)
//                .SingleOrDefaultAsync(i => i.Id == id);
//            return result;
//        }
//    }
//}
