﻿using System.Collections.Generic;
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
    public class SaddleRepository : RepositoryWithJournal<Saddle, SaddleJournal, SaddleTCP>, ISaddleRepository
    {
        public SaddleRepository(DataContext context) : base(context)
        {
        }

        public async Task<bool> IsAssembliedAsync(Saddle saddle, BaseValve valve)
        {
            using (DataContext context = new DataContext())
            {
                var detail = await context.Saddles.Include(i => i.BaseValve).SingleOrDefaultAsync(i => i.Id == saddle.Id);
                if (detail?.BaseValve != null && detail.BaseValve.Id != valve.Id)
                {
                    MessageBox.Show($"Обойма применена в {detail.BaseValve.Name} № {detail.BaseValve.Number}", "Ошибка");
                    return true;
                }
                else return false;
            }
        }

        public new async Task<IEnumerable<SaddleTCP>> GetTCPsAsync()
        {
            await db.SaddleTCPs.Include(i => i.OperationType).LoadAsync();
            return db.SaddleTCPs.Local.ToObservableCollection();
        }

        public override async Task<IList<Saddle>> GetAllAsync()
        {
            //await db.Saddles.Include(i => i.MetalMaterial).LoadAsync();
            await db.Saddles.Include(i => i.PID).LoadAsync();
            var result = db.Saddles.Local.ToObservableCollection();
            return result;
        }

        public async Task<IList<Saddle>> GetAllAsyncForCompare()
        {            
            await db.Saddles.LoadAsync();            
            return db.Saddles.Local.ToObservableCollection();
        }

        public async Task Load()
        {
            await db.Saddles.LoadAsync();
        }

        public IList<Saddle> UpdateList()
        {
            return db.Saddles.Local.Where(i => i.BaseValveId == null).OrderBy(i => i.ZK).ToList();
        }

        

        public async Task<Saddle> GetByIdIncludeAsync(int id)
        {
            var result = await db.Saddles
                .Include(i => i.SaddleWithSealings)
                    .ThenInclude(i => i.FrontalSaddleSealing)
                .Include(i => i.BaseValve)
                .Include(i => i.SaddleJournals)
                    .ThenInclude(i => i.EntityTCP)
                    .ThenInclude(i => i.ProductType)
                .Include(i => i.MetalMaterial)
                .Include(i => i.ForgingMaterial)                
                .SingleOrDefaultAsync(i => i.Id == id);
            return result;
        }
    }
}
