using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notice : MonoBehaviour
{
    private const float SPEED = 0.075f;
    private static readonly Vector3 top = new Vector3(0, 108f, 0);
    private static readonly Vector3 bottom = new Vector3(0, -36f, 0);

    RectTransform box;
    [SerializeField]
    TMPro.TMP_Text body;

    [System.NonSerialized]
    public bool isPlaying = false;

    [System.NonSerialized]
    public bool isEnd = false;

    private string content;

    private bool follow = false;

    private void Awake() {
        box = GetComponent<RectTransform>();
    }
    
    public void SetText(string text) => content = text;
    public void ShowText() {
        isPlaying = true;

        StartCoroutine(Play());
    }

    IEnumerator Play() {
        float progress = 0f, duration = 0.5f, clamp;
        while(progress < duration) {
            yield return YieldDictionary.WaitForEndOfFrame;
            progress += Time.deltaTime;

            clamp = Mathf.Clamp(progress / duration, 0f, 1f);
            box.anchoredPosition = LineAnimation.Lerp(top, bottom, clamp, 0f, 1f);
        }

        string content = this.content;
        follow = true;

        body.text = content;
        yield return YieldDictionary.WaitForSeconds(3.5f);
        follow = false;

        progress = 0f; duration = 0.5f;
        Vector2 size = box.sizeDelta;
        
        while(progress < duration) {
            yield return YieldDictionary.WaitForEndOfFrame;
            progress += Time.deltaTime;

            clamp = Mathf.Clamp(progress / duration, 0f, 1f);
            
            box.sizeDelta = LineAnimation.Lerp(size, Vector2.one * 64f, clamp, 0.3f, 0.7f);
            box.anchoredPosition = LineAnimation.Lerp(bottom, top, clamp, 1f, 0f);
        }
        body.text = "";

        yield return YieldDictionary.WaitForSeconds(0.5f);
        isEnd = true;
    }

    private void LateUpdate()
    {
        if(!follow) return;

        Vector2 size = body.rectTransform.sizeDelta;
        box.sizeDelta = Vector2.Lerp(box.sizeDelta, size, Time.deltaTime * 10f);
    }
}
