using BusinessLayer.Repository.Interfaces.Entities.Material;
using DataLayer;
using DataLayer.Entities.Detailing.WeldGateValveDetails;
using DataLayer.Entities.Materials;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BusinessLayer.Repository.Implementations.Entities.Material
{
    public class MetalMaterialRepository : Repository<MetalMaterial>, IMetalMaterialRepository
    {
        public MetalMaterialRepository(DataContext context) : base(context) { }

        //public async Task<IList<MetalMaterial>> GetAllAsyncSorted()
        //{
        //    await db.MetalMaterials.OrderBy(i => i.Melt).LoadAsync();
        //    return db.MetalMaterials.Local.ToObservableCollection();
        //}

        public async Task Load()
        {
            await db.MetalMaterials.LoadAsync();
        }

        public IList<MetalMaterial> SortList()
        {
            return db.MetalMaterials.Local.OrderBy(i => i.Melt).ToList();
        }

    }
}
