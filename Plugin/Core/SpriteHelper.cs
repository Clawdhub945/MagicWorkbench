using UnityEngine;

namespace MagicWorkbenchPlugin;

public static class SpriteHelper
{
    public static void ReplaceSprite(Facility facility, string spriteName)
    {
        var sprite = SpriteManager.Get(spriteName);
        if (sprite == null) return;

        // MySpriteRenderer（游戏自定义渲染系统）
        var spBody = facility.sp_body;
        if (spBody != null)
        {
            spBody.sprite_id = SpriteManager.Ins.TryGetSpriteId(spriteName);
            spBody.SetActive(false);
            spBody.SetActive(true);
        }

        // 直接设置 body/sp 下的 SpriteRenderer
        var bodySp = facility.transform.Find("body/sp");
        if (bodySp != null)
        {
            var sr = bodySp.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.sprite = sprite; sr.enabled = true; }
        }

        // 遍历所有 SpriteRenderer
        foreach (var sr in facility.gameObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sprite = sprite;
            sr.enabled = true;
        }
    }
}
