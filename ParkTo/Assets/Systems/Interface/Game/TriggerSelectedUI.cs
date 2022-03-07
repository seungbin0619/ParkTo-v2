using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerSelectedUI : MonoBehaviour
{
    private Image image;
    [SerializeField]
    private Canvas parent;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnSelectTrigger()
    {
        image.enabled = true;
    }

    public void OnUnselectTrigger()
    {
        image.enabled = false;
    }

    public void OnSelectedTriggerStateChange(object[] args)
    {
        parent.sortingOrder = (bool)args[0] ? 15 : -20;
    }
}
