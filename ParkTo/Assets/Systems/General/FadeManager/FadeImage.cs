using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeImage : MonoBehaviour
{
    [SerializeField]
    private FadeManager targetManager;

    public void FadeEnd()
    {
        targetManager.currentAction.Complete();
    }
}
