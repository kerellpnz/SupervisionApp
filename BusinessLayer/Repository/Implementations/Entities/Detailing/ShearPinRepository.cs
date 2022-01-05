using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class ShearPinRepository : RepositoryWithJournal<ShearPin, ShearPinJournal, ShearPinTCP>
    {
        public ShearPinRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAmountRemaining(ShearPin shearPin, int amount)
        {
            using (DataContext context = new DataContext())
            {
                var amountRemaining = await GetAmountRemaining(shearPin);
                if (amountRemaining >= amount)
                    return true;
                else
                {
                    MessageBox.Show($"Остаток штифтов {shearPin.Number} составляет {amountRemaining} шт.", "Ошибка");
                    return false;
                }
            }
        }

        public async Task<int> GetAmountRemaining(ShearPin shearPin)
        {
            using (DataContext context = new DataContext())
            {
                var item = await context.ShearPins.Include(i => i.BaseValveWithShearPins).SingleOrDefaultAsync(i => i.Id == shearPin.Id);
                int amountUsed = 0;
                foreach (var i in item.BaseValveWithShearPins)
                {
                    amountUsed += i.ShearPinAmount;
                }
                return item.Amount - amountUsed;
            }
        }



        //public IList<ShearPin> UpdateList()
        //{
        //    return db.ShearPins.Local.Where(i => i.AmountRemaining > 0).ToList();
        //}

        public async Task Load()
        {
            await db.ShearPins.LoadAsync();
        }

        public IList<ShearPin> SortList()
        {
            return db.ShearPins.Local.OrderBy(i => i.Number).ToList();
        }

        public async Task<ShearPin> GetByIdIncludeAsync(int id)
        {
            var result = await db.ShearPins
                .Include(i => i.BaseValveWithShearPins)
                    .ThenInclude(i => i.BaseValve)
                .Include(i => i.ShearPinJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
