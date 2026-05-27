using System.Collections.Generic;

namespace MagicWorkbenchPlugin;

public static class BuildingRegistry
{
    public static readonly Dictionary<int, (string basePrefab, string sprite)> Buildings = new();

    public static void Register(int stuffId, string basePrefab, string sprite)
        => Buildings[stuffId] = (basePrefab, sprite);
}
