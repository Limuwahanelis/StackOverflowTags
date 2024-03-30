using Microsoft.EntityFrameworkCore;
using SOTags.Model;

namespace SOTags.Data
{
    public class SOTagsDBContext:DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        public SOTagsDBContext (DbContextOptions<SOTagsDBContext> options):base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           if(!optionsBuilder.IsConfigured) optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SOTagsDB;Trusted_Connection=True;");
        }
    }
}
