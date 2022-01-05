﻿using DataLayer;
using DataLayer.Entities.Materials.AnticorrosiveCoating;
using DataLayer.Journals.Materials.AnticorrosiveCoating;
using DataLayer.TechnicalControlPlans.Materials.AnticorrosiveCoating;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Repository.Implementations.Entities.Material
{
    public class AbrasiveMaterialRepository : RepositoryWithJournal<AbrasiveMaterial, AbrasiveMaterialJournal, AnticorrosiveCoatingTCP>
    {
        public AbrasiveMaterialRepository(DataContext context) : base(context) { }

        public async Task<AbrasiveMaterial> GetByIdIncludeAsync(int id)
        {
            return await db.AbrasiveMaterials.Include(i => i.AbrasiveMaterialJournals)
                .Include(i => i.BaseValveWithCoatings).ThenInclude(i => i.BaseValve)
                
                .SingleOrDefaultAsync(i => i.Id == id);
        }

        //public override async Task<IList<AbrasiveMaterial>> GetAllAsync()
        //{
        //    await db.AbrasiveMaterials.Include(i => i.AbrasiveMaterialJournals).LoadAsync();
        //    return db.AbrasiveMaterials.Local.ToObservableCollection();
        //}
    }
}
