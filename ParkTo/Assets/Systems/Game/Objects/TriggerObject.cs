using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerObject : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] sprites;
    private float[] alpha = null;

    [SerializeField]
    private bool mode;

    [SerializeField]
    private bool gen;

    private void OnEnable()
    {
        if (alpha != null) return;

        alpha = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            alpha[i] = sprites[i].color.a;
        }
    }

    public void OnSelectedTriggerStateChanged()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            SpriteRenderer sprite = sprites[i];

            Color col = sprite.color;
            col.a = alpha[i] * (GameManager.instance.TriggerSelectedMode == mode || gen ? 0.2f : 1f);

            sprite.color = col;
        }
    }

    public void OnUnselectTrigger()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            SpriteRenderer sprite = sprites[i];

            Color col = sprite.color;
            col.a = alpha[i];

            sprite.color = col;
        }
    }
}
