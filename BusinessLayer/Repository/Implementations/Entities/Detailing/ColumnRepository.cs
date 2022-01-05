using System.Collections.Generic;
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
    public class ColumnRepository : RepositoryWithJournal<Column, ColumnJournal, ColumnTCP>
    {
        public ColumnRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(BaseValveCover cover)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.Columns.Include(i => i.BaseValveCover).SingleOrDefaultAsync(i => i.Id == cover.Column.Id);
                if (detail?.BaseValveCover != null && detail.BaseValveCover.Id != cover.Id)
                {
                    MessageBox.Show($"Стойка применена в {detail.BaseValveCover.Name} № {detail.BaseValveCover.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public override async Task<IList<Column>> GetAllAsync()
        {            
            await db.Columns.LoadAsync();            
            return db.Columns.Local.ToObservableCollection(); 
        }

        public async Task<Column> GetByIdIncludeAsync(int id)
        {
            var result = await db.Columns
                .Include(i => i.BaseValveCover)
                .Include(i => i.RunningSleeve)
                .Include(i => i.ColumnJournals) 
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
