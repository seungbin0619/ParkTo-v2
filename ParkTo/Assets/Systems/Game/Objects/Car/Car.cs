using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    #region [ ��Ÿ ���� ]

    private const int MAX_COUNT = 100;
    private const float fixedDuration = 0.3f;

    private static readonly Vector3Int[] direction = new Vector3Int[4]
    {
        Vector3Int.up,
        Vector3Int.left,
        Vector3Int.down,
        Vector3Int.right
    };

    private static readonly Vector3[] angles = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 90f),
        new Vector3(0, 0, 180f),
        new Vector3(0, 0, 270f),
        new Vector3(0, 0, 360f)
    };

    public struct PathData
    {
        public Vector3Int position;
        public int rotation;
        public bool backward;
        public bool stopped;
        public int ispeed; // 속도의 역수
        
        public PathData(Vector3Int position, int rotation, bool backward = false, bool stopped = false, int ispeed = 1)
        {
            this.position = position;
            this.rotation = rotation;
            this.backward = backward;
            this.stopped = stopped;
            this.ispeed = ispeed;
        }
    }

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

    #endregion

    #region [ �� ���� ]

    public Vector2Int Position { private set; get; }
    public int Rotation { private set; get; }
    public Color32 Color { private set; get; }
    public bool Collided { private set; get; }
    //public bool Stopped => path[path.Count - 1].stopped; // 정지했는데 다시 가는 경우는 없으니?
    public bool Stopped {private set; get; }

    #endregion

    #region [ ��� ���� ]

    private bool isTriggerStop = false;
    private bool isTriggerBakcward = false;

    private List<PathData> trace = new List<PathData>();
    public List<PathData> path = new List<PathData>();

    private int pathIndex;          // ���� path�� ��ġ
    private float currentProgress;  // ���� ���� ����
    private float targetProgress;   // ���� ��α����� �ð�

    public bool IsMovable { get { return path.Count > 1; } } // ������ ������ �� �ִ���

    #endregion

    private Vector3 targetScale = Vector3.one * 0.8f;

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

        transform.localPosition = (Vector3Int)position;
        transform.eulerAngles = new Vector3(0, 0, rotation * 90f);
        spriteRenderer.color = color;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collided = true;
        Vector2 hitPoint = collision.contacts[0].point;
        //MapSystem.instance.AddCollision(hitPoint);
    }

    #region [ ��� ]

    public void InitPath()
    {
        pathIndex = 0;

        currentProgress = 0;
        targetProgress = 1;

        //
        //isTriggerBakcward = false;
        //isTriggerStop = false;
        //

        Stopped = false;
        path = new List<PathData>();
        path.Add(new PathData((Vector3Int)Position, Rotation, isTriggerBakcward, isTriggerStop));
    }

    public void GetNextPath() // 문제점: stopped에 언제 멈췄는지에 대한 정보가 없다.
    {
        if (isTriggerStop)
        {
            Stopped = true;
            return;
        }
        if (Stopped) return;

        PathData tmp = GetFront(path[path.Count - 1]);
        if (!GameManager.instance.IsValidPosition(tmp.position))
        {
            Stopped = true;
            return;
        }

        LevelBase.TileData tile = GameManager.instance.CurrentTiles[tmp.position.y][tmp.position.x];

        if (tile.type == LevelBase.TileType.Trigger) {
            switch ((LevelBase.TriggerType)tile.data)
            {
                case LevelBase.TriggerType.TURNLEFT: tmp.rotation = Rotate(tmp.rotation, !tmp.backward ? 1 : -1); break;
                case LevelBase.TriggerType.TURNRIGHT: tmp.rotation = Rotate(tmp.rotation, !tmp.backward ? -1 : 1); break;
                case LevelBase.TriggerType.STOP: Stopped = true; break;
                case LevelBase.TriggerType.BACKWARD: tmp.backward = true; break;
                default: break;
            }
        }

        for (int i = 0; i < GameManager.instance.CurrentCars.Count; i++)
        {
            Car dif = GameManager.instance.CurrentCars[i];
            PathData difPos = dif.path[Mathf.Min(path.Count, dif.path.Count) - 1];

            if (dif == this) continue;
            if (difPos.position != tmp.position) continue;
            if (GameManager.instance.IsValidPosition(GetFront(difPos).position) && (path.Count < dif.path.Count || !dif.Stopped)) continue;

            Stopped = true;
            return;
        }

        path.Add(tmp);
        if (path.Count > MAX_COUNT) Stopped = true;
    }

    private PathData GetFront(PathData bef) => 
        new PathData(bef.position + direction[Rotate(bef.rotation, back: bef.backward)],
                     bef.rotation,
                     bef.backward,
                     bef.stopped);

    #endregion

    #region [ �̵� ]

    public void PrevMove()
    {
        trace.Add(new PathData((Vector3Int)Position, Rotation, isTriggerBakcward, isTriggerStop));

        isTriggerStop = false;
        isTriggerBakcward = false;
    }

    public void OnMove()
    {
        if (Collided) return;

        transform.localPosition = (Vector3Int)Position;
        transform.eulerAngles = angles[Rotate(Rotation)];
    }

    public bool MoveTo(float progress)
    {
        if (Collided) return false;
        if (!IsMovable) return false;

        Vector3 position;
        PathData cur, bef;

        LevelBase.TileData tile;
        float clamp;

        progress /= fixedDuration;
        if(progress > targetProgress)
        {
            pathIndex++;
            if (pathIndex >= path.Count) return false;

            currentProgress = targetProgress;
            targetProgress += 1f;
        }

        clamp = (progress - currentProgress) / (targetProgress - currentProgress);

        cur = path[pathIndex];
        tile = GameManager.instance.CurrentTiles[cur.position.y][cur.position.x];

        if (pathIndex == 0)
        {
            position = cur.position;
            position += 0.5f * Mathf.Pow(clamp, 2) * (Vector3)direction[Rotate(cur.rotation, back: cur.backward)];
        }
        else
        {
            bef = path[pathIndex - 1];
            position = bef.position + cur.position;
            position *= 0.5f;

            if (pathIndex == path.Count - 1) // 정지 직전
            {
                position += (clamp - 0.5f * Mathf.Pow(clamp, 2)) * (Vector3)direction[Rotate(bef.rotation, back: bef.backward)];

                if (tile.type == LevelBase.TileType.Trigger)
                {
                    switch ((LevelBase.TriggerType)tile.data)
                    {
                        case LevelBase.TriggerType.TURNLEFT:
                        case LevelBase.TriggerType.TURNRIGHT:
                            Rotation = cur.rotation;

                            if (bef.rotation == 3 && cur.rotation == 0) cur.rotation = 4;
                            if (bef.rotation == 0 && cur.rotation == 3) bef.rotation = 4;

                            transform.eulerAngles = LineAnimation.Lerp(angles[bef.rotation], angles[cur.rotation], clamp, 0, 0.5f);
                            break;
                        case LevelBase.TriggerType.SLOW:
                        default:
                            transform.eulerAngles = angles[bef.rotation];

                            break;
                    }
                }else transform.eulerAngles = angles[bef.rotation];
            }
            else // 평소 움직일 때
            {
                if (tile.type == LevelBase.TileType.Trigger)
                {
                    switch ((LevelBase.TriggerType)tile.data)
                    {
                        case LevelBase.TriggerType.TURNLEFT:
                            position = GetTurnPosition(bef, cur, position, clamp, !bef.backward ? 1 : -1);

                            break;
                        case LevelBase.TriggerType.TURNRIGHT:
                            position = GetTurnPosition(bef, cur, position, clamp, !bef.backward ? -1 : 1);

                            break;
                        case LevelBase.TriggerType.BACKWARD:
                            if (bef.backward != cur.backward)
                                position += (clamp - Mathf.Pow(clamp, 2)) * (Vector3)direction[bef.rotation];
                            else position += clamp * (Vector3)direction[Rotate(bef.rotation, back: bef.backward)];

                            transform.eulerAngles = angles[bef.rotation];
                            break;
                        case LevelBase.TriggerType.SLOW:
                        default:
                            position += clamp * (Vector3)direction[Rotate(bef.rotation, back: bef.backward)];
                            transform.eulerAngles = angles[bef.rotation];

                            break;
                    }
                }else
                {
                    position += clamp * (Vector3)direction[Rotate(bef.rotation, back: bef.backward)];
                    transform.eulerAngles = angles[bef.rotation];
                }
            }
        }

        transform.localPosition = position;
        Position = Vector2Int.RoundToInt(position);

        return true;
    }
    private Vector3 GetTurnPosition(PathData bef, PathData cur, Vector3 position, float clamp, int dir)
    {
        Vector3 nxt = cur.position + (Vector3)direction[Rotate(cur.rotation, back: cur.backward)] * 0.5f;
        Rotation = cur.rotation;

        if (bef.rotation == 3 && cur.rotation == 0) cur.rotation = 4;
        if (bef.rotation == 0 && cur.rotation == 3) bef.rotation = 4;

        transform.eulerAngles = Vector3.Lerp(angles[bef.rotation], angles[cur.rotation], clamp);

        Vector3 center = new Vector3();
        float A = Mathf.PI;

        if ((position.y - nxt.y) / (position.x - nxt.x) * dir > 0)
        {
            center.x = position.x;
            center.y = nxt.y;

            A *= center.y < position.y ? 0.5f : 1.5f;
        }
        else
        {
            center.x = nxt.x;
            center.y = position.y;

            A *= center.x < position.x ? 0f : 1f;
        }

        float angle = A + 0.5f * Mathf.PI * dir * clamp;
        position.x = center.x + Mathf.Cos(angle) * 0.5f;
        position.y = center.y + Mathf.Sin(angle) * 0.5f;

        return position;
    }

    #endregion

    private int Rotate(int rotation, int delta = 0, bool back = false)
    {
        rotation += delta;
        if (rotation < 0) rotation += 4;
        if (back) rotation += 2;

        rotation %= 4;

        return rotation;
    }

    #region [ Ʈ���� ]

    public void SetTrigger(LevelBase.TriggerType triggerType, bool undo = false)
    {
        switch (triggerType)
        {
            case LevelBase.TriggerType.TURNLEFT:
                Rotation = Rotate(Rotation, undo ? -1 : 1);

                break;
            case LevelBase.TriggerType.TURNRIGHT:
                Rotation = Rotate(Rotation, undo ? 1 : -1);

                break;
            case LevelBase.TriggerType.STOP:
                isTriggerStop = !undo;

                break;
            case LevelBase.TriggerType.BACKWARD:
                isTriggerBakcward = !undo;

                break;
            default: break;
        }

        Rotation %= 4;
        transform.eulerAngles = angles[Rotation];
    }

    private void PreviewUpdate()
    {
        if (GameManager.instance.IsPlaying) return;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);
    }

    public void PreviewTrigger(float size)
    {
        targetScale = Vector3.one * size;
    }

    #endregion


    public void Undo()
    {
        if (trace.Count == 0) return;
        PathData pathData = trace[trace.Count - 1];

        Position = (Vector2Int)pathData.position;
        Rotation = pathData.rotation;

        transform.localPosition = pathData.position;
        transform.eulerAngles = angles[Rotate(Rotation)];

        if(Collided)
        {
            Collided = false;

            //
        }

        trace.RemoveAt(trace.Count - 1);
    }

    private void OnMouseEnter() => GameManager.instance.predictCar = this;

    private void OnMouseExit() => GameManager.instance.predictCar = null;

    private  void Update()
    {
        PreviewUpdate();
    }
}
