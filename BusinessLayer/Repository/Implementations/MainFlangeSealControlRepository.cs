using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BusinessLayer.Repository.Implementations.Entities
{
    public class MainFlangeSealControlRepository : Repository<MainFlangeSealControl>
    {
        public MainFlangeSealControlRepository(DataContext context) : base(context) { }

        public async Task<MainFlangeSealControl> GetByIdIncludeAsync(int id)
        {            
            return await db.MainFlangeSealControl.SingleOrDefaultAsync(i => i.Id == id); ;
        }
    }
}
