using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpButton : UIButtonFocus
{
    protected override void Update()
    {
        bool flag = false;
        Vector3 scale = targetScale;
        if(GameManager.instance.CurrentLevel != null) {
            flag = GameManager.instance.CurrentLevel.decorates.Length > 0 && 
                GameManager.instance.CurrentLevel.decorates[0].decorate.tag == "Help";
            scale = flag ? scale : Vector3.zero;
        }
        
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scale, Time.deltaTime * (flag ? 5f : 10f));
    }
}
