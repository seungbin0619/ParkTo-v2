using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

// 선언부
partial class GameManager : SingleTon<GameManager>
{
    #region [ 오브젝트 ]

    [SerializeField] 
    private Transform levelGrid;

    private Transform carTile;
    private Tilemap triggerTile;
    private Tilemap groundTile;
    private Transform predictTile;
    private Transform decorateTile;

    [SerializeField]
    private TriggerBar triggerBar;
    private ScrollRect triggerScrollRect;
    private GameObject noTrigger;

    [SerializeField]
    private SpriteRenderer previewTrigger;

    [SerializeField]
    private PlayButton playButton;

    [SerializeField] 
    private Car carPrefab;

    [SerializeField]
    private Tile goalTilePrefab;

    [SerializeField]
    private Trigger triggerPrefab;

    [SerializeField]
    private Tile triggerTilePrefab;

    [SerializeField]
    private ScrollRect triggerView;

    public Sprite[] triggerImages;

    #endregion

    #region [ 게임 데이터 ]

    public bool IsGameOver { private set; get; } // 게임 오버 상태인가?
    public bool IsDrew { private set; get; }    // 맵이 그려져 있는 상태인가?
    private bool IsAnimate { set; get; }

    public LevelBase.TileData[][] currentTiles { private set; get; }
    public List<Car> CurrentCars { private set; get; }
    public List<Trigger> CurrentTriggers { private set; get; }
    public List<Goal> CurrentGoals { private set; get; }
    public LevelBase CurrentLevel { private set; get; }
    private int LevelIndex { set; get; }

    public bool BarHide { private set; get; }
    public bool IsPlayable { private set; get; }
    public bool IsPlaying { private set; get; }

    public bool TriggerSelectedMode { private set; get; }

    private Trigger selectedTrigger;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        carTile = levelGrid.GetChild(0);
        triggerTile = levelGrid.GetChild(1).GetComponent<Tilemap>();
        groundTile = levelGrid.GetChild(2).GetComponent<Tilemap>();
        predictTile = levelGrid.GetChild(3);
        decorateTile = levelGrid.GetChild(4);

        triggerScrollRect = triggerBar.GetComponent<ScrollRect>();
        noTrigger = triggerScrollRect.content.GetChild(0).gameObject;
    }

    private void Update()
    {
        InteractBar();
        FollowSelectedTrigger();
    }
}

partial class GameManager // LeveDraw
{
    private void Start()
    {
#if UNITY_EDITOR
        if (ThemeManager.index == -1)
        {
            StartCoroutine(PrevSetLevel(0, false));
            return;
        }
#endif

        StartCoroutine(PrevSetLevel(0, true));
    }

    private bool SetLevel(int index)
    {
        if (ThemeManager.currentTheme == null) return false;
        if (index < 0 || index >= ThemeManager.currentTheme.levels.Count)
        {
            int theme = ThemeManager.index;
            theme += (int)Mathf.Sign(index - ThemeManager.currentTheme.levels.Count);

            ThemeManager.instance.SetTheme(theme);

            ActionManager.AddAction("FadeIn", 0.5f);
            ActionManager.AddAction("Move", "Game");
            ActionManager.AddAction("FadeOut", 0.5f);

            ActionManager.Play();

            return false;
        }

        CurrentLevel = ThemeManager.currentTheme.levels[index];
        LevelIndex = index;
        return true;
    }

    private void InitializeLevel()
    {
        if (CurrentLevel == null) return;

        currentTiles = new LevelBase.TileData[CurrentLevel.size.y][];
        for (int y = 0; y < CurrentLevel.tiles.Length; y++)
            currentTiles[y] = CurrentLevel.tiles[y].tile.Clone() as LevelBase.TileData[];

        CurrentCars = new List<Car>();
        CurrentGoals = new List<Goal>();
        CurrentTriggers = new List<Trigger>();

        behaviors = new List<Behavior>();
        IsGameOver = false;
    }

