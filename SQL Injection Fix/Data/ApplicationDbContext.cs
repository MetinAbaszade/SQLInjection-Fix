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
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-G1Q07RP;Initial Catalog=SQL-Injection_Test;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
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
