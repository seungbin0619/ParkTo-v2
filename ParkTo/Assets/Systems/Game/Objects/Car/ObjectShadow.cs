using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShadow : MonoBehaviour
{
    private Vector3 shadowDistance;

    [SerializeField] private Transform parent;
    [SerializeField] private float distance = 0.1f;

    private readonly WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

    private bool disable = false;
    private Coroutine coroutine = null;

    private void Awake()
    {
        shadowDistance = new Vector3(0, -distance, 0);
    }

    private void OnEnable()
    {
        disable = true;
        coroutine = StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        disable = false;
        if (coroutine == null) return;

        StopCoroutine(coroutine);
        coroutine = null;
    }

    private IEnumerator Loop()
    {
        while(disable)
        {
            transform.position = parent.position + shadowDistance;

            yield return waitFrame;
        }
    }
}
