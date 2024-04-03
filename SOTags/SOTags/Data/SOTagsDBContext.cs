using Microsoft.EntityFrameworkCore;
using SOTags.Model;
using System.Runtime.InteropServices;

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
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)?"Server=(localdb)\\mssqllocaldb;Database=SOTagsDB;Trusted_Connection=True;": "Server=SqlServerDb;Database=TestDB;User=sa;Password=myStong_Password123#;Trust Server Certificate=True;");
            }
        }
    }
}
