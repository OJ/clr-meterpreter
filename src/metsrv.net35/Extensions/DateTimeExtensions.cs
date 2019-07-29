using System;

namespace Met.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static ulong ToUnixTimestamp(this DateTime dateTime)
        {
            return (ulong)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
