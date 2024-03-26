using Microsoft.EntityFrameworkCore;
using SOTags.Model;

namespace SOTags.Data
{
    public class SOTagsDBContext:DbContext
    {
        public DbSet<Tag> Pizzas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SOTagsDB;Trusted_Connection=True;");
        }
    }
}
