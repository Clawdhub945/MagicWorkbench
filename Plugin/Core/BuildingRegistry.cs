using System.Collections.Generic;

namespace MagicWorkbenchPlugin;

public static class BuildingRegistry
{
    public static readonly Dictionary<int, BuildingDef> Buildings = new();

    public static void Register(int stuffId, string basePrefab, string sprite, int cellw, int cellh, string className)
        => Buildings[stuffId] = new BuildingDef
        {
            basePrefab = basePrefab,
            sprite = sprite,
            cellw = cellw,
            cellh = cellh,
            className = className
        };
}

public class BuildingDef
{
    public string basePrefab;
    public string sprite;
    public int cellw;
    public int cellh;
    public string className;
}
