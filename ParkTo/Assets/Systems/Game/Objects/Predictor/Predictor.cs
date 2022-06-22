using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predictor : MonoBehaviour
{
    private const float period = 1.5f;
    private const float viewCount = 4;

    private float initialTime;
    private float loopDuration;
    public Car target { private set; get; }
    private Color color;
    private float index = -1;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Car target, float index, Vector3 position, float initialTime, float maxPathProgress, bool middle = false)
    {
        this.initialTime = initialTime;
        loopDuration = maxPathProgress / viewCount + period;

        this.target = target;
        color = target.Color;
        color.a = 0; spriteRenderer.color = color;

        transform.localPosition = position;
        if (middle) transform.localScale *= 0.7f;

        this.index = index / viewCount;
    }

    void Update()
    {
        if(!GameManager.instance.IsPlayable ||
            GameManager.instance.IsPlaying || 
            GameManager.instance.SelectedTrigger != null || 
            (GameManager.instance.predictCar != target && GameManager.instance.predictCar != null))
        {
            color.a = 0;
            spriteRenderer.color = color;
            return;
        }

        if (index == -1) return;

        float deltaTime = (Time.time - initialTime) % loopDuration;
        // 1 ~ 4 -> �ִ� index = 3.5 / 3 = 1.16666... 1.1666.. + period�� ����?

        color.a = (deltaTime >= index && deltaTime < index + period) ? Mathf.Cos((deltaTime - index) * Mathf.PI * 2 / period) * -0.5f + 0.5f : 0;
        spriteRenderer.color = color;
    }
}
