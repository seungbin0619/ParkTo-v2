using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUpdate : MonoBehaviour
{
    private void Start()
    {
        CameraManager.currentCamera = GetComponent<Camera>();
        EventManager.instance.OnCameraChange.Raise();
    }
}
