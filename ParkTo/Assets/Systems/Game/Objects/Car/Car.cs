using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    #region [ 변수들 ]
    private const int MAX_COUNT = 100;
    private const float fixedSpeed = 0.3f;

    private static readonly Vector3Int[] carDirection = new Vector3Int[4]
    {
        Vector3Int.up,
        Vector3Int.left,
        Vector3Int.down,
        Vector3Int.right
    };

    private static readonly Vector3[] carAngles = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 90f),
        new Vector3(0, 0, 180f),
        new Vector3(0, 0, 270f),
        new Vector3(0, 0, 360f)
    };

    public class PathData
    {
        public Vector3Int Position { get; set; }
        public int Rotation { get; set; }
        public bool IsBackward { get; set; }
        public bool IsStopped { get; set; }
        public int ISpeed { get; set; } // 속도의 역수

        public bool IsCollided { get; set; }
        
        public PathData(Vector3Int Position, int Rotation, bool Backward = false, bool Stopped = false, int ISpeed = 1, bool Collided = false)
        {
            this.Position = Position;
            this.Rotation = Rotation;
            this.IsBackward = Backward;
            this.IsStopped = Stopped;
            this.ISpeed = ISpeed;
            this.IsCollided = Collided;
        }
    }

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

    public Vector2Int Position { private set; get; }
    public int Rotation { private set; get; }
    public int ISpeed { private set; get; }

    public Color32 Color { private set; get; }
    public bool Collided { private set; get; }

    public bool Stopped {private set; get; }
    public bool Stopped2 => path[pathCount - 1].IsStopped;

    private bool isTriggerStop = false;
    private bool isTriggerBakcward = false;

    private List<PathData> trace = new List<PathData>();
    public List<PathData> path = new List<PathData>();
    public List<float> timePath = new List<float>();
    public int pathCount { get; set; }

    private int pathIndex;
    private float currentProgress;
    private float targetProgress;

    public bool IsMovable { get { return pathCount > 1; } }

    private Vector3 targetScale = Vector3.one * 0.8f;

    #endregion

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2Int position, int rotation, Color color)
    {
        Position = position;
        Rotation = rotation;
        ISpeed = 1;

        Color = color;

        transform.localPosition = (Vector3Int)position;
        transform.eulerAngles = new Vector3(0, 0, rotation * 90f);
        spriteRenderer.color = color;
    }

    public void InitPath()
    {
        pathIndex = 0;
        currentProgress = 0;
        targetProgress = ISpeed;

        Stopped = false;
        
        // 초기 경로 (다른 차들의 경로는 무시.) - 사실상 움직일 수 있는 가장 긴 거리
        path = new List<PathData>();
        path.Add(new PathData((Vector3Int)Position, Rotation, isTriggerBakcward, isTriggerStop, ISpeed));

        timePath = new List<float>();
        timePath.Add(path[0].ISpeed);
        
        // 무한 루프에 대한 처리는 여기서 하기.
        PathData tmp = path[0];
        while(!tmp.IsStopped) {
            tmp = GetFront(tmp);
            if(!GameManager.instance.IsValidPosition(tmp.Position)) {
                path[path.Count - 1].IsStopped = true;
                break;
            }

            switch (GetTriggerType(tmp))
            {
                case LevelBase.TriggerType.TURNLEFT: tmp.Rotation = Rotate(tmp.Rotation, !tmp.IsBackward ? 1 : -1); break;
                case LevelBase.TriggerType.TURNRIGHT: tmp.Rotation = Rotate(tmp.Rotation, !tmp.IsBackward ? -1 : 1); break;
                case LevelBase.TriggerType.STOP: tmp.IsStopped = true; break;
                case LevelBase.TriggerType.BACKWARD: tmp.IsBackward = true; break;
                case LevelBase.TriggerType.SLOW: tmp.ISpeed = 2; break;
                default: break;
            }
            
            // 가져올때 계산하는게 문제라면 처음부터 다 계산해서 구해버리면 되지 않을까.
            timePath.Add(path[path.Count - 1].ISpeed != tmp.ISpeed ? 1.5f : tmp.ISpeed);
            path.Add(tmp);

            if(path.Count >= MAX_COUNT) {
                tmp.IsStopped = true;
                break;
            }
        }

        pathCount = path.Count;

        // string s = "";
        // for(int i = 0; i < path.Count;i ++) {
        //     s += path[i].Position + " ";
        // }
        // Debug.Log(s);
    }

    // 다른 차와의 관계로 Path 갱신
    public bool GetRelativePath() {
        bool changed = false;
        float progress = 0;

        for(int i = 1; i < path.Count; i++) {
            progress += timePath[i - 1]; // progress = 다음 위치에 도달하는 시간
            PathData current = path[i - 1], next = path[i]; // 현재 위치, 다음 위치

            foreach(Car car in GameManager.instance.CurrentCars) {
                if(car == this) continue;
                PathData p = car.GetPath(progress); // 다음 위치에 도달하는 시간대의 다른 차의 위치 정보

                // 다음 위치에 도달하기 전, 이미 도달해 있는 차가 있는 경우 정지
                if(p.Position != next.Position) continue; // 이미 도달해 있는가?
                if(!p.IsStopped) continue; // 그리고 정지해 있는가?

                current.IsStopped = true;
                changed = pathCount != i;
                pathCount = i;

                break;
            }

            if(current.IsStopped) break;
        }

        return changed;
    }

    // progress만큼 진행했을 때 차의 path index
    public PathData GetPath(float progress) => path[TimeToIndex(progress)];

    public int TimeToIndex(float time) {
        int index = 0;
        for(; index < path.Count; index++) {
            if(path[index].IsStopped) break;

            if((time -= timePath[index]) > 0) continue;
            break;
        }
        return index;
    }

    private PathData GetFront(PathData bef) => 
        new PathData(bef.Position + carDirection[Rotate(bef.Rotation, back: bef.IsBackward)],
                     bef.Rotation,
                     bef.IsBackward,
                     bef.IsStopped,
                     bef.ISpeed);

    public void PrevMove()
    {
        path.RemoveRange(pathCount, path.Count - pathCount);
        trace.Add(new PathData((Vector3Int)Position, Rotation, isTriggerBakcward, isTriggerStop, ISpeed));

        isTriggerStop = false;
        isTriggerBakcward = false;
        ISpeed = 1;
    }

    public void OnMove()
    {
        if (Collided) return;

        transform.localPosition = (Vector3Int)Position;
        transform.eulerAngles = carAngles[Rotate(Rotation)];
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collided = true;
        Vector2 hitPoint = collision.contacts[0].point;
        GameManager.instance.AddCollision(hitPoint);
    }

    public bool MoveTo(float progress)
    {
        if (Collided) return false;
        if (!IsMovable) return false;

        Vector3 position;
        PathData current, before;
        float clamp;

        progress /= fixedSpeed;
        if(progress > targetProgress) {
            pathIndex++;
            if(pathIndex >= pathCount) return false;

            currentProgress = targetProgress;
            targetProgress += timePath[pathIndex];
        }

        clamp = (progress - currentProgress) / timePath[pathIndex];

        current = path[pathIndex];
        if (pathIndex == 0) // 출발할 때
        {
            position = current.Position;
            position += 0.5f * Mathf.Pow(clamp, 2) * (Vector3)carDirection[Rotate(current.Rotation, back: current.IsBackward)];
        }
        else
        {
            before = path[pathIndex - 1];
            position = before.Position + current.Position;
            position *= 0.5f;

            if (pathIndex == pathCount - 1) // 정지 직전
            {
                position += (clamp - 0.5f * Mathf.Pow(clamp, 2)) * (Vector3)carDirection[Rotate(before.Rotation, back: before.IsBackward)];

                switch (GetTriggerType(current))
                {
                    case LevelBase.TriggerType.TURNLEFT:
                    case LevelBase.TriggerType.TURNRIGHT:
                        Rotation = current.Rotation;
                        
                        int curRot = current.Rotation, befRot = before.Rotation;
                        if (befRot == 3 && curRot == 0) curRot = 4;
                        if (befRot == 0 && curRot == 3) befRot = 4;

                        transform.eulerAngles = LineAnimation.Lerp(carAngles[befRot], carAngles[curRot], clamp, 0, 0.5f);
                        break;
                    case LevelBase.TriggerType.SLOW:
                    default:
                        transform.eulerAngles = carAngles[before.Rotation];

                        break;
                }
            }
            else // 평소 움직일 때
            {
                switch (GetTriggerType(current))
                {
                    case LevelBase.TriggerType.TURNLEFT:
                        position = GetTurnPosition(before, current, position, clamp, !before.IsBackward ? 1 : -1);
                        break;
                    case LevelBase.TriggerType.TURNRIGHT:
                        position = GetTurnPosition(before, current, position, clamp, !before.IsBackward ? -1 : 1);
                        break;
                    case LevelBase.TriggerType.BACKWARD:
                        if (before.IsBackward != current.IsBackward)
                            position += (clamp - Mathf.Pow(clamp, 2)) * (Vector3)carDirection[before.Rotation];
                        else position += clamp * (Vector3)carDirection[Rotate(before.Rotation, back: before.IsBackward)]; // 이미 후진 중?

                        transform.eulerAngles = carAngles[before.Rotation];
                        break;
                    case LevelBase.TriggerType.SLOW:
                        position += Decelerate(clamp) * (Vector3)carDirection[Rotate(before.Rotation, back: before.IsBackward)];
                        break;
                    default:
                        position += clamp * (Vector3)carDirection[Rotate(before.Rotation, back: before.IsBackward)];
                        transform.eulerAngles = carAngles[before.Rotation];

                        break;
                }
            }
        }

        transform.localPosition = position;
        Position = Vector2Int.RoundToInt(position);

        return true;
    }

    private float Decelerate(float t) {
        t *= 1.5f;
        return 2f / 27f * Mathf.Pow(t, 3f) - 1f / 3f * Mathf.Pow(t, 2f) + t;
    }
    private Vector3 GetTurnPosition(PathData before, PathData current, Vector3 position, float clamp, int dir)
    {
        Vector3 next = current.Position + (Vector3)carDirection[Rotate(current.Rotation, back: current.IsBackward)] * 0.5f;
        Rotation = current.Rotation;

        int curRot = current.Rotation, befRot = before.Rotation;
        if (befRot == 3 && curRot == 0) curRot = 4;
        if (befRot == 0 && curRot == 3) befRot = 4;

        transform.eulerAngles = LineAnimation.Lerp(carAngles[befRot], carAngles[curRot], clamp);

        Vector3 center = new Vector3();
        float A = Mathf.PI;

        if ((position.y - next.y) / (position.x - next.x) * dir > 0)
        {
            center.x = position.x;
            center.y = next.y;

            A *= center.y < position.y ? 0.5f : 1.5f;
        }
        else
        {
            center.x = next.x;
            center.y = position.y;

            A *= center.x < position.x ? 0f : 1f;
        }

        float angle = A + 0.5f * Mathf.PI * dir * clamp;
        position.x = center.x + Mathf.Cos(angle) * 0.5f;
        position.y = center.y + Mathf.Sin(angle) * 0.5f;

        return position;
    }

    private LevelBase.TriggerType GetTriggerType(PathData path) {
        LevelBase.TileData tile = GameManager.instance.CurrentTiles[path.Position.y][path.Position.x];

        if (tile.type != LevelBase.TileType.Trigger) return LevelBase.TriggerType.NONE;
        else return (LevelBase.TriggerType)tile.data;
    }

    private int Rotate(int rotation, int delta = 0, bool back = false)
    {
        rotation += delta;
        if (rotation < 0) rotation += 4;
        if (back) rotation += 2;

        rotation %= 4;

        return rotation;
    }

    public void SetTrigger(LevelBase.TriggerType triggerType, bool undo = false)
    {
        switch (triggerType)
        {
            case LevelBase.TriggerType.TURNLEFT: Rotation = Rotate(Rotation, undo ? -1 : 1); break;
            case LevelBase.TriggerType.TURNRIGHT: Rotation = Rotate(Rotation, undo ? 1 : -1); break;
            case LevelBase.TriggerType.STOP: isTriggerStop = !undo; break;
            case LevelBase.TriggerType.BACKWARD: isTriggerBakcward = !undo; break;
            case LevelBase.TriggerType.SLOW: ISpeed = undo ? 1 : 2; break;
            default: break;
        }

        Rotation %= 4;
        transform.eulerAngles = carAngles[Rotation];
    }

    private void PreviewUpdate()
    {
        //if (GameManager.instance.IsPlaying) return;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);
    }

    public void PreviewTrigger(float size) => targetScale = Vector3.one * size;

    public void Undo()
    {
        if (trace.Count == 0) return;
        PathData pathData = trace[trace.Count - 1];

        Position = (Vector2Int)pathData.Position;
        Rotation = pathData.Rotation;
        ISpeed = pathData.ISpeed;
        
        isTriggerBakcward = pathData.IsBackward;
        isTriggerStop = pathData.IsStopped;

        transform.localPosition = pathData.Position;
        transform.eulerAngles = carAngles[Rotate(Rotation)];

        if(Collided)
        {
            Collided = false;

            //
        }

        trace.RemoveAt(trace.Count - 1);
    }

    private void OnMouseEnter() => GameManager.instance.predictCar = this;

    private void OnMouseExit() => GameManager.instance.predictCar = null;

    private void Update() => PreviewUpdate();
}
