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

        // 挂钩 BuildHelper.DoBuildFacility
        var buildHelperType = AccessTools.TypeByName("BuildHelper");
        if (buildHelperType == null)
        {
            Log.LogError("BuildHelper type not found!");
            return;
        }

        MethodBase doBuildMethod = null;
        foreach (var method in buildHelperType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if (method.Name == "DoBuildFacility")
            {
                doBuildMethod = method;
                break;
            }
        }

        if (doBuildMethod == null)
        {
            Log.LogError("DoBuildFacility method not found!");
            return;
        }

        var prefix = typeof(DoBuildFacilityPatch).GetMethod(nameof(DoBuildFacilityPatch.Prefix));
        var postfix = typeof(DoBuildFacilityPatch).GetMethod(nameof(DoBuildFacilityPatch.Postfix));
        harmony.Patch(doBuildMethod, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
        Log.LogInfo("Patched BuildHelper.DoBuildFacility");

        // 空保护：防止 WindowWorkshop 因 workshop 为 null 而崩溃
        PatchWindowNullGuard("WindowWorkshop", "ShowWindowTip");
        PatchWindowNullGuard("WindowWorkFacility", "ShowWindowTip");
        PatchWindowNullGuard("WindowWorkshop", "Refresh");
        PatchWindowNullGuard("WindowWorkshop", "UpdateAlternativeFormula");
    }

    private void PatchWindowNullGuard(string typeName, string methodName)
    {
        var type = AccessTools.TypeByName(typeName);
        if (type == null) return;

        foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            if (m.Name == methodName)
            {
                try
                {
                    var guardPrefix = typeof(WindowNullGuard).GetMethod(nameof(WindowNullGuard.Prefix));
                    harmony.Patch(m, prefix: new HarmonyMethod(guardPrefix));
                    Log.LogInfo("Patched " + typeName + "." + methodName + " (null guard)");
                }
                catch (System.Exception e)
                {
                    Log.LogWarning("Failed to patch " + typeName + "." + methodName + ": " + e.Message);
                }
                break;
            }
        }
    }
}

public static class DoBuildFacilityPatch
{
    private static string savedPrefab = null;

    // Prefix: 临时把 prefab 改为 "workbench"，让游戏用制造台预制体（含 FacilityBlacksmith 组件）
    public static bool Prefix(int stuff_id, int rotation)
    {
        if (stuff_id != 105099) return true;

        var plugin = MagicWorkbenchPlugin.Instance;

        // 直接修改 prefab 字段，让游戏内部自己查找预制体
        var stuffDic = D.Ins.stuff_dic;
        if (stuffDic.TryGetValue(105099, out var stuffInfo))
        {
            savedPrefab = stuffInfo.prefab;
            stuffInfo.prefab = "workbench";
            plugin.Log.LogInfo("Prefab changed: '" + savedPrefab + "' -> 'workbench' (rotation=" + rotation + ")");
        }
        else
        {
            plugin.Log.LogError("stuff_id 105099 not found in stuff_dic!");
        }

        return true;
    }

    // Postfix: 替换精灵图 + 恢复 prefab 字段
    public static void Postfix(int stuff_id, Facility __result)
    {
        // 恢复原始 prefab 字段
        if (savedPrefab != null)
        {
            var stuffDic = D.Ins.stuff_dic;
            if (stuffDic.TryGetValue(105099, out var stuffInfo))
            {
                stuffInfo.prefab = savedPrefab;
            }
            savedPrefab = null;
        }

        if (__result == null || stuff_id != 105099) return;

        var plugin = MagicWorkbenchPlugin.Instance;
        plugin.Log.LogInfo("Postfix: facility type=" + __result.GetType().Name);

        // 替换精灵
        var customSprite = SpriteManager.Get("super_magic_workbench_0");
        if (customSprite == null)
        {
            plugin.Log.LogError("Sprite 'super_magic_workbench_0' not found!");
            return;
        }

        // 方式1: 通过 MySpriteRenderer 设置
        var spBody = __result.sp_body;
        if (spBody != null)
        {
            var spriteId = SpriteManager.Ins.TryGetSpriteId("super_magic_workbench_0");
            spBody.sprite_id = spriteId;
            spBody.SetActive(false);
            spBody.SetActive(true);
            plugin.Log.LogInfo("Set via MySpriteRenderer.sprite_id");
        }

        // 方式2: 直接找到 "body/sp" 下的 SpriteRenderer，设置精灵并启用
        var bodySp = __result.transform.Find("body/sp");
        if (bodySp != null)
        {
            var sr = bodySp.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = customSprite;
                sr.enabled = true;
                plugin.Log.LogInfo("Set via direct SpriteRenderer on body/sp");
            }
        }

        // 方式3: 遍历所有 SpriteRenderer（包括禁用的）
        var allSrs = __result.gameObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in allSrs)
        {
            sr.sprite = customSprite;
            sr.enabled = true;
        }
        plugin.Log.LogInfo("Set " + allSrs.Length + " SpriteRenderers");
    }
}

// 空引用保护：当 workshop/facility 字段为 null 时跳过原方法
public static class WindowNullGuard
{
    private static bool logged = false;

    public static bool Prefix(object __instance)
    {
        var workshopProp = AccessTools.Property(__instance.GetType(), "workshop");
        if (workshopProp != null)
        {
            var val = workshopProp.GetValue(__instance);
            if (val == null)
            {
                if (!logged)
                {
                    MagicWorkbenchPlugin.Instance?.Log.LogWarning("WindowNullGuard: workshop is null in " + __instance.GetType().Name);
                    logged = true;
                }
                return false;
            }
        }

        var facilityProp = AccessTools.Property(__instance.GetType(), "facility");
        if (facilityProp != null)
        {
            var val = facilityProp.GetValue(__instance);
            if (val == null)
            {
                if (!logged)
                {
                    MagicWorkbenchPlugin.Instance?.Log.LogWarning("WindowNullGuard: facility is null in " + __instance.GetType().Name);
                    logged = true;
                }
                return false;
            }
        }

        return true;
    }
}
