using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace MagicWorkbenchPlugin;

[BepInPlugin("com.magicworkbench.plugin", "MagicWorkbench", "1.0.0")]
public class MagicWorkbenchPlugin : BasePlugin
{
    public static MagicWorkbenchPlugin Instance;
    private Harmony harmony;

    public override void Load()
    {
        Instance = this;
        harmony = new Harmony("com.magicworkbench.plugin");

        var buildHelperType = AccessTools.TypeByName("BuildHelper");
        if (buildHelperType == null) { Log.LogError("BuildHelper not found"); return; }

        MethodBase doBuildMethod = null;
        foreach (var m in buildHelperType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            if (m.Name == "DoBuildFacility") { doBuildMethod = m; break; }

        if (doBuildMethod == null) { Log.LogError("DoBuildFacility not found"); return; }

        harmony.Patch(doBuildMethod,
            prefix: new HarmonyMethod(typeof(DoBuildFacilityPatch).GetMethod(nameof(DoBuildFacilityPatch.Prefix))),
            postfix: new HarmonyMethod(typeof(DoBuildFacilityPatch).GetMethod(nameof(DoBuildFacilityPatch.Postfix))));

        PatchNullGuard("WindowWorkshop", "ShowWindowTip");
        PatchNullGuard("WindowWorkFacility", "ShowWindowTip");
        PatchNullGuard("WindowWorkshop", "Refresh");
        PatchNullGuard("WindowWorkshop", "UpdateAlternativeFormula");
    }

    private void PatchNullGuard(string typeName, string methodName)
    {
        var type = AccessTools.TypeByName(typeName);
        if (type == null) return;
        foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            if (m.Name == methodName) { harmony.Patch(m, prefix: new HarmonyMethod(typeof(WindowNullGuard).GetMethod(nameof(WindowNullGuard.Prefix)))); break; }
    }
}

public static class DoBuildFacilityPatch
{
    private static string savedPrefab;

    public static bool Prefix(int stuff_id, int rotation)
    {
        if (stuff_id != 105099) return true;
        if (D.Ins.stuff_dic.TryGetValue(105099, out var info))
        {
            savedPrefab = info.prefab;
            info.prefab = "workbench";
        }
        return true;
    }

    public static void Postfix(int stuff_id, Facility __result)
    {
        if (savedPrefab != null)
        {
            if (D.Ins.stuff_dic.TryGetValue(105099, out var info)) info.prefab = savedPrefab;
            savedPrefab = null;
        }
        if (__result == null || stuff_id != 105099) return;

        var sprite = SpriteManager.Get("super_magic_workbench_0");
        if (sprite == null) return;

        var spBody = __result.sp_body;
        if (spBody != null)
        {
            spBody.sprite_id = SpriteManager.Ins.TryGetSpriteId("super_magic_workbench_0");
            spBody.SetActive(false);
            spBody.SetActive(true);
        }

        var bodySp = __result.transform.Find("body/sp");
        if (bodySp != null)
        {
            var sr = bodySp.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.sprite = sprite; sr.enabled = true; }
        }

        foreach (var sr in __result.gameObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sprite = sprite;
            sr.enabled = true;
        }
    }
}

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
