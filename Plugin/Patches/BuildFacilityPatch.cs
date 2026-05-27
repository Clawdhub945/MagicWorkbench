using HarmonyLib;
using System.Reflection;

namespace MagicWorkbenchPlugin;

public static class BuildFacilityPatch
{
    private static string savedPrefab;

    public static void Register(Harmony harmony)
    {
        var type = AccessTools.TypeByName("BuildHelper");
        if (type == null) return;

        MethodBase method = null;
        foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            if (m.Name == "DoBuildFacility") { method = m; break; }

        if (method == null) return;

        harmony.Patch(method,
            prefix: new HarmonyMethod(typeof(BuildFacilityPatch).GetMethod(nameof(Prefix))),
            postfix: new HarmonyMethod(typeof(BuildFacilityPatch).GetMethod(nameof(Postfix))));
    }

    public static bool Prefix(int stuff_id, int rotation)
    {
        if (!BuildingRegistry.Buildings.TryGetValue(stuff_id, out var reg)) return true;
        if (D.Ins.stuff_dic.TryGetValue(stuff_id, out var info))
        {
            savedPrefab = info.prefab;
            info.prefab = reg.basePrefab;
        }
        return true;
    }

    public static void Postfix(int stuff_id, Facility __result)
    {
        if (savedPrefab != null)
        {
            if (D.Ins.stuff_dic.TryGetValue(stuff_id, out var info)) info.prefab = savedPrefab;
            savedPrefab = null;
        }
        if (__result == null || !BuildingRegistry.Buildings.TryGetValue(stuff_id, out var reg)) return;
        SpriteHelper.ReplaceSprite(__result, reg.sprite);
    }
}
