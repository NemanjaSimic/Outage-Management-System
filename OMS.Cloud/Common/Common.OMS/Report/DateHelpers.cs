using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.OMS.Report
{
    public static class DateHelpers
    {
        public static string MONTHLY_TYPE = "Monthly";
        public static string YEARLY_TYPE = "Yearly";
        public static string DAILY_TYPE = "Daily";

        public static Dictionary<int, string> Months = new Dictionary<int, string>
        {
            { 1, "January" },
            { 2, "February" },
            { 3, "March" },
            { 4, "April" },
            { 5, "May" },
            { 6, "June" },
            { 7, "July" },
            { 8, "August" },
            { 9, "September" },
            { 10, "October" },
            { 11, "November" },
            { 12, "December" }
        };

        public static string GetType(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                int dayDiffernce = endDate.Value.Subtract(startDate.Value).Days;

                if (dayDiffernce > 364)
                {
                    return YEARLY_TYPE;
                }
                else if (dayDiffernce > 1)
                {
                    return MONTHLY_TYPE;
                }
                else
                {
                    return DAILY_TYPE;
                }
            }
            else
            {
                return YEARLY_TYPE;
            }
        }
    }
}
