using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help0 : HelpBase
{
    protected override bool CheckCondition()
    {
        return DataManager.HasData("Game", "Help1-1");
    }

    protected override IEnumerator Content()
    {
        yield return Focusing(GameManager.instance.CurrentCars[0].transform.position, Auto, "1-1.0", new Vector2(100f, 50f));
        yield return Focusing(GameManager.instance.CurrentGoals[0].transform.position, Auto, "1-1.1", new Vector2(100f, 50f));
        yield return Focusing(GameManager.instance.playButton.transform.position, new Vector2(200f, 200f), "1-1.2", new Vector2(100f, 50f));
        yield return new WaitWhile(() => !GameManager.instance.IsPlaying);
        yield return PrevDispose();

        DataManager.SetData("Game", "Help1-1", 0);
        DataManager.SaveData();
    }
}
