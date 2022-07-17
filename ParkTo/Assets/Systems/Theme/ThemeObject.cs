using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeObject : MonoBehaviour
{
    [SerializeField]
    protected int index;
    protected MaskableGraphic graphic;

    protected virtual void Awake() => graphic = GetComponent<MaskableGraphic>();
    protected virtual void Start() => FollowTheme();

    protected virtual void FollowTheme()
    {
        graphic.color = ThemeManager.currentTheme.colors[index];
    }
}
