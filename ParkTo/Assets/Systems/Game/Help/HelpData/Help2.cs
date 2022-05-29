using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help2 : HelpBase
{
    protected override IEnumerator Content()
    {
        GameManager.instance.BarHide = GameManager.instance.triggerBar.Hide = true;
        HelpManager.instance.screenImage.enabled = false;

        yield return YieldDictionary.WaitForSeconds(0.5f);
        yield return Focusing(GameManager.instance.triggerBar.transform.position + Vector3.down, new Vector2(300f, 300f), "2.0", new Vector2(100f, 50f), wait: false);
        yield return new WaitWhile(() => GameManager.instance.SelectedTrigger == null && GameManager.instance.CurrentTriggers.Count == 1);
        yield return SetText("", new Vector2(100f, 50f));

        yield return Focusing(Vector3.zero, Vector2.zero, "2.1", Vector2.zero, wait: false);
        HelpManager.instance.Focusing = true;
        yield return new WaitWhile(() => !GameManager.instance.TriggerSelectedMode && GameManager.instance.SelectedTrigger != null);
        yield return SetText("", Vector2.zero);

        yield return Focusing(GameManager.instance.CurrentCars[0].transform.position, Auto, "2.2", new Vector2(100f, 50f), wait:false);
        yield return new WaitWhile(() => {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); mousePosition.z = 0;
                Vector3Int tilePosition = GameManager.instance.triggerTile.WorldToCell(mousePosition);

                HelpManager.instance.Focusing = 
                    !GameManager.instance.TriggerSelectedMode || 
                    tilePosition != new Vector3Int(0, 2, 0);
                    
                return GameManager.instance.SelectedTrigger != null;
            });
        yield return SetText("", new Vector2(100f, 50f));

        if (GameManager.instance.CurrentCars[0].Rotation != 2)
            yield return Focusing(Vector3.zero, Vector3.zero, "1.2", Vector3.zero);

        yield return PrevDispose();
    }
}
