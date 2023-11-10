using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Types;

namespace Supplier.Services
{
    public class CodeObjectContext : DbContext
    {
        public CodeObjectContext(DbContextOptions<CodeObjectContext> options) : base(options)
        {
        }

        public DbSet<CodeObject> METODO { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CodeObject>().HasKey("MT_CODIGO");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder =>
            {
                builder.AddFilter(_ => false);
            }));
        }
    }
}
