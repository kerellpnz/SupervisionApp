using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer;
using DataLayer.Entities.AssemblyUnits;
using DataLayer.Journals.AssemblyUnits;
using DataLayer.TechnicalControlPlans.AssemblyUnits;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository.Implementations.Entities.Detailing
{
    public class SheetGateValveRepository : RepositoryWithJournal<SheetGateValve, SheetGateValveJournal, SheetGateValveTCP>
    {
        public SheetGateValveRepository(DataContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CoatingTCP>> GetCoatingTCPsAsync()
        {
            await db.CoatingTCPs.LoadAsync();
            return db.CoatingTCPs.Local.ToObservableCollection();
        }

        public new async Task<IEnumerable<SheetGateValveTCP>> GetTCPsAsync()
        {
            await db.SheetGateValveTCPs.Include(i => i.OperationType).LoadAsync();
            return db.SheetGateValveTCPs.Local.ToObservableCollection();
        }

        public override async Task<IList<SheetGateValve>> GetAllAsync()
        {            
            await db.SheetGateValves.Include(i => i.PID).LoadAsync();
            return db.SheetGateValves.Local.ToObservableCollection();
        }

        //public async Task LoadAsync()
        //{
        //    await db.SheetGateValves.Include(i => i.PID).LoadAsync();            
        //}

        //public IList<SheetGateValve> GetObservableCollection()
        //{
        //    return db.SheetGateValves.Local.ToObservableCollection();
        //}

        public async Task<IList<SheetGateValve>> GetAllAsyncForCompare()
        {
            await db.SheetGateValves.LoadAsync();
            return db.SheetGateValves.Local.ToObservableCollection();
        }


        public async Task<SheetGateValve> GetByIdIncludeAsyncForBlank(int id)
        {
            var result = await db.SheetGateValves
                .Include(i => i.WeldGateValveCase)
                    .ThenInclude(i => i.CoverFlange)
                .Include(i => i.WeldGateValveCase)
                    .ThenInclude(i => i.CaseBottom)
                .Include(i => i.WeldGateValveCase)
                    .ThenInclude(i => i.Rings)
                .Include(i => i.WeldGateValveCase)
                    .ThenInclude(i => i.CoverSleeve008)
                .Include(i => i.WeldGateValveCover)
                    .ThenInclude(i => i.CoverFlange)
                .Include(i => i.WeldGateValveCover)
                    .ThenInclude(i => i.CaseBottom)
                .Include(i => i.WeldGateValveCover)
                    .ThenInclude(i => i.CoverSleeve)
                .Include(i => i.WeldGateValveCover)
                    .ThenInclude(i => i.CoverSleeve008)
                .Include(i => i.WeldGateValveCover)
                    .ThenInclude(i => i.BaseValveSCoverWithSeals)
                        .ThenInclude(i => i.AssemblyUnitSealing)
                .Include(i => i.Saddles)
                    .ThenInclude(i => i.SaddleWithSealings)
                        .ThenInclude(i => i.FrontalSaddleSealing)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }

        public async Task<SheetGateValve> GetByIdIncludeAsync(int id)
        {
            var result = await db.SheetGateValves
                .Include(i => i.PID)
                    .ThenInclude(i => i.Specification)                
                .Include(i => i.WeldGateValveCase)
                    //.ThenInclude(i => i.CoverFlange)
                //.Include(i => i.WeldGateValveCase)
                    //.ThenInclude(i => i.CaseBottom)  
                //.Include(i => i.WeldGateValveCover)
                    //.ThenInclude(i => i.CoverFlange)
                //.Include(i => i.WeldGateValveCover)
                    //.ThenInclude(i => i.CaseBottom)
                .Include(i => i.WeldGateValveCover)
                    .ThenInclude(i => i.Column)
                    .ThenInclude(i => i.RunningSleeve)                
                .Include(i => i.WeldGateValveCover)
                    .ThenInclude(i => i.Spindle)                                
                .Include(i => i.Saddles)                    
                .Include(i => i.Gate) 
                .Include(i => i.CounterFlanges)
                .Include(i => i.Nozzles)                    
                .Include(i => i.BaseValveWithScrewNuts)
                    .ThenInclude(i => i.ScrewNut)
                .Include(i => i.BaseValveWithScrewStuds)
                    .ThenInclude(i => i.ScrewStud)
                .Include(i => i.BaseValveWithShearPins)
                    .ThenInclude(i => i.ShearPin)
                .Include(i => i.BaseValveWithSprings)
                    .ThenInclude(i => i.Spring)
                .Include(i => i.BaseValveWithSeals)
                    .ThenInclude(i => i.MainFlangeSealing)
                .Include(i => i.BaseValveWithCoatings)
                    .ThenInclude(i => i.BaseAnticorrosiveCoating)                
                .Include(i => i.SheetGateValveJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.OperationType)                
                .Include(i => i.CoatingJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.OperationType)  
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
