using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class HelpBase : MonoBehaviour
{
    private Coroutine current;
    protected static readonly Vector2 Auto = Vector2.one * 160f;
    protected static bool InputFlag { private set; get; }
    protected WaitWhile Wait = new WaitWhile(() => !InputFlag);

    private void Awake()
    {
        if (CheckCondition()) return;
        HelpManager.instance.Initialize();
        current = StartCoroutine(Content());
    }

    private void OnDestroy()
    {
        HelpManager.instance.Dispose();

        if (current == null) return;
        StopCoroutine(current);
    }

    protected virtual bool CheckCondition() { return false; }
    protected virtual IEnumerator Content()
    {
        yield return null;
    }

    protected IEnumerator SetFocus(Vector3 position, Vector2 size, bool flag = false) => HelpManager.instance.SetFocus(position, size, flag);
    protected IEnumerator SetText(string text, Vector2 delta) => HelpManager.instance.SetText(text, delta);
    protected string LocaleText(string data) => LocalizationSettings.StringDatabase.GetLocalizedString("Help", data);

    protected IEnumerator Focusing(Vector3 position, Vector2 size, string data, Vector3 delta)
    {
        yield return SetFocus(position, size);
        yield return SetText(LocaleText(data), delta);
        yield return Wait;
        yield return SetText("", new Vector2(100f, 50f));
    }

    protected IEnumerator PrevDispose() => HelpManager.instance.PrevDispose();

    private void Update()
    {
        InputFlag = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
    }
}
