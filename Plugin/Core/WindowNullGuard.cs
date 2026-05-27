using HarmonyLib;

namespace MagicWorkbenchPlugin;

public static class WindowNullGuard
{
    public static void Register(Harmony harmony)
    {
        Patch(harmony, "WindowWorkshop", "ShowWindowTip");
        Patch(harmony, "WindowWorkFacility", "ShowWindowTip");
        Patch(harmony, "WindowWorkshop", "Refresh");
        Patch(harmony, "WindowWorkshop", "UpdateAlternativeFormula");
    }

    private static void Patch(Harmony harmony, string typeName, string methodName)
    {
        var type = AccessTools.TypeByName(typeName);
        if (type == null) return;
        foreach (var m in type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static))
            if (m.Name == methodName) { harmony.Patch(m, prefix: new HarmonyMethod(typeof(WindowNullGuard).GetMethod(nameof(Check)))); break; }
    }

    public static bool Check(object __instance)
    {
        var t = __instance.GetType();
        var wp = AccessTools.Property(t, "workshop");
        if (wp != null && wp.GetValue(__instance) == null) return false;
        var fp = AccessTools.Property(t, "facility");
        if (fp != null && fp.GetValue(__instance) == null) return false;
        return true;
    }
}
