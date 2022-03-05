using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonFocus : UpdateCoroutine<UIButtonFocus>
{
    private RectTransform rectTransform;
    private Vector3 targetScale = Vector3.one;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void TriggerPointerEnter(BaseEventData e)
    {
        targetScale = Vector3.one * 1.2f;
    }

    public void TriggerPointerExit(BaseEventData e)
    {
        targetScale = Vector3.one;
    }

    protected override void Routine()
    {
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10f);
    }
}
