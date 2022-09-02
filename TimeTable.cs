using Microsoft.EntityFrameworkCore;

namespace TimeTableBot
{
    [PrimaryKey(nameof(GuildId))]
    public record TimeTable
    {
        public ulong GuildId { get; set; }

        //Days, Odd weeks
        public string? MondayOdd { get; set; } = "mon1";
        public string? TuesdayOdd { get; set; } = "tue1";
        public string? WednesdayOdd { get; set; } = "wed1";
        public string? ThursdayOdd { get; set; } = "thu1";
        public string? FridayOdd { get; set; } = "fri1";
        public string? SaturdayOdd { get; set; } = "sat1";

        //Even weeks
        public string? MondayEven { get; set; } = "mon2";
        public string? TuesdayEven { get; set; } = "tue2";
        public string? WednesdayEven { get; set; } = "wed2";
        public string? ThursdayEven { get; set; } = "thu2";
        public string? FridayEven { get; set; } = "fri2";
        public string? SaturdayEven { get; set; } = "sat2";

        /// <summary>
        /// The day which is considered the Odd Monday, depending on this <see cref="GetTimeTable(TimeTable, DateOnly)"/> will return different days.
        /// This is recommended to be either the first studying day or the first odd monday BEFORE it.
        /// </summary>
        public DateOnly FirstDay { get; set; }

        /// <summary>
        /// Gets a day which corresponds with specified <paramref name="date"/>, using <see cref="FirstDay"/> as the root.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="date"></param>
        /// <returns>The value for the specified day, or <see langword="null"/> if this day is a Sunday.</returns>
        public string? GetTimeTable(DateOnly date)
        {
            int offset = (date.DayNumber - FirstDay.DayNumber) % 14;

            return offset switch
            {
                0 => MondayOdd,
                1 => TuesdayOdd,
                2 => WednesdayOdd,
                3 => ThursdayOdd,
                4 => FridayOdd,
                5 => SaturdayOdd,

                7 => MondayEven,
                8 => TuesdayEven,
                9 => WednesdayEven,
                10 => ThursdayEven,
                11 => FridayEven,
                12 => SaturdayEven,

                _ => null
            };
        }
    }
}
