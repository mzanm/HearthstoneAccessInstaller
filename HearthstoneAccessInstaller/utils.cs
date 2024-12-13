using System;
namespace HearthstoneAccessInstaller;
public static class Utils
{
    public static string GetElapsedTime(DateTimeOffset date)
    {
        TimeSpan timeDiff = DateTimeOffset.UtcNow - date;

        return timeDiff switch
        {
            { TotalSeconds: < 60 } => $"{(int)timeDiff.TotalSeconds} seconds ago",
            { TotalMinutes: < 60 } => $"{(int)timeDiff.TotalMinutes} minutes ago",
            { TotalHours: < 24 } => $"{(int)timeDiff.TotalHours} hours ago",
            _ => $"{(int)timeDiff.TotalDays} days ago"
        };
    }
}
