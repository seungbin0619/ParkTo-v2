using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBar : MonoBehaviour
{
    private readonly Vector2[] barPosition = new Vector2[2]
    {
        new Vector2(0, 40f), new Vector2(0, 160f)
    };

    private RectTransform rectTransform;

    private Vector2 targetPosition;
    private Vector3 targetSizeDelta;

    public bool Hide { set { targetPosition = barPosition[value ? 0 : 1]; } }
    public int Size { set { targetSizeDelta = new Vector2(Mathf.Clamp(value, 1, 3.5f) * 200 + 40, 240); } }

    public float Position => rectTransform.position.y;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPosition = barPosition[1];
        targetSizeDelta = rectTransform.sizeDelta;
    }

    private void FixedUpdate()
    {
        rectTransform.sizeDelta = Vector3.Lerp(rectTransform.sizeDelta, targetSizeDelta, Time.fixedDeltaTime * 5f);
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.fixedDeltaTime * 5f);
    }
}
