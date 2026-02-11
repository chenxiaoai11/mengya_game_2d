using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class TimelineSkipManager
{
    private static HashSet<string> skipOnce = new HashSet<string>();

    public static void SetSkipOnce(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName)) skipOnce.Add(sceneName);
    }

    public static bool ConsumeSkipIfSet(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        if (skipOnce.Contains(sceneName))
        {
            skipOnce.Remove(sceneName);
            return true;
        }
        return false;
    }

    public static bool ConsumeSkipForActiveScene()
    {
        var sn = SceneManager.GetActiveScene().name ?? "";
        return ConsumeSkipIfSet(sn);
    }
}
