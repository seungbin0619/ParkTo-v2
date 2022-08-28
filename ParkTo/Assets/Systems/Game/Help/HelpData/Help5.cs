using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help5 : HelpBase
{
    protected override IEnumerator Content()
    {
        yield return Focusing(new Vector3(-2f, 0f, 0f), Auto, "5.0", new Vector2(100f, 50f));
        yield return Focusing(new Vector3(1f, 0f, 0f), Auto, "5.1", new Vector2(100f, 50f));
        yield return PrevDispose();
    }
}
