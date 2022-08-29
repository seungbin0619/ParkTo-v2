using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    public int index;
    private bool clicked = false;
    private bool entered = false;
    private Vector3 mousePosition;

    private UnityEngine.UI.Button button = null;

    public void Select(int index) => SelectManager.instance.SelectIndex(index);

    private void OnMouseOver() {
        if(!(button ??= GetComponent<UnityEngine.UI.Button>()).interactable) return;
        if(!entered) {
            mousePosition = Input.mousePosition;
            entered = true;

            return;
        }
        if(mousePosition == Input.mousePosition) return;
        mousePosition = Input.mousePosition;
        
        Select(index);
    }

    private void OnMouseExit() {
        entered = false;
    }


    private void OnMouseDown() {
        if(!(button ??= GetComponent<UnityEngine.UI.Button>()).interactable) return;
        if(clicked) return;

        clicked = true;
        SelectManager.instance.EnterGame();
    }

    private void OnMouseUp() {
        clicked = false;
    }
}
