using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help0 : HelpBase
{
    protected override IEnumerator Content()
    {
        yield return SetFocus(GameManager.instance.CurrentCars[0].transform.position, Auto);
        yield return SetText("�̰� ������", new Vector2(100f, 50f));
    }
}
