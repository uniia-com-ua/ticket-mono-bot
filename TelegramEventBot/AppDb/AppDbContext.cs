using Microsoft.EntityFrameworkCore;
using TelegramEventBot.Models;

namespace TelegramEventBot.AppDb
{
    public class AppDbContext : DbContext
    {
        public DbSet<EventUserModel> EventUsers { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
