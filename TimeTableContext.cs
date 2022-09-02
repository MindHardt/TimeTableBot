using Microsoft.EntityFrameworkCore;

namespace TimeTableBot
{
    public class TimeTableContext : DbContext
    {
        public DbSet<TimeTable> TimeTables { get; set; } = null!;

        public TimeTableContext(DbContextOptions<TimeTableContext> options)
            : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}
