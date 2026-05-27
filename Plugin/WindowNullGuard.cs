using HarmonyLib;

namespace MagicWorkbenchPlugin;

public static class WindowNullGuard
{
    public static bool Prefix(object __instance)
    {
        var t = __instance.GetType();
        var wp = AccessTools.Property(t, "workshop");
        if (wp != null && wp.GetValue(__instance) == null) return false;
        var fp = AccessTools.Property(t, "facility");
        if (fp != null && fp.GetValue(__instance) == null) return false;
        return true;
    }
}
