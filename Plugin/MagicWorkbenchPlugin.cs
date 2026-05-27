using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace MagicWorkbenchPlugin;

[BepInPlugin("com.magicworkbench.plugin", "MagicWorkbench", "1.0.0")]
public class MagicWorkbenchPlugin : BasePlugin
{
    public override void Load()
    {
        var harmony = new Harmony("com.magicworkbench.plugin");

        MagicWorkbench.Register();

        BuildFacilityPatch.Register(harmony);
        WindowNullGuard.Register(harmony);
    }
}
