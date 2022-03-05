using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Trigger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    private ScrollRect parentRect;

    public void Initialize()
    {

    }

    public void Select()
    {
        
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
