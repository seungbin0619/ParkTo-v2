using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    public int index;
    private bool clicked = false;

    public void Select(int index) => SelectManager.instance.SelectIndex(index);

    private void OnMouseEnter() => Select(index);


    private void OnMouseDown() {
        if(clicked) return;
        clicked = true;

        SelectManager.instance.EnterGame();
    }

    private void OnMouseUp() {
        clicked = false;
    }
}
