using Microsoft.EntityFrameworkCore;

namespace KataevLIB
{
    public class ApplicationContext : DbContext
    {
        public DbSet<KataevPartner> KataevPartners { get; set; }
        public DbSet<KataevProduct> KataevProducts { get; set; }
        public DbSet<KataevSalesHistory> KataevSalesHistories { get; set; }

        public ApplicationContext()
        {
          
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=kataev;Username=app;Password=123456789;");
        }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
    }
}
