using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeImage : MonoBehaviour
{
    [SerializeField]
    private FadeManager targetManager;
    private UnityEngine.UI.Image image;

    private void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
    }

    public void FadeStart() {
        image.raycastTarget = true;
    }

    public void FadeOutEnd()
    {
        targetManager.currentAction.Complete();
        image.raycastTarget = false;
    }

    public void FadeInEnd() => targetManager.currentAction.Complete();

}
