using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpSelect : HelpBase
{
    protected override void Awake() {}
    
    //private void OnEnable() {
    //    base.Awake();    
    //}
    
    public void PlayHelp() {
        Reset();
        Play();
    }

    protected override IEnumerator Content()
    {
        yield return Focusing(Vector3.zero, Vector3.zero, "s.0", new Vector2(0f, -72f));
        yield return Focusing(Vector3.zero, Vector3.zero, "s.1", new Vector2(0f, -72f));
        yield return Focusing(SelectManager.instance.car.transform.position, Auto, "s.3", new Vector2(100f, 50f));
        yield return PrevDispose();
    }
}
