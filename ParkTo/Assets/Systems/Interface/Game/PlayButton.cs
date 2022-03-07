using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButton : MonoBehaviour
{
    private UnityEngine.UI.Button button;

    private Vector3 targetScale = Vector3.zero;
    private Vector3 beforeScale = Vector3.zero;

    private bool condition = true;

    public bool Hide { 
        set 
        {
            if (condition == value) return;
            condition = value;

            progress = 0;
            button.enabled = !value;

            beforeScale = transform.localScale;
            targetScale = Vector3.one * (condition ? 0 : 1); 
        } 
    }

    private float progress = 0;

    private void Awake()
    {
        button = GetComponent<UnityEngine.UI.Button>();
    }

    private void Update()
    {
        float tmpProgress = Mathf.Clamp(progress, 0f, 1f);
        if (tmpProgress == 1f) return;

        transform.localScale = LineAnimation.Lerp(beforeScale, targetScale, tmpProgress, 0.5f, 0.5f);
        progress += Time.deltaTime * 2f;
    }
}
