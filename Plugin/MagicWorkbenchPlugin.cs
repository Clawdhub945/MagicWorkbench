using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
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
