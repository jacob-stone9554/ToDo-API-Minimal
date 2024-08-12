using Microsoft.EntityFrameworkCore;
using ToDoAPI_Minimal.Models;

namespace ToDoAPI_Minimal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Item> Items { get; set; }
    }
}
