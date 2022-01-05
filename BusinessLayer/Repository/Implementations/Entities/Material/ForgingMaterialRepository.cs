using BusinessLayer.Repository.Interfaces.Entities.Material;
using DataLayer;
using DataLayer.Entities.Detailing;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using DataLayer.Journals.Materials;
using DataLayer.TechnicalControlPlans.Materials;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BusinessLayer.Repository.Implementations.Entities.Material
{
    public class ForgingMaterialRepository : RepositoryWithJournal<ForgingMaterial, ForgingMaterialJournal, ForgingMaterialTCP>
    {
        public ForgingMaterialRepository(DataContext context) : base(context) { }

        public async Task<ForgingMaterial> GetByIdIncludeAsync(int id)
        {          
                 var result = await db.ForgingMaterials
                //.Include(i => i.CoverSleeve)
                //.Include(i => i.Nozzle)
                //.Include(i => i.Saddle)
                //.Include(i => i.CounterFlange)
                //.Include(i => i.WeldGateValveCover)
                //.Include(i => i.WeldGateValveCase)
                .Include(i => i.ForgingMaterialJournals)
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }        

        public async Task<bool> IsAssembliedAsync(CoverSleeve sleeve)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.ForgingMaterials.Include(i => i.CoverSleeve).SingleOrDefaultAsync(i => i.Id == sleeve.ForgingMaterial.Id);
                if (detail?.CoverSleeve != null && detail.CoverSleeve.Id != sleeve.Id)
                {
                    MessageBox.Show($"Поковка применена в {detail.CoverSleeve.Name} № {detail.CoverSleeve.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task<bool> IsAssembliedAsync(Nozzle nozzle)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.ForgingMaterials.Include(i => i.Nozzle).SingleOrDefaultAsync(i => i.Id == nozzle.ForgingMaterial.Id);
                if (detail?.Nozzle != null && detail.Nozzle.Id != nozzle.Id)
                {
                    MessageBox.Show($"Отливка применена в {detail.Nozzle.Name} № {detail.Nozzle.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task<bool> IsAssembliedAsync(Saddle saddle)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.ForgingMaterials.Include(i => i.Saddle).SingleOrDefaultAsync(i => i.Id == saddle.ForgingMaterial.Id);
                if (detail?.Saddle != null && detail.Saddle.Id != saddle.Id)
                {
                    MessageBox.Show($"Поковка применена в {detail.Saddle.Name} № {detail.Saddle.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task<bool> IsAssembliedAsync(WeldGateValveCover cover)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.ForgingMaterials.Include(i => i.WeldGateValveCover).SingleOrDefaultAsync(i => i.Id == cover.ForgingMaterial.Id);
                if (detail?.WeldGateValveCover != null && detail.WeldGateValveCover.Id != cover.Id)
                {
                    MessageBox.Show($"Поковка применена в {detail.WeldGateValveCover.Name} № {detail.WeldGateValveCover.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task<bool> IsAssembliedAsync(WeldGateValveCase ValveCase)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.ForgingMaterials.Include(i => i.WeldGateValveCase).SingleOrDefaultAsync(i => i.Id == ValveCase.ForgingMaterial.Id);
                if (detail?.WeldGateValveCase != null && detail.WeldGateValveCase.Id != ValveCase.Id)
                {
                    MessageBox.Show($"Поковка применена в {detail.WeldGateValveCase.Name} № {detail.WeldGateValveCase.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task<bool> IsAssembliedAsync(CounterFlange counter)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.ForgingMaterials.Include(i => i.CounterFlange).SingleOrDefaultAsync(i => i.Id == counter.ForgingMaterial.Id);
                if (detail?.CounterFlange != null && detail.CounterFlange.Id != counter.Id)
                {
                    MessageBox.Show($"Поковка применена в {detail.CounterFlange.Name} № {detail.CounterFlange.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task<bool> IsAssembliedAsync(Ring043 ring)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.ForgingMaterials.Include(i => i.Ring043).SingleOrDefaultAsync(i => i.Id == ring.ForgingMaterial.Id);
                if (detail?.Ring043 != null && detail.Ring043.Id != ring.Id)
                {
                    MessageBox.Show($"Поковка применена в {detail.Ring043.Name} № {detail.Ring043.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task<bool> IsAssembliedAsync(Ring047 ring)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.ForgingMaterials.Include(i => i.Ring047).SingleOrDefaultAsync(i => i.Id == ring.ForgingMaterial.Id);
                if (detail?.Ring047 != null && detail.Ring047.Id != ring.Id)
                {
                    MessageBox.Show($"Поковка применена в {detail.Ring047.Name} № {detail.Ring047.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public async Task Load()
        {
            await db.ForgingMaterials.LoadAsync();
        }

        //public async Task LoadRings()
        //{
        //    await db.ForgingMaterials.Include(i => i.Ring043).Include(i => i.Ring047).LoadAsync();
        //}

        public IList<ForgingMaterial> GetByDetail(string str)
        {
            return db.ForgingMaterials.Local.Where(i => i.Target == str).ToList();
        }      


        //public async Task<IList<ForgingMaterial>> GetAllAsync(string str)
        //{

        //    await db.ForgingMaterials.Where(ForgingMaterial => ForgingMaterial.Target == str).LoadAsync();
        //    return db.ForgingMaterials.Local.ToObservableCollection();
        //}

        //public override async Task<IList<SheetGateValveCase>> GetAllAsync()
        //{
        //    await db.SheetGateValveCases.LoadAsync();
        //    await db.SheetGateValveCases.Include(i => i.PID).LoadAsync();
        //    var result = db.SheetGateValveCases.Local.ToObservableCollection();
        //    return result;
        //}

    }
}
