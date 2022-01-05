using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BusinessLayer.Repository.Interfaces.Entities.Detailing;
using DataLayer;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing;
using DataLayer.Journals.Detailing;
using DataLayer.TechnicalControlPlans.Detailing;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class NozzleRepository : RepositoryWithJournal<Nozzle, NozzleJournal, NozzleTCP>, INozzleRepository
    {
        public NozzleRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(Nozzle nozzle, BaseValve valve)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.Nozzles.Include(i => i.BaseValve).SingleOrDefaultAsync(i => i.Id == nozzle.Id);
                if (detail?.BaseValve != null && detail.BaseValve.Id != valve.Id)
                {
                    MessageBox.Show($"Катушка применена в {detail.BaseValve.Name} № {detail.BaseValve.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public new async Task<IEnumerable<NozzleTCP>> GetTCPsAsync()
        {
            await db.NozzleTCPs.Include(i => i.OperationType).LoadAsync();
            return db.NozzleTCPs.Local.ToObservableCollection();
        }

        public async Task Load()
        {
            await db.Nozzles.LoadAsync();
        }

        public IList<Nozzle> UpdateList()
        {
            return db.Nozzles.Local.Where(i => i.BaseValveId == null).OrderBy(i => i.ZK).ToList();
        }

        public override async Task<IList<Nozzle>> GetAllAsync()
        {            
            await db.Nozzles.Include(i => i.PID).LoadAsync();
            return db.Nozzles.Local.ToObservableCollection();
        }

        public async Task<IList<Nozzle>> GetAllAsyncForCompare()
        {
            await db.Nozzles.LoadAsync();
            return db.Nozzles.Local.ToObservableCollection();
        }

        public async Task<Nozzle> GetByIdIncludeAsync(int id)
        {
            var result = await db.Nozzles
                .Include(i => i.BaseValve)                
                .Include(i => i.NozzleJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.OperationType)
                .Include(i => i.MetalMaterial)
                .Include(i => i.ForgingMaterial)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
