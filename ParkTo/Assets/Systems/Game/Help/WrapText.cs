using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WrapText : MonoBehaviour
{
    [SerializeField]
    RectTransform targetFit;

    RectTransform Rect => GetComponent<RectTransform>();

    private void LateUpdate()
    {
        Vector2 size = targetFit.sizeDelta;
        Rect.sizeDelta = Vector2.Lerp(Rect.sizeDelta, size, Time.deltaTime * 15f);
    }
}
