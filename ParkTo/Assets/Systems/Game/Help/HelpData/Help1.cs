using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help1 : HelpBase
{
    protected override IEnumerator Content()
    {
        HelpManager.instance.screenImage.enabled = false;

        yield return YieldDictionary.WaitForSeconds(0.5f);
        yield return Focusing(GameManager.instance.triggerBar.transform.position + Vector3.down, new Vector2(300f, 300f), "1.0", new Vector2(100f, 50f), wait:false);
        yield return new WaitWhile(() => GameManager.instance.SelectedTrigger == null && GameManager.instance.CurrentTriggers.Count == 1);
        yield return SetText("", new Vector2(100f, 50f));

        yield return Focusing(new Vector3(0.5f, 0.5f, 0), Auto, "1.1", new Vector2(80f, 40f), wait:false);
        yield return new WaitWhile(() => GameManager.instance.SelectedTrigger != null);
        yield return SetText("", new Vector2(100f, 50f));

        if (GameManager.instance.CurrentTiles[1][1].type == LevelBase.TileType.Normal)
            yield return Focusing(Vector3.zero, Vector3.zero, "1.2", Vector3.zero);

        yield return PrevDispose();
    }
}
