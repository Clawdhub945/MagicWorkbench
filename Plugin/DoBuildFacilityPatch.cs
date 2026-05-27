using HarmonyLib;
using UnityEngine;

namespace MagicWorkbenchPlugin;

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
