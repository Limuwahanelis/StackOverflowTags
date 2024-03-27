using Microsoft.EntityFrameworkCore;
using SOTags.Model;

namespace SOTags.Data
{
    public class SOTagsDBContext:DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SOTagsDB;Trusted_Connection=True;");
        }
    }
}
