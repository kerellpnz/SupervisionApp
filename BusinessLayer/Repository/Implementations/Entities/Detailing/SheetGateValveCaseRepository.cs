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
    public class SheetGateValveCaseRepository : RepositoryWithJournal<SheetGateValveCase, SheetGateValveCaseJournal, SheetGateValveCaseTCP>
    {
        public SheetGateValveCaseRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(SheetGateValve valve)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.SheetGateValveCases.Include(i => i.BaseWeldValve).SingleOrDefaultAsync(i => i.Id == valve.WeldGateValveCase.Id);
                if (detail?.BaseWeldValve != null && detail.BaseWeldValve.Id != valve.Id)
                {
                    MessageBox.Show($"Корпус применен в {detail.BaseWeldValve.Name} № {detail.BaseWeldValve.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public new async Task<IEnumerable<SheetGateValveCaseTCP>> GetTCPsAsync()
        {
            await db.SheetGateValveCaseTCPs.Include(i => i.OperationType).LoadAsync();
            return db.SheetGateValveCaseTCPs.Local.ToObservableCollection();
        }
        

        public async Task Load()
        {
            await db.SheetGateValveCases
                //.Include(i => i.CaseBottom)
                //.Include(i => i.CoverFlange)
                .LoadAsync();
        }

        public IList<SheetGateValveCase> SortList()
        {
            return db.SheetGateValveCases.Local.OrderBy(i => i.Number).ToList();
        }


        public override async Task<IList<SheetGateValveCase>> GetAllAsync()
        {            
            await db.SheetGateValveCases.Include(i => i.PID).LoadAsync();            
            return db.SheetGateValveCases.Local.ToObservableCollection();
        }

        public async Task<IList<SheetGateValveCase>> GetAllAsyncForCompare()
        {
            await db.SheetGateValveCases.LoadAsync(); 
            return db.SheetGateValveCases.Local.ToObservableCollection(); 
        }

        public async Task<SheetGateValveCase> GetByIdIncludeAsync(int id)
        {
            var result = await db.SheetGateValveCases
                .Include(i => i.BaseWeldValve)
                .Include(i => i.CoverFlange)
                .Include(i => i.Rings)
                //.Include(i => i.Ring047)
                .Include(i => i.CaseBottom)
                .Include(i => i.SheetGateValveCaseJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.OperationType)                
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
