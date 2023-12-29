using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VideoHosting_Backend.Services
{
    public static class DateServices
    {
        public static string getDifferenceBetweenDates(DateTime publicationDate)
        {
            string date = "";
            DateTime currentDate = DateTime.Now;
            TimeSpan timeDifference = currentDate - publicationDate;

            if (timeDifference.Days >= 365)
            {
                int yearsDifference = timeDifference.Days / 365;
                date = $"{yearsDifference} {(yearsDifference == 1 ? "year" : "years")} ago";
            }
            else if (timeDifference.Days >= 30)
            {
                int monthsDifference = timeDifference.Days / 30;
                date = $"{monthsDifference} {(monthsDifference == 1 ? "month" : "months")} ago";
            }
            else if (timeDifference.Days >= 1)
            {
                date = $"{timeDifference.Days} {(timeDifference.Days == 1 ? "day" : "days")} ago";
            }
            else if (timeDifference.Hours >= 1)
            {
                date = $"{timeDifference.Hours} {(timeDifference.Hours == 1 ? "hour" : "hours")} ago";
            }
            else if (timeDifference.Minutes >= 1)
            {
                date = $"{timeDifference.Minutes} {(timeDifference.Hours == 1 ? "minute" : "minutes")} ago";

            }
            else if (timeDifference.Seconds >= 1)
            {
                date = $"{timeDifference.Seconds} {(timeDifference.Seconds == 1 ? "second" : "seconds")} ago";

            }
            return date;
        }
    }
}
