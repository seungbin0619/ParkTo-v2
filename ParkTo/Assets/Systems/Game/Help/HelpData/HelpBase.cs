using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class HelpBase : MonoBehaviour
{
    public Coroutine current;
    protected static readonly Vector2 Auto = Vector2.one * 160f;
    protected static bool InputFlag { private set; get; }
    protected bool Skipped = false;

    protected WaitWhile Wait = new WaitWhile(() => !InputFlag);

    [SerializeField]
    protected string id;

    protected virtual void Awake() => Play();

    private void OnDestroy()
    {
        if (current == null) return;
        StopCoroutine(current);
    }

    protected bool CheckCondition() => DataManager.GetData("Game", id, 0) == 0;
    public void Reset() => DataManager.SetData("Game", id, 0);

    protected void Play() {
        if (!CheckCondition()) return;
        HelpManager.instance.Initialize(this);
        current = StartCoroutine(Content());
    }

    protected virtual IEnumerator Content()
    {
        yield return null;
    }

    protected IEnumerator SetFocus(Vector3 position, Vector2 size, bool flag = false) => HelpManager.instance.SetFocus(position, size, flag);
    protected IEnumerator SetText(string text, Vector2 delta) => HelpManager.instance.SetText(text, delta);
    protected string LocaleText(string data) => LocalizationSettings.StringDatabase.GetLocalizedString("Help", data);

    protected IEnumerator Focusing(Vector3 position, Vector2 size, string data, Vector3 delta, bool wait = true)
    {
        HelpManager.instance.Focusing = true;
        HelpManager.instance.screenImage.enabled = false;

        yield return SetFocus(position, size);
        yield return SetText(LocaleText(data), delta);
        HelpManager.instance.Focusing = false;

        HelpManager.instance.screenImage.enabled = true;

        if (wait)
        {
            yield return Wait;
            yield return SetText("", delta);
        }
    }

    protected IEnumerator PrevDispose()
    {
        DataManager.SetData("Game", id, 1);
        DataManager.SaveData();

        yield return HelpManager.instance.PrevDispose();
    }

    private void Update()
    {
        InputFlag = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
    }
}
