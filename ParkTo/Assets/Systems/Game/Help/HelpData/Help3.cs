using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help3 : HelpBase
{
    protected override IEnumerator Content()
    {
        yield return Focusing(new Vector3(0, -1f, 0f), Auto, "3.0", new Vector2(100f, 50f));
        yield return PrevDispose();
    }
}
