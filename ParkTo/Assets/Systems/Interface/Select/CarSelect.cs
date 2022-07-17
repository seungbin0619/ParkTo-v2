using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSelect : MonoBehaviour
{
    private Vector3 targetScale;

    private void Awake() => targetScale = Vector3.one * 0.8f;

    private void OnMouseEnter() => targetScale = Vector3.one;

    private void OnMouseExit() => targetScale = Vector3.one * 0.8f;

    private void Update() => 
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * 10f);

    private void OnMouseDown() {
        SelectManager.instance.EnterGame();
    }
}
