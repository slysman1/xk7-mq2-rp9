using System.Collections.Generic;

public static class NotificationSession
{
    private static readonly HashSet<string> shown = new();

    public static bool CanShow(string key)
    {
        if (shown.Contains(key))
            return false;

        shown.Add(key);
        return true;
    }
}
