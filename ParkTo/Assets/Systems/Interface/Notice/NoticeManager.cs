using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoticeManager : SingleTon<NoticeManager>
{
    [SerializeField]
    Notice noticePrefab;
    RectTransform canvas;
    Queue<Notice> notices;

    public bool IsPlaying { get => notices.Count > 0; }

    protected override void Awake() {
        base.Awake();

        canvas = GetComponent<RectTransform>();
        notices = new Queue<Notice>();
    }

    public void NoticeString(string content) {
        Notice notice = Instantiate(noticePrefab, canvas);
        notice.SetText(content);

        notices.Enqueue(notice);
    }

    private void Update() {
        if(notices.Count == 0) return;
        Notice notice = notices.Peek();

        if(!ActionManager.IsPlaying && !notice.isPlaying) notice.ShowText();
        else if(notice.isEnd) {
            notices.Dequeue();

            Destroy(notice);
        }
    }
}
