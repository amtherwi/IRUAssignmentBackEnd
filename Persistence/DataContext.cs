using Domain.IruAssignment;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {     
        }

        public DbSet<IruAssignment> IruAssignments { get; set; }
        
    }
}