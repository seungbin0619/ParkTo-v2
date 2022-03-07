using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Trigger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    private ScrollRect parentRect;
    private Image image;

    public LevelBase.TriggerType Type { private set; get; }
    public Sprite sprite;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Initialize(LevelBase.TriggerType type, ScrollRect parent)
    {
        Type = type;
        parentRect = parent;

        sprite = image.sprite = GameManager.instance.triggerImages[(int)type];
    }

    public void Select()
    {
        GameManager.instance.SelectTrigger(this);
    }

    #region [ 이벤트 전달 ]

    public void OnBeginDrag(PointerEventData e)
    {
        parentRect.OnBeginDrag(e);
    }
    public void OnDrag(PointerEventData e)
    {
        parentRect.OnDrag(e);
    }
    public void OnEndDrag(PointerEventData e)
    {
        parentRect.OnEndDrag(e);
    }

    public void OnScroll(PointerEventData e)
    {
        parentRect.OnScroll(e);
    }

    #endregion
}
