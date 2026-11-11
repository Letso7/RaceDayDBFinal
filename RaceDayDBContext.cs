using Microsoft.EntityFrameworkCore;
using RaceDayDB1.Models;

namespace RaceDayDB1.Data
{
    public class RaceDayDBContext : DbContext
    {
        public RaceDayDBContext(DbContextOptions<RaceDayDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
    }
}
