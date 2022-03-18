using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpBase : MonoBehaviour
{
    private Coroutine current;
    protected static readonly Vector2 Auto = Vector2.one * 160f;

    private void Awake()
    {
        HelpManager.instance.Initialize();
        current = StartCoroutine(Content());
    }

    private void OnDestroy()
    {
        HelpManager.instance.Dispose();

        if (current == null) return;
        StopCoroutine(current);
    }

    protected virtual IEnumerator Content()
    {
        yield return null;
    }

    protected IEnumerator SetFocus(Vector3 position, Vector2 size) => HelpManager.instance.SetFocus(position, size);
    protected IEnumerator SetText(string text, Vector2 delta) => HelpManager.instance.SetText(text, delta);
}
