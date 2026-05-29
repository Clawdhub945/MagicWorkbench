using HarmonyLib;
using System;
using System.Reflection;

namespace MagicWorkbenchPlugin;

public static class BuildFacilityPatch
{
    private static bool injected;

    public static void Register(Harmony harmony)
    {
        // Hook DoBuildFacility for prefab swapping
        var type = AccessTools.TypeByName("BuildHelper");
        if (type != null)
        {
            MethodBase method = null;
            foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                if (m.Name == "DoBuildFacility") { method = m; break; }

            if (method != null)
            {
                harmony.Patch(method,
                    postfix: new HarmonyMethod(typeof(BuildFacilityPatch).GetMethod(nameof(Postfix))));
                Console.WriteLine("[MagicWorkbench] Patched DoBuildFacility");
            }
        }

        // Hook MainScene.StartGame to inject build_dic entries before save recovery
        var mainSceneType = AccessTools.TypeByName("MainScene");
        if (mainSceneType != null)
        {
            MethodBase startGame = null;
            foreach (var m in mainSceneType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                if (m.Name == "StartGame") { startGame = m; break; }

            if (startGame != null)
            {
                harmony.Patch(startGame,
                    prefix: new HarmonyMethod(typeof(BuildFacilityPatch).GetMethod(nameof(StartGamePrefix))));
                Console.WriteLine("[MagicWorkbench] Patched MainScene.StartGame");
            }
        }
    }

    public static void StartGamePrefix()
    {
        if (injected) return;
        injected = true;
        InjectAllBuildInfos();
    }

    public static void Postfix(int stuff_id, Facility __result)
    {
        if (__result == null || !BuildingRegistry.Buildings.TryGetValue(stuff_id, out var reg)) return;
        SpriteHelper.ReplaceSprite(__result, reg.sprite);
    }

    private static void InjectAllBuildInfos()
    {
        try
        {
            if (D.Ins == null || D.Ins.build_dic == null)
            {
                Console.WriteLine("[MagicWorkbench] D.Ins or build_dic is null, skip injection");
                return;
            }

            foreach (var kvp in BuildingRegistry.Buildings)
            {
                var stuffId = kvp.Key;
                var def = kvp.Value;
                if (!D.Ins.build_dic.ContainsKey(stuffId))
                {
                    InjectBuildInfo(stuffId, def);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[MagicWorkbench] InjectAllBuildInfos error: {e}");
        }
    }

    private static void InjectBuildInfo(int stuffId, BuildingDef def)
    {
        try
        {
            BuildInfo sourceInfo = null;
            foreach (var kvp in D.Ins.build_dic)
            {
                var info = kvp.Value;
                if (info == null) continue;
                // Try to match class_name
                try
                {
                    var cn = info.class_name;
                    if (cn == def.className) { sourceInfo = info; break; }
                }
                catch { }
            }
            // Fallback: use first entry
            if (sourceInfo == null)
            {
                foreach (var kvp in D.Ins.build_dic)
                {
                    sourceInfo = kvp.Value;
                    break;
                }
            }
            if (sourceInfo == null)
            {
                Console.WriteLine($"[MagicWorkbench] No source BuildInfo found for {stuffId}");
                return;
            }

            // Clone the source to avoid corrupting it (var alias would modify the original)
            var newInfo = (BuildInfo)sourceInfo.MemberwiseClone();
            newInfo.id = stuffId;
            newInfo.cellw = def.cellw;
            newInfo.cellh = def.cellh;

            D.Ins.build_dic[stuffId] = newInfo;
            Console.WriteLine($"[MagicWorkbench] Injected build_dic for {stuffId} (cellw={def.cellw}, cellh={def.cellh}, class={def.className})");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[MagicWorkbench] Inject failed for {stuffId}: {e.Message}");
        }
    }
}
