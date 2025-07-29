using Microsoft.EntityFrameworkCore;

namespace CoreAdminWeb.Models
{
    /// <summary>
    /// Application Database Context for DRCARE_CORE database
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // No entity configurations needed since we're using raw SQL commands
            // All data access is done through SqlDataReader in controllers
        }
    }
} 