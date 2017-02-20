using Microsoft.EntityFrameworkCore;

namespace GetStartedDotnet.Models
{
    public class VisitorsDbContext : DbContext
    {
        public VisitorsDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Visitor> Visitors { get; set; }
    }
}
