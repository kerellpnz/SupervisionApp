using DataLayer;

namespace BusinessLayer.Repository.Implementations.Entities
{
    public class VersionControlRepository : Repository<VersionControl>
    {
        public VersionControlRepository(DataContext context) : base(context) { }

        
    }
}
