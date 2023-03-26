using Microsoft.EntityFrameworkCore;
using SQL_Injection_Fix.Entities;

namespace SQL_Injection_Fix.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base()
        {
            
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json")
               .Build();
            var ConnectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
             .Property(u => u.Id)
             .ValueGeneratedOnAdd();
        }

        public DbSet<User> Users { get; set; }
    }
}
