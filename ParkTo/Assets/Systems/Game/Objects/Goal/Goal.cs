using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private Car target = null;
    private Vector2Int position;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2Int position, Car target)
    {
        this.position = position;
        this.target = target;

        spriteRenderer.color = target.Color;
    }
}
