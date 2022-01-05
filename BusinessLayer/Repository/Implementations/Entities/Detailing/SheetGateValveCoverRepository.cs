using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataLayer;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Entities.Detailing.SheetGateValveDetails;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Journals.Detailing.WeldGateValveDetails;
using DataLayer.TechnicalControlPlans.Detailing.WeldGateValveDetails;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class SheetGateValveCoverRepository : RepositoryWithJournal<SheetGateValveCover, SheetGateValveCoverJournal, SheetGateValveCoverTCP>
    {
        public SheetGateValveCoverRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(SheetGateValve valve)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.SheetGateValveCovers.Include(i => i.BaseWeldValve).SingleOrDefaultAsync(i => i.Id == valve.WeldGateValveCover.Id);
                if (detail?.BaseWeldValve != null && detail.BaseWeldValve.Id != valve.Id)
                {
                    MessageBox.Show($"Крышка применена в {detail.BaseWeldValve.Name} № {detail.BaseWeldValve.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        //public async Task<IList<SheetGateValveCover>> GetAllIncludeAsync()
        //{
        //    await db.SheetGateValveCovers.Include(i => i.Spindle).ThenInclude(i => i.MetalMaterial)
        //        .Include(i => i.CoverFlange)
        //        .Include(i => i.CaseBottom)
        //            .ThenInclude(i => i.MetalMaterial)
        //        .Include(i => i.Column)
        //            .ThenInclude(i => i.RunningSleeve)
        //        .LoadAsync();
        //    var result = db.SheetGateValveCovers.Local.ToObservableCollection();
        //    return result;
        //}

        public new async Task<IEnumerable<SheetGateValveCoverTCP>> GetTCPsAsync()
        {
            await db.SheetGateValveCoverTCPs.Include(i => i.OperationType).LoadAsync();
            return db.SheetGateValveCoverTCPs.Local.ToObservableCollection();
        }

        public async Task Load()
        {
            await db.SheetGateValveCovers.Include(i => i.Spindle)
                //.Include(i => i.CoverFlange)
                //.Include(i => i.CoverSleeve)
                //.Include(i => i.CoverSleeve008)
                //.Include(i => i.BaseValveSCoverWithSeals)
                    //.ThenInclude(i => i.AssemblyUnitSealing)
                //.Include(i => i.CaseBottom)
                .Include(i => i.Column)
                    .ThenInclude(i => i.RunningSleeve)
                .LoadAsync();
        }

        public IList<SheetGateValveCover> SortList()
        {
            return db.SheetGateValveCovers.Local.OrderBy(i => i.Number).ToList();
        }

        public override async Task<IList<SheetGateValveCover>> GetAllAsync()
        {                        
            await db.SheetGateValveCovers.Include(i => i.PID).LoadAsync(); 
            return db.SheetGateValveCovers.Local.ToObservableCollection();
        }

        public async Task<IList<SheetGateValveCover>> GetAllAsyncForCompare()
        {
            await db.SheetGateValveCovers.LoadAsync(); 
            return db.SheetGateValveCovers.Local.ToObservableCollection();
        }

        public async Task<SheetGateValveCover> GetByIdIncludeAsync(int id)
        {
            var result = await db.SheetGateValveCovers
                .Include(i => i.BaseWeldValve)
                .Include(i => i.BaseValveSCoverWithSeals)
                    .ThenInclude(i => i.AssemblyUnitSealing)
                .Include(i => i.Column)
                .Include(i => i.CoverSleeve)
                .Include(i => i.CoverSleeve008)
                .Include(i => i.ForgingMaterial)
                .Include(i => i.CaseBottom)
                .Include(i => i.CoverFlange)
                .Include(i => i.Spindle)                
                .Include(i => i.SheetGateValveCoverJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.OperationType)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
