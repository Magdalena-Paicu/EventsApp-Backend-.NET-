using Microsoft.EntityFrameworkCore;
using NessWebApi.Models;

namespace NessWebApi.Data
{
    public class DbContextNessApp : DbContext
    {
        public DbContextNessApp(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<UploadedFile> UploadedFiles { get; set; }
    }
}
