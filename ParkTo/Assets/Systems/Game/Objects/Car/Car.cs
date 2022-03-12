using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    #region [ 기타 설정 ]

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
        public PathData(Vector3Int position, int rotation, bool backward)
        {
            this.position = position;
            this.rotation = rotation;
            this.backward = backward;
        }
    }


    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

    #endregion

    #region [ 차 상태 ]

    public Vector2Int Position { private set; get; }
    public int Rotation { private set; get; }
    public Color32 Color { private set; get; }
    public bool Collided { private set; get; }
    public bool Stopped { private set; get; }

    #endregion

    #region [ 경로 관련 ]

    private bool isTriggerStop = false;
    private bool isTriggerBakcward = false;

    private List<PathData> trace = new List<PathData>();
    private List<PathData> path = new List<PathData>();

    private int pathIndex;          // 현재 path상 위치
    private float currentProgress;  // 현재 진행 상태
    private float targetProgress;   // 다음 경로까지의 시간

    public bool IsMovable { get { return path.Count > 1; } } // 다음에 움직일 수 있는지

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

        transform.eulerAngles = new Vector3(0, 0, rotation * 90f);
        spriteRenderer.color = color;
    }

    #region [ 경로 ]

    public void InitPath()
    {
        pathIndex = 0;

        currentProgress = 0;
        targetProgress = 1;

        path = new List<PathData>();
        path.Add(new PathData((Vector3Int)Position, Rotation, isTriggerBakcward));

        Stopped = false;
    }

    public void GetNextPath()
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

        LevelBase.TileData tile = GameManager.instance.currentTiles[tmp.position.y][tmp.position.x];

        if (tile.type == LevelBase.TileType.Trigger) {
            switch ((LevelBase.TriggerType)tile.data)
            {
                case LevelBase.TriggerType.TURNLEFT:
                    tmp.rotation = Rotate(tmp.rotation, !tmp.backward ? 1 : -1);

                    break;
                case LevelBase.TriggerType.TURNRIGHT:
                    tmp.rotation = Rotate(tmp.rotation, !tmp.backward ? -1 : 1);

                    break;
                case LevelBase.TriggerType.STOP:
                    Stopped = true;

                    break;
                case LevelBase.TriggerType.BACKWARD:
                    tmp.backward = true;

                    break;
                default:
                    break;
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
    private PathData GetFront(PathData bef)
    {
        return new PathData(bef.position + direction[Rotate(bef.rotation, back: bef.backward)], bef.rotation, bef.backward);
    }

    #endregion

    #region [ 이동 ]

    public void PrevMove()
    {
        trace.Add(new PathData((Vector3Int)Position, Rotation, isTriggerBakcward));

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

        clamp = progress - currentProgress;

        cur = path[pathIndex];
        tile = GameManager.instance.currentTiles[cur.position.y][cur.position.x];

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

            if (pathIndex == path.Count - 1) // 감속
            {
                position += (clamp - 0.5f * Mathf.Pow(clamp, 2)) * (Vector3)direction[Rotate(bef.rotation, back: bef.backward)];

                switch ((LevelBase.TriggerType)tile.data) // 바닥에 따라 방향이 달라질 수 있음
                {
                    case LevelBase.TriggerType.TURNLEFT:
                    case LevelBase.TriggerType.TURNRIGHT:
                        Rotation = cur.rotation;

                        if (bef.rotation == 3 && cur.rotation == 0) cur.rotation = 4;
                        if (bef.rotation == 0 && cur.rotation == 3) bef.rotation = 4;

                        transform.eulerAngles = LineAnimation.Lerp(angles[bef.rotation], angles[cur.rotation], clamp, 0, 0.5f);

                        break;
                    default: // 직진
                        transform.eulerAngles = angles[bef.rotation];

                        break;
                }
            }
            else
            {
                switch ((LevelBase.TriggerType)tile.data) // 바닥에 따라 방향이 달라질 수 있음
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
                    default: // 직진
                        position += clamp * (Vector3)direction[Rotate(bef.rotation, back: bef.backward)];
                        transform.eulerAngles = angles[bef.rotation];

                        break;
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

    #region [ 트리거 ]

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

    private  void Update()
    {
        PreviewUpdate();
    }
}