    private void DrawLevel()
    {
        Random.InitState(CurrentLevel.seed);
        levelGrid.transform.position = new Vector3(-CurrentLevel.size.x * 0.5f, -CurrentLevel.size.y * 0.5f);

        #region [ 컬러 셔플 ]

        Color[] shuffledColor = ThemeManager.currentTheme.cars.Clone() as Color[];
        for (int i = 0; i < 10; i++)
        {
            int p1 = Random.Range(0, shuffledColor.Length), p2 = Random.Range(0, shuffledColor.Length);

            Color tmp = shuffledColor[p1];
            shuffledColor[p1] = shuffledColor[p2];
            shuffledColor[p2] = tmp;
        }

        #endregion

        for (int i = 0; i < CurrentLevel.cars.Length; i++)
        {
            LevelBase.CarData carData = CurrentLevel.cars[i];

            Car newCar = Instantiate(carPrefab, carTile);
            newCar.Initialize(carData.position, carData.rotation, shuffledColor[CurrentCars.Count]);

            CurrentCars.Add(newCar);
        }

        for (int i = 0; i < CurrentLevel.triggers.Length; i++)
            AddTrigger(CurrentLevel.triggers[i]);


        for (int y = 0; y < CurrentLevel.size.y; y++)
        {
            for (int x = 0; x < CurrentLevel.size.x; x++)
            {
                Vector2Int position = new Vector2Int(x, y);

                LevelBase.TileData tile = currentTiles[y][x];
                switch (tile.type)
                {
                    case LevelBase.TileType.Empty:
                        continue;
                    case LevelBase.TileType.Normal:
                        DrawGround(position);

                        break;
                    case LevelBase.TileType.Goal:
                        DrawGround(position);

                        triggerTile.SetTile((Vector3Int)position, goalTilePrefab);
                        Goal goal = triggerTile.GetInstantiatedObject((Vector3Int)position).GetComponent<Goal>();
                        goal.Initialize(position, CurrentCars[tile.data]);

                        CurrentGoals.Add(goal);

                        break;
                    case LevelBase.TileType.Trigger:
                        DrawGround(position);

                        triggerTile.SetTile((Vector3Int)position, triggerTilePrefab);
                        TriggerTile trigger = triggerTile.GetInstantiatedObject((Vector3Int)position).GetComponent<TriggerTile>();
                        trigger.Initialize(triggerImages[tile.data]);

                        break;
                }
            }
        }

        IsDrew = true;
    }

    private void DrawGround(Vector2Int position)
    {
        Vector2Int[] d = new Vector2Int[4] { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };

        uint count = 0;
        uint data = 0x0000;

        for (int i = 0; i < d.Length; i++)
        {
            Vector2Int near = d[i] + position;

            if (near.x < 0 || near.x >= CurrentLevel.size.x) continue;
            if (near.y < 0 || near.y >= CurrentLevel.size.y) continue;
            if (currentTiles[near.y][near.x].type == LevelBase.TileType.Empty) continue;

            count++;
            data |= (uint)(1 << i);
        }

        int index = -1, rotation = 0;

        switch (count)
        {
            case 1:
                index = 2;
                for (int i = 1; i < data; i *= 2)
                    rotation++;

                break;
            case 2:
                if (data % 5 == 0)
                {
                    index = 0;
                    rotation = (int)data / 5 % 2;
                }
                else
                {
                    index = 1;
                    rotation = new int[] { 0, 1, 3, 2 }[data / 3 - 1];
                }

                break;
            case 3:
                index = 0;
                rotation = (data & 1) == 0 || (data & 4) == 0 ? 0 : 1;

                break;
            case 4: index = 0; break;
        }

        groundTile.SetTile((Vector3Int)position, ThemeManager.currentTheme.grounds[index]);
        groundTile.GetInstantiatedObject((Vector3Int)position).transform.eulerAngles = new Vector3(0, 0, 90 * rotation);
    }

