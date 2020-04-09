namespace OutageManagementService.Report
{
    using System;
    using System.Collections.Generic;

    public static class DateHelpers
    {
        public static string MONTHLY_TYPE = "Monthly";
        public static string YEARLY_TYPE = "Yearly";

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

        public static string GetType(DateTime? startDate, DateTime? endDate) =>
             startDate.HasValue ? endDate.HasValue ?
                        endDate.Value.Subtract(startDate.Value).Days / (365.2425 / 12) < 12
                            ? MONTHLY_TYPE
                            : YEARLY_TYPE
                    : DateTime.Now.Subtract(startDate.Value).Days / (365.2425 / 12) < 12
                        ? MONTHLY_TYPE
                        : YEARLY_TYPE
                    : YEARLY_TYPE;
    }
}
