using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help4 : HelpBase
{
    protected override IEnumerator Content()
    {
        yield return Focusing(new Vector3(0f, 0f, 0f), Auto, "4.0", new Vector2(100f, 50f));
        yield return PrevDispose();
    }
}