    private void DrawDecorates()
    {
        for (int i = 0; i < CurrentLevel.decorates.Length; i++)
        {
            LevelBase.DecorateData decorateData = CurrentLevel.decorates[i];
            if (decorateData.decorate == null) return;

            _ = Instantiate(decorateData.decorate,
                            (Vector3Int)decorateData.position,
                            Quaternion.Euler(0, 0, 0),
                            decorateTile);
        }
    }

    private void EraseLevel()
    {
        foreach (Transform child in carTile) Destroy(child.gameObject);
        //foreach (Transform child in predictTile) Destroy(child.gameObject);

        triggerTile.ClearAllTiles();
        groundTile.ClearAllTiles();

        if (CurrentTriggers != null)
            foreach (Trigger trigger in CurrentTriggers)
                Destroy(trigger.gameObject);

        foreach (Transform child in decorateTile) Destroy(child.gameObject);

        IsDrew = false;
    }

    private IEnumerator PrevSetLevel(int index, bool animate = true, float delay = 0f)
    {
        if (delay > 0) yield return YieldDictionary.WaitForSeconds(delay);
        if (CurrentLevel != null)
        {
            yield return LevelAppearEffect(0);
            EraseLevel();
        }

        if (!SetLevel(index)) yield break;
        InitializeLevel();
        DrawLevel();

        if (animate) yield return LevelAppearEffect(1);
        DrawDecorates();

        EventManager.instance.OnChange.Raise();
    }
    private IEnumerator LevelAppearEffect(int delta)
    {
        IsAnimate = true;

        Vector3 currentPosition = levelGrid.transform.position, targetPosition;
        currentPosition.x += 20f * delta;

        targetPosition = currentPosition;
        targetPosition.x -= 20f;

        float duration = 1f, progress = 0;
        while (true)
        {
            progress += Time.deltaTime;
            float clamp = Mathf.Clamp(progress / duration, 0, 1);

            levelGrid.transform.position = LineAnimation.Lerp(currentPosition, targetPosition, clamp, (delta + 1) % 2, delta);

            yield return YieldDictionary.WaitForEndOfFrame;
            if (progress > duration) break;
        }

        IsAnimate = false;
        yield break;
    }
}

partial class GameManager // Undo
{
    public struct Behavior
    {
        public BehaviorType type;
        public List<object> args;
    }
    public enum BehaviorType
    {
        MOVE,    // 차의 이동
        TRIGGER, // 트리거
    }

    private List<Behavior> behaviors;

    public void AddBehavior(BehaviorType type, params object[] args)
    {
        Behavior beh = new Behavior();
        beh.type = type;
        beh.args = new List<object>();
        beh.args.AddRange(args);

        behaviors.Add(beh);
    }

    public void Undo()
    {
        if (IsAnimate || !IsDrew || IsPlaying) return;
        if (behaviors.Count == 0) return;

        Behavior behavior = behaviors[behaviors.Count - 1];
        switch (behavior.type)
        {
            case BehaviorType.MOVE:
                for (int i = 0; i < CurrentCars.Count; i++)
                    CurrentCars[i].Undo();
                if (IsGameOver) IsGameOver = false;

                break;
            case BehaviorType.TRIGGER:
                LevelBase.TriggerType trigger = (LevelBase.TriggerType)behavior.args[0];
                int index = int.Parse(behavior.args[1].ToString());

                if (behavior.args[2].GetType() == typeof(Car)) // 차에 사용했다면.
                {
                    Car car = behavior.args[2] as Car;
                    car.SetTrigger(trigger, true);
                }
                else
                {
                    Vector3Int position = (Vector3Int)behavior.args[2];
                    SetTrigger(position, LevelBase.TriggerType.NORMAL);
                }

                AddTrigger(trigger, index);

                break;
        }

        behaviors.RemoveAt(behaviors.Count - 1);
        EventManager.instance.OnChange.Raise();
    }
}

