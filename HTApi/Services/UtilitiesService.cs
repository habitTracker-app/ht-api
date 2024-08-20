namespace HTApi.Services
{
    public interface IUtilitiesService
    {
        Tuple<DateTime, DateTime> GetTodayStartEnd(DateTime date);
        Tuple<DateTime, DateTime> GetFortnightStartEnd(DateTime date);
        Tuple<DateTime, DateTime> GetSemesterStartEnd(DateTime date);
        Tuple<DateTime, DateTime> GetMonthStartEnd(DateTime date);
        Tuple<DateTime, DateTime> GetWeekStartEnd(DateTime date);
        Tuple<DateTime, DateTime> GetYearStartEnd(DateTime date);
        Tuple<DateTime, DateTime> GetPeriodStartEnd(string frequency, DateTime date);
    }

    public class UtilitiesService : IUtilitiesService
    {
        public UtilitiesService()
        {
            
        }
        public Tuple<DateTime, DateTime> GetTodayStartEnd(DateTime date)
        {
            DateTime start = date.Date;
            DateTime end = date.Date.AddDays(1).AddTicks(-1);
            return new(start, end);
        }
        public Tuple<DateTime, DateTime> GetFortnightStartEnd(DateTime date)
        {
            if (date.Day <= 15)
            {
                return new(new DateTime(date.Year, date.Month, 1).Date, _getEndOfDay(new DateTime(date.Year, date.Month, 15)));
            }
            else
            {
                int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                return new(new DateTime(date.Year, date.Month, 16).Date, _getEndOfDay(new DateTime(date.Year, date.Month, daysInMonth)));
            }
        }

        public Tuple<DateTime, DateTime> GetSemesterStartEnd(DateTime date)
        {
            if (date.Month <= 6)
            {
                return new(new DateTime(date.Year, 1, 1).Date, _getEndOfDay(new DateTime(date.Year, 6, 30)));
            }
            else
            {
                return new(new DateTime(date.Year, 7, 1).Date, _getEndOfDay(new DateTime(date.Year, 12, 31)));
            }
        }

        public Tuple<DateTime, DateTime> GetMonthStartEnd(DateTime date)
        {
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            return new(new DateTime(date.Year, date.Month, 1).Date, _getEndOfDay(new DateTime(date.Year, date.Month, daysInMonth)));
        }

        public Tuple<DateTime, DateTime> GetWeekStartEnd(DateTime date)
        {
            DayOfWeek startDay = DayOfWeek.Sunday;
            DayOfWeek endDay = DayOfWeek.Saturday;

            int diffStart = (7 + (date.DayOfWeek - startDay)) % 7;
            int diffEnd = (7 + (date.DayOfWeek - endDay)) % 7;

            return new(date.AddDays(-1 * diffStart).Date, _getEndOfDay(date.AddDays(diffEnd).Date));
        }

        public Tuple<DateTime, DateTime> GetYearStartEnd(DateTime date)
        {
            DateTime start = new DateTime(date.Year, date.Month - 1, 1).Date;
            DateTime end = _getEndOfDay(new DateTime(date.Year, 11, 31));

            return new(start, end);
        }


        public Tuple<DateTime, DateTime> GetPeriodStartEnd(string frequency, DateTime date)
        {
            if (frequency == "daily") { return this.GetTodayStartEnd(date); }

            if (frequency == "weekly") { return this.GetWeekStartEnd(date); }

            if (frequency == "monthly") { return this.GetMonthStartEnd(date); }

            if (frequency == "annually") { return this.GetYearStartEnd(date); }

            if (frequency == "biweekly") { return this.GetFortnightStartEnd(date); }

            if (frequency == "biannual") { return this.GetSemesterStartEnd(date); }
            else
            {
                throw new Exception("Not a valid frequency.");
            }
        } 

        private DateTime _getEndOfDay(DateTime date)
        {
            return date.AddDays(1).AddTicks(-1);
        }
    }
}
