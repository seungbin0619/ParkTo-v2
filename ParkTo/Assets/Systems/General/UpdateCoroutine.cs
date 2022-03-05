using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateCoroutine<T> : MonoBehaviour where T : MonoBehaviour
{
    private static readonly WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

    private bool disable = false;
    protected bool loop = true;

    private Coroutine coroutine = null;

    protected virtual void OnEnable()
    {
        disable = true;
        coroutine = StartCoroutine(Loop());
    }

    protected virtual void OnDisable()
    {
        disable = false;
        if (coroutine == null) return;

        StopCoroutine(coroutine);
        coroutine = null;
    }

    private IEnumerator Loop()
    {
        while (disable)
        {
            if(loop) Routine();
            yield return waitFrame;
        }
    }

    protected virtual void Routine() { }
}