partial class GameManager // Trigger
{
    public void AddTrigger(LevelBase.TriggerType trigger, int index = -1)
    {
        Trigger newTrigger = Instantiate(triggerPrefab, triggerScrollRect.content);
        newTrigger.Initialize(trigger, triggerScrollRect);

        if (index == -1 || CurrentTriggers.Count < index)
            CurrentTriggers.Add(newTrigger);
        else
        {
            CurrentTriggers.Insert(index, newTrigger);
            newTrigger.transform.SetSiblingIndex(index);
        }

        UpdateTriggerBar();
    }

    public void SelectTrigger(Trigger trigger)
    {
        if (IsGameOver) return;
        if (IsPlaying) return;

        previewTrigger.sprite = trigger.sprite;
        previewTrigger.gameObject.SetActive(true);

        selectedTrigger = trigger;
        TriggerSelectedMode = false;

        EventManager.instance.OnSelectTrigger.Raise();
        EventManager.instance.OnSelectedTriggerStateChange.Raise(false);
    }

    public void UnselectTrigger()
    {
        previewTrigger.gameObject.SetActive(false);
        selectedTrigger = null;

        EventManager.instance.OnUnselectTrigger.Raise();
    }

    public void UseTrigger()
    {
        if (selectedTrigger == null) return;

        previewTrigger.gameObject.SetActive(false);

        CurrentTriggers.Remove(selectedTrigger);
        Destroy(selectedTrigger.gameObject);

        selectedTrigger = null;

        UpdateTriggerBar();
        EventManager.instance.OnChange.Raise();
    }

    public void SetTrigger(Vector3Int position, LevelBase.TriggerType triggerType)
    {
        if (triggerType == LevelBase.TriggerType.NORMAL)
        {
            triggerTile.SetTile(position, null);
            currentTiles[position.y][position.x].type = LevelBase.TileType.Normal;
        }
        else
        {
            triggerTile.SetTile(position, triggerTilePrefab);
            TriggerTile tile = triggerTile.GetInstantiatedObject(position).GetComponent<TriggerTile>();

            tile.Initialize(triggerImages[(int)triggerType]);

            currentTiles[position.y][position.x].type = LevelBase.TileType.Trigger;
            currentTiles[position.y][position.x].data = (int)triggerType;
        }
    }

    private void FollowSelectedTrigger()
    {
        if (selectedTrigger == null) return;

        bool tileValid = false;

        if (Input.GetMouseButtonDown(1))
        {
            TriggerSelectedMode = !TriggerSelectedMode;
            EventManager.instance.OnSelectedTriggerStateChange.Raise(TriggerSelectedMode);
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); mousePosition.z = 0;
        Vector3Int tilePosition = triggerTile.WorldToCell(mousePosition);

        if (!previewTrigger.gameObject.activeSelf) previewTrigger.gameObject.SetActive(true);

        bool mb0Click = Input.GetMouseButtonDown(0);

        if (TriggerSelectedMode)
        {
            Car car = null;
            foreach (Car tmp in CurrentCars)
            {
                if (tilePosition != (Vector3Int)tmp.Position) continue;

                car = tmp;
                break;
            }

            foreach (Car tmp in CurrentCars)
                tmp.PreviewTrigger(0.8f);

            if (car == null)
            {
                previewTrigger.transform.position = mousePosition;
                tileValid = false;
            }
            else
            {
                if (!mb0Click)
                {
                    previewTrigger.gameObject.SetActive(false);

                    car.PreviewTrigger(1f);
                }
                else
                {
                    car.SetTrigger(selectedTrigger.Type);

                    AddBehavior(BehaviorType.TRIGGER, selectedTrigger.Type, selectedTrigger.transform.GetSiblingIndex(), car);
                    UseTrigger();

                    EventManager.instance.OnUnselectTrigger.Raise();
                }
            }
        }
        else
        {
            if (!IsValidPosition(tilePosition))
            {
                previewTrigger.transform.position = mousePosition;
                tileValid = false;
            }
            else
            {
                tileValid = currentTiles[tilePosition.y][tilePosition.x].type == LevelBase.TileType.Normal;
                if (!mb0Click)
                {
                    previewTrigger.transform.localPosition = tilePosition + new Vector3(0.5f, 0.5f, 0);
                }
                else if (tileValid)
                {
                    SetTrigger(tilePosition, selectedTrigger.Type);

                    AddBehavior(BehaviorType.TRIGGER, selectedTrigger.Type, selectedTrigger.transform.GetSiblingIndex(), tilePosition);
                    UseTrigger();

                    EventManager.instance.OnUnselectTrigger.Raise();
                }
                else { }
            }
        }

        previewTrigger.color = tileValid ? Color.white : Color.red * 0.5f;
    }

