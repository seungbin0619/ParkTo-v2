using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public struct PathData
    {
        public Vector2Int position;
        public int rotation;
        public bool backward;
        public PathData(Vector2Int position, int rotation, bool backward)
        {
            this.position = position;
            this.rotation = rotation;
            this.backward = backward;
        }
    }

    private const int MAX_COUNT = 100;

    public Vector2Int Position { private set; get; }
    public int Rotation { private set; get; }
    public Color32 Color { private set; get; }

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2Int position, int rotation, Color color)
    {
        Position = position;
        Rotation = rotation;
        Color = color;

        transform.eulerAngles = new Vector3(0, 0, rotation * 90f);
        spriteRenderer.color = color;
    }
}
