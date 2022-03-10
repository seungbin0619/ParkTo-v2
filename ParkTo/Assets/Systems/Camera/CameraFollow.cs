using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    public void Start()
    {
        OnCameraChange();
    }

    public void OnCameraChange()
    {
        canvas.worldCamera = CameraManager.currentCamera;
    }
}
