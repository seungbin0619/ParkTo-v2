using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpManager : SingleTon<HelpManager>
{
    [SerializeField]
    Animation screen;
    public Coffee.UIExtensions.UnmaskRaycastFilter screenImage { get; private set; }
    public Image Focus;

    [SerializeField]
    private RectTransform skipPanel;

    [SerializeField]
    private UnityEngine.UI.Image skipProgressBar;

    List<AnimationClip> fadeClips;
    public static bool IsInitialize { private set; get; }

    public TMPro.TMP_Text content;
    public RectTransform Mask;
    public RectTransform Descript;

    private HelpBase help;
    private float skipProgress;
    private bool skipped = false;
    private const float SKIP_TIME = 1f;

    private bool Focused { get; set; }
    public bool Focusing { get; set; }

    private bool appQuit = false;

    protected override void Awake()
    {
        base.Awake();

        screenImage = screen.GetComponent<Coffee.UIExtensions.UnmaskRaycastFilter>();
        fadeClips = new List<AnimationClip>();

        fadeClips.Add(screen["FadeIn80"].clip);
        fadeClips.Add(screen["FadeOut80"].clip);
    }

    public void Initialize(HelpBase help)
    {
        if (IsInitialize) return;
        IsInitialize = true;

        Mask.gameObject.SetActive(true);
        Descript.gameObject.SetActive(true);

        content.text = "";

        screen.clip = fadeClips[0];
        screen.Play();

        this.help = help;
        skipped = false;
    }

    private void OnApplicationQuit() => appQuit = true;

    public void Dispose()
    {
        if (appQuit) return;
        if (!IsInitialize) return;
        IsInitialize = false;

        Descript.gameObject.SetActive(false);

        content.text = "";

        screen.clip = fadeClips[1];
        screen.Play();

        Focusing = false;

        help = null;
    }

    public IEnumerator SetFocus(Vector3 position, Vector2 size, bool notFocus = false)
    {
        float progress = 0, duration = 0.5f;
        Vector2 before;

        if (Focused)
        {
            before = Focus.rectTransform.sizeDelta;
            while(duration > progress)
            {
                Focus.rectTransform.sizeDelta = LineAnimation.Lerp(before, Vector2.zero, progress / duration, 0.5f, 0);

                yield return YieldDictionary.WaitForEndOfFrame;
                progress += Time.deltaTime;
            }
            progress = 0;
            Focus.rectTransform.sizeDelta = Vector3.zero;
            Focused = false;
        }

        if (notFocus) yield break;

        before = Focus.rectTransform.sizeDelta;
        Focus.rectTransform.anchoredPosition = ConvertPosition(position);

        while (duration > progress)
        {
            Focus.rectTransform.sizeDelta = LineAnimation.Lerp(before, size, progress / duration, 0, 0.5f);

            yield return YieldDictionary.WaitForEndOfFrame;
            progress += Time.deltaTime;
        }
        Focus.rectTransform.sizeDelta = size;
        Focused = true;

        yield return null;
    }

    private Vector3 ConvertPosition(Vector3 position)
    {
        Vector3 ViewportPosition = Camera.main.WorldToViewportPoint(position);
        Vector3 WorldObject_ScreenPosition = new Vector3(
            (ViewportPosition.x * Mask.rect.width) - (Mask.rect.width * 0.5f),
            (ViewportPosition.y * Mask.rect.height) - (Mask.rect.height * 0.5f));

        return WorldObject_ScreenPosition;
    }

    private Vector2 lastDelta;
    public IEnumerator SetText(string text, Vector2 delta)
    {
        int progress = 0;
        content.text = "";
        Descript.anchoredPosition = Focus.rectTransform.anchoredPosition + delta;
        lastDelta = delta;

        while (progress < text.Length)
        {
            content.text += text[progress];
            yield return YieldDictionary.WaitForSeconds(0.075f);

            progress++;
        }

        content.text = text;
    }

    public IEnumerator PrevDispose()
    {
        yield return SetText("", lastDelta);
        yield return SetFocus(Vector3.zero, Vector2.zero, true);

        Dispose();
    }
    
    private void Update() {
        skipPanel.anchoredPosition = Vector2.Lerp(
            skipPanel.anchoredPosition, 
            Vector2.up * 72f * (IsInitialize && !skipped ? -1 : 1), 
            Time.deltaTime * 5f);

        if(!IsInitialize) return;
        if(skipped) return;
        
        if(!Input.GetKey(KeyCode.S)) {
            skipProgress = 0;
            skipProgressBar.fillAmount = 0;
            
            return;
        }

        skipProgress += Time.deltaTime;
        skipProgressBar.fillAmount = skipProgress / SKIP_TIME;

        if(skipProgress < SKIP_TIME) return;

        skipProgress = 0;
        skipped = true;
        // 스킵
        StopCoroutine(help.current);
        help.Save();
        
        StartCoroutine(PrevDispose());
    }
}
