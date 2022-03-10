using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private Car target = null;
    private Vector2Int position;

    private Vector3 targetScale = Vector3.zero;
    private Vector3 beforeScale = Vector3.zero;

    private bool condition = false;

    public bool IsArrived
    {
        set
        {
            if (condition == value) return;
            condition = value;

            progress = 0;

            beforeScale = transform.localScale;
            targetScale = Vector3.one * (condition ? 0 : 1);
        }
        get
        {
            return condition;
        }
    }

    private SpriteRenderer spriteRenderer;
    private float progress = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2Int position, Car target)
    {
        this.position = position;
        this.target = target;

        spriteRenderer.color = target.Color;
        beforeScale = targetScale = Vector3.one;
    }
    private void Update()
    {
        if (target == null) return;

        IsArrived = !target.Collided && target.Position == position;

        float tmpProgress = Mathf.Clamp(progress, 0f, 1f);
        transform.localScale = LineAnimation.Lerp(beforeScale, targetScale, tmpProgress, 0.5f, 0.5f);

        progress += Time.deltaTime * 2f;
    }
}
