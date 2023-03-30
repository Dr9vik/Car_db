using Data_Access_Layer.Common.Models;
using Data_Access_Layer.ModelConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Data_Access_Layer.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new CarConfiguration());
            modelBuilder.ApplyConfiguration(new UserCarConfiguration());
        }


        public DbSet<CarDB> Cars { get; set; }
        public DbSet<UserCarDB> UserCars { get; set; }
    }
}
