using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FadeManager : ActionBase
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private Animation animations;
    private readonly float duration = 0.5f;

    private Dictionary<string, AnimationClip> clips;
    public static UnityEvent fadeEvent;

    private void Awake()
    {
        DontDestroyOnLoad(canvas.gameObject);

        clips = new Dictionary<string, AnimationClip>();

        clips.Add("FadeIn", animations.GetClip("FadeIn"));
        clips.Add("FadeOut", animations.GetClip("FadeOut"));
    }

    public void Fade(Action action, object[] args)
    {
        currentAction = action;

        float duration = float.Parse(args[0].ToString());
        Fade(action.id, duration);
    }

    public void Fade(string name, float duration = 0.5f)
    {
        if (!clips.ContainsKey(name)) return;

        animations.clip = clips[name];
        animations[name].speed = this.duration / duration;

        animations.Play();
    }
}