    private void UpdateTriggerBar()
    {
        int count = CurrentTriggers.Count;

        noTrigger.SetActive(count == 0);
        triggerBar.Size = count;
    }

    private void UpdatePlayButton()
    {
        bool flag = !BarHide || !IsPlayable || IsPlaying || IsAnimate || IsGameOver;
        playButton.Hide = flag;

        if (!BarHide && !flag) BarHide = true;
    }
}

partial class GameManager // 이동 및 기타 UI 기능
{
    public void Move()
    {
        IsPlaying = true;
        UpdatePlayButton();

        StartCoroutine(MoveCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        EventManager.instance.PrevMove.Raise();

        float progress = 0;
        bool complete = false;

        while (!complete)
        {
            complete = true;

            for (int i = 0; i < CurrentCars.Count; i++)
                complete = !CurrentCars[i].MoveTo(progress) && complete;

            yield return YieldDictionary.WaitForEndOfFrame;
            progress += Time.deltaTime;
        }

        EventManager.instance.OnMove.Raise();
        IsPlaying = false;
    }

    public void OnMove() // 이동 직후
    {
        AddBehavior(BehaviorType.MOVE);

        if (CurrentCars.FindAll(p => p.Collided).Count > 0)
        {
            IsGameOver = true;

            return;
        }

        EventManager.instance.OnChange.Raise();
    }

    public void OnChange()
    {
        if (CurrentGoals.FindAll(p => p.IsArrived).Count == CurrentGoals.Count)
        {
            StartCoroutine(PrevSetLevel(LevelIndex + 1, delay: 1f));
            return;
        }

        GetNextPath();

        BarHide = triggerBar.Hide = CurrentTriggers.Count == 0; // 한 번 움직인 후 올라오기
        UpdatePlayButton();
    }

    private void GetNextPath()
    {
        for (int i = 0; i < CurrentCars.Count; i++)
            CurrentCars[i].InitPath();

        while (true)
        {
            bool pFlag = false;
            for (int i = 0; i < CurrentCars.Count; i++)
            {
                CurrentCars[i].GetNextPath();
                pFlag = pFlag || !CurrentCars[i].Stopped;
            }

            if (!pFlag) break;
        }
        for (int i = 0; i < CurrentCars.Count; i++) // 차 이동 가능 체크
            IsPlayable = CurrentCars[i].IsMovable || IsPlayable;

        UpdatePlayButton();
    }

    public bool IsValidPosition(Vector3Int position)
    {
        int x = position.x, y = position.y;

        if (x < 0 || x > CurrentLevel.size.x - 1 || y < 0 || y > CurrentLevel.size.y - 1) return false;
        if (currentTiles[y][x].type == LevelBase.TileType.Empty) return false;

        return true;
    }

    public void Reload()
    {
        if (IsAnimate) return;
        StartCoroutine(PrevSetLevel(LevelIndex));
    }

    private void InteractBar()
    {
        if (!IsDrew || IsAnimate || IsPlaying || HelpManager.IsInitialize) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (triggerBar.Position >= mousePosition.y)
        {
            if (BarHide)
            {
                BarHide = triggerBar.Hide = false;
                UpdatePlayButton();
            }
        }
        else if (!BarHide)
        {
            if (Input.GetMouseButtonDown(0)) // 바깥 부분 클릭하면
            {
                BarHide = triggerBar.Hide = true;
                UpdatePlayButton();
            }
        }
    }
}
