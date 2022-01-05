using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BusinessLayer.Repository.Interfaces.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class RunningSleeveRepository : RepositoryWithJournal<RunningSleeve, RunningSleeveJournal, RunningSleeveTCP>
    {
        public RunningSleeveRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(Column column)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.RunningSleeves.Include(i => i.Column).SingleOrDefaultAsync(i => i.Id == column.RunningSleeve.Id);
                if (detail?.Column != null && detail.Column.Id != column.Id)
                {
                    MessageBox.Show($"Втулка ходовая применена в {detail.Column.Name} № {detail.Column.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task Load()
        {
            await db.RunningSleeves.LoadAsync();
        }

        public IList<RunningSleeve> SortList()
        {
            return db.RunningSleeves.Local.OrderBy(i => i.ZK).ToList();
        }

        public async Task<RunningSleeve> GetByIdIncludeAsync(int id)
        {
            var result = await db.RunningSleeves
                .Include(i => i.Column)
                .Include(i => i.RunningSleeveJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
