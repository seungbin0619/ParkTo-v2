using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

// �����
partial class GameManager : SingleTon<GameManager>
{
    #region [ ������Ʈ ]

    [SerializeField] 
    private Transform levelGrid;

    private Transform carTile;

    [System.NonSerialized]
    public Tilemap triggerTile;
    private Tilemap groundTile;
    private Transform predictTile;
    private Transform decorateTile;
    private Transform particleTile;

    public TriggerBar triggerBar;
    private ScrollRect triggerScrollRect;
    private GameObject noTrigger;

    [SerializeField]
    private SpriteRenderer previewTrigger;

    public PlayButton playButton;

    //[SerializeField] 
    //private Car carPrefab;

    [SerializeField]
    private Tile goalTilePrefab;

    [SerializeField]
    private Trigger triggerPrefab;

    [SerializeField]
    private Tile triggerTilePrefab;

    [SerializeField]
    private ScrollRect triggerView;

    public Sprite[] triggerImages;

    [SerializeField]
    private Image triggerUnselectButton;

    #endregion

    #region [ ���� ������ ]

    public bool IsGameOver { private set; get; } // ���� ���� �����ΰ�?
    public bool IsDrew { private set; get; }    // ���� �׷��� �ִ� �����ΰ�?
    public bool IsAnimate { set; get; }

    private float lastDrawTime;

    public LevelBase.TileData[][] CurrentTiles { private set; get; }
    public List<Car> CurrentCars { private set; get; }
    public List<Trigger> CurrentTriggers { private set; get; }
    public List<Goal> CurrentGoals { private set; get; }
    public LevelBase CurrentLevel { private set; get; }
    private int LevelIndex { set; get; }

    public bool BarHide { set; get; }
    public bool IsPlayable { private set; get; }
    public bool IsPlaying { private set; get; }

    public bool TriggerSelectedMode { private set; get; }

    public Trigger SelectedTrigger { private set; get; }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        carTile = levelGrid.GetChild(0);
        triggerTile = levelGrid.GetChild(1).GetComponent<Tilemap>();
        groundTile = levelGrid.GetChild(2).GetComponent<Tilemap>();
        predictTile = levelGrid.GetChild(3);
        decorateTile = levelGrid.GetChild(4);
        particleTile = levelGrid.GetChild(5);

        triggerScrollRect = triggerBar.GetComponent<ScrollRect>();
        noTrigger = triggerScrollRect.content.GetChild(0).gameObject;
    }

    private void Update()
    {
        ReadKey();
        InteractBar();
        FollowSelectedTrigger();
    }

    private void ReadKey() {
        if(SettingManager.instance.IsOpen) return;
        if(HelpManager.IsInitialize) return;
        
        if(Input.GetKeyDown(KeyCode.Z)) Undo();
        else if(Input.GetKeyDown(KeyCode.R)) Reload();
        else if(Input.GetKeyDown(KeyCode.Space)) Move();
        #if UNITY_EDITOR
        else if(Input.GetKeyDown(KeyCode.C)) ClearLevel();
        #endif
    }
}

partial class GameManager // LevelDraw
{
    public static int SelectedLevel { get; set; } = 0;
    [SerializeField]
    private TMPro.TMP_Text levelText;

    private void Start()
    {
#if UNITY_EDITOR
        if (ThemeManager.index == -1)
        {
            StartCoroutine(PrevSetLevel(SelectedLevel, false));
            return;
        }
#endif

        StartCoroutine(PrevSetLevel(SelectedLevel, true));
    }

    private bool SetLevel(int index)
    {
        if (ThemeManager.currentTheme == null) return false;
        if (index < 0 || index >= ThemeManager.currentTheme.levels.Count)
        {
            int theme = ThemeManager.index;
            theme += index < 0 ? -1 : 1;

            if(theme >= ThemeManager.instance.themes.Count) {
                SelectManager.Delta = -1;
                SelectManager.NextPage = SelectedLevel / SelectManager.MAX_COUNT;
                
                SettingManager.instance.Goto("Under Construction");

                return false;
            }

            ThemeManager.instance.SetTheme(theme);
            SelectedLevel = ThemeManager.currentTheme.levels.Count - index + 16;
            SelectedLevel %= ThemeManager.currentTheme.levels.Count;

            SettingManager.instance.Goto("Game");
            //ActionManager.Play();

            return false;
        }

        CurrentLevel = ThemeManager.currentTheme.levels[index];
        levelText.text = CurrentLevel.name;
        LevelIndex = index;

        SelectManager.LastPlayedLevel = LevelIndex;
        SelectManager.Delta = 0;

        return true;
    }

    private void InitializeLevel()
    {
        if (CurrentLevel == null) return;

        CurrentTiles = new LevelBase.TileData[CurrentLevel.size.y][];
        for (int y = 0; y < CurrentLevel.tiles.Length; y++)
            CurrentTiles[y] = CurrentLevel.tiles[y].tile.Clone() as LevelBase.TileData[];

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

        Color[] shuffledColor = ThemeManager.currentTheme.cars.Clone() as Color[];
        for (int i = 0; i < 10; i++)
        {
            int p1 = Random.Range(0, shuffledColor.Length), p2 = Random.Range(0, shuffledColor.Length);

            Color tmp = shuffledColor[p1];
            shuffledColor[p1] = shuffledColor[p2];
            shuffledColor[p2] = tmp;
        }

        for (int i = 0; i < CurrentLevel.cars.Length; i++)
        {
            LevelBase.CarData carData = CurrentLevel.cars[i];

            Car newCar = Instantiate(ThemeManager.currentTheme.car, carTile);
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

                LevelBase.TileData tile = CurrentTiles[y][x];
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
            if (CurrentTiles[near.y][near.x].type == LevelBase.TileType.Empty) continue;

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
        foreach (Transform child in predictTile) Destroy(child.gameObject);

        triggerTile.ClearAllTiles();
        groundTile.ClearAllTiles();

        if (CurrentTriggers != null)
            foreach (Trigger trigger in CurrentTriggers)
                Destroy(trigger.gameObject);

        foreach (Transform child in decorateTile) Destroy(child.gameObject);
        foreach (Transform child in particleTile) Destroy(child.gameObject);

        IsDrew = false;
    }

    private IEnumerator PrevSetLevel(int index, bool animate = true, float delay = 0f)
    {
        IsAnimate = true;
        BarHide = triggerBar.Hide = true;

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

        lastDrawTime = Time.time;
        DrawDecorates();

        EventManager.instance.OnChange.Raise();
    }
    private IEnumerator LevelAppearEffect(int delta)
    {
        IsAnimate = true;
        UpdatePlayButton();
        
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
        UpdatePlayButton();

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
        MOVE,    // ���� �̵�
        TRIGGER, // Ʈ����
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

                if (behavior.args[2].GetType() == typeof(Car)) // ���� ����ߴٸ�.
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

        // 파티클 지우기.
        foreach (Transform child in particleTile) Destroy(child.gameObject);
        SFXManager.instance.PlaySound(3);
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
        //previewTrigger.gameObject.SetActive(true);

        SelectedTrigger = trigger;
        TriggerSelectedMode = false;

        triggerUnselectButton.enabled = true;
        triggerUnselectButton.transform.GetChild(0).gameObject.SetActive(true);

        EventManager.instance.OnSelectTrigger.Raise();
        EventManager.instance.OnSelectedTriggerStateChange.Raise(false);
    }

    public void UnselectTrigger()
    {
        previewTrigger.gameObject.SetActive(false);
        triggerUnselectButton.enabled = false;
        triggerUnselectButton.transform.GetChild(0).gameObject.SetActive(false);

        SelectedTrigger = null;

        EventManager.instance.OnUnselectTrigger.Raise();
    }

    public void UseTrigger()
    {
        if (SelectedTrigger == null) return;

        CurrentTriggers.Remove(SelectedTrigger);
        Destroy(SelectedTrigger.gameObject);

        UnselectTrigger();

        UpdateTriggerBar();
        EventManager.instance.OnChange.Raise();
    }

    public void SetTrigger(Vector3Int position, LevelBase.TriggerType triggerType)
    {
        if (triggerType == LevelBase.TriggerType.NORMAL)
        {
            triggerTile.SetTile(position, null);
            CurrentTiles[position.y][position.x].type = LevelBase.TileType.Normal;
        }
        else
        {
            triggerTile.SetTile(position, triggerTilePrefab);
            TriggerTile tile = triggerTile.GetInstantiatedObject(position).GetComponent<TriggerTile>();

            tile.Initialize(triggerImages[(int)triggerType]);

            CurrentTiles[position.y][position.x].type = LevelBase.TileType.Trigger;
            CurrentTiles[position.y][position.x].data = (int)triggerType;
        }
    }

    private void FollowSelectedTrigger()
    {
        if (SelectedTrigger == null) return;

        bool tileValid = false;

        if (Input.GetMouseButtonDown(1))
        {
            TriggerSelectedMode = !TriggerSelectedMode;
            EventManager.instance.OnSelectedTriggerStateChange.Raise(TriggerSelectedMode);
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); mousePosition.z = 0;
        Vector3Int tilePosition = triggerTile.WorldToCell(mousePosition);

        if (!previewTrigger.gameObject.activeSelf) previewTrigger.gameObject.SetActive(true);

        bool mb0Click = Input.GetMouseButtonDown(0) && !HelpManager.instance.Focusing;

        foreach (Car tmp in CurrentCars)
            tmp.PreviewTrigger(0.8f);
            
        if (TriggerSelectedMode)
        {
            Car car = null;
            foreach (Car tmp in CurrentCars)
            {
                if (tilePosition != (Vector3Int)tmp.Position) continue;

                car = tmp;
                break;
            }

            if (car == null)
            {
                previewTrigger.transform.position = mousePosition;
                tileValid = false;

                if (mb0Click) SFXManager.instance.PlaySound(9);
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
                    SFXManager.instance.PlaySound(8);
                    car.SetTrigger(SelectedTrigger.Type);

                    AddBehavior(BehaviorType.TRIGGER, SelectedTrigger.Type, SelectedTrigger.transform.GetSiblingIndex(), car);
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

                if (mb0Click) SFXManager.instance.PlaySound(9);
            }
            else
            {
                tileValid = CurrentTiles[tilePosition.y][tilePosition.x].type == LevelBase.TileType.Normal;
                if (!mb0Click)
                {
                    previewTrigger.transform.localPosition = tilePosition + new Vector3(0.5f, 0.5f, 0);
                }
                else if (tileValid)
                {
                    SFXManager.instance.PlaySound(8);
                    SetTrigger(tilePosition, SelectedTrigger.Type);

                    AddBehavior(BehaviorType.TRIGGER, SelectedTrigger.Type, SelectedTrigger.transform.GetSiblingIndex(), tilePosition);
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

partial class GameManager
{
    [SerializeField]
    private Predictor predictor;
    public Car predictCar { set; get; }

    public void Move()
    {
        if(IsPlaying) return;
        if(!IsPlayable) return;
        
        // 출발음
        SFXManager.instance.PlaySound(5);

        BarHide = triggerBar.Hide = true;
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
        
        IsPlaying = false;
        EventManager.instance.OnMove.Raise();
    }

    public void OnMove()
    {
        AddBehavior(BehaviorType.MOVE);

        if (CurrentCars.FindAll(p => p.Collided).Count > 0)
        {
            IsPlayable = false;
            IsGameOver = true;

            if(SteamManager.Initialized) {
                Steamworks.SteamUserStats.GetStat("CRASH", out int crash);
                Steamworks.SteamUserStats.SetStat("CRASH", crash + 1);

                Steamworks.SteamUserStats.StoreStats();
            }

            return;
        }

        EventManager.instance.OnChange.Raise();
    }

    public void OnChange()
    {
        // 클리어 체크
        if (CurrentGoals.FindAll(p => p.IsArrived).Count == CurrentGoals.Count)
        {
            IsPlayable = false;

            ClearLevel();
            return;
        }

        GetNextPath();
        DrawPredictors();

        BarHide = triggerBar.Hide = CurrentTriggers.Count == 0;
        UpdatePlayButton();
    }

    private void ClearLevel() {
        int currentClearedLevel = SteamDataManager.GetData("Game", "Theme" + ThemeManager.index, 0);
        if(currentClearedLevel < LevelIndex + 1) {
            if(LevelIndex + 1 == SelectManager.MAX_COUNT && ThemeManager.index < ThemeManager.instance.themes.Count - 1) {
                NoticeManager.instance.NoticeString(LocalizationManager.instance.LocaleText("UIText", "notice_unlock_level"));
            }

            SteamDataManager.SetData("Game", "Theme" + ThemeManager.index, LevelIndex + 1);
            SteamDataManager.SaveData();
        }

        SteamApiManager.instance.CheckClearAchievements();
        
        SFXManager.instance.PlaySound(7);
        for(int i = 0; i < Random.Range(5, 9); i++) AddClear();

        StartCoroutine(PrevSetLevel(LevelIndex + 1, delay: 1f));
    }

    private void GetNextPath()
    {
        for (int i = 0; i < CurrentCars.Count; i++)
            CurrentCars[i].InitPath();

        while (true)
        {
            bool flag = false;
            for(int i = 0; i < CurrentCars.Count; i++)
                flag = CurrentCars[i].GetRelativePath() || flag;

            if(!flag) break;
        }

        IsPlayable = false;
        for (int i = 0; i < CurrentCars.Count; i++) // �� �̵� ���� üũ
            IsPlayable = CurrentCars[i].IsMovable || IsPlayable;

        //UpdatePlayButton();
    }

    private void DrawPredictors()
    {
        foreach (Transform child in predictTile.transform)
            Destroy(child.gameObject);

        float maxPathProgress = 0;
        for (int i = 0; i < CurrentCars.Count; i++) {
            float pathProgress = 0;
            for(int j = 0; j < CurrentCars[i].timePath.Count; j++)
                pathProgress += CurrentCars[i].timePath[j];
            maxPathProgress = Mathf.Max(pathProgress, maxPathProgress);
        }

        for (int i = 0; i < CurrentCars.Count; i++) {
            float progress = 0;
            for (int j = 1; j < CurrentCars[i].pathCount; j++)
            {
                Vector3Int targetPosition = CurrentCars[i].path[j].Position;
                
                // 큰 점
                Predictor tmpPredictor = Instantiate(predictor, predictTile.transform);
                tmpPredictor.Initialize(CurrentCars[i],
                                        progress + CurrentCars[i].timePath[j - 1],
                                        targetPosition,
                                        lastDrawTime,
                                        maxPathProgress);
                
                // 작은 점
                tmpPredictor = Instantiate(predictor, predictTile.transform);
                tmpPredictor.Initialize(CurrentCars[i],
                                        progress + CurrentCars[i].timePath[j - 1] * 0.5f,
                                        (Vector3)(CurrentCars[i].path[j - 1].Position + targetPosition) * 0.5f,
                                        lastDrawTime,
                                        maxPathProgress,
                                        true);
            
                progress += CurrentCars[i].timePath[j - 1];
            }
        }
    }

    public bool IsValidPosition(Vector3Int position)
    {
        int x = position.x, y = position.y;

        if (x < 0 || x > CurrentLevel.size.x - 1 || y < 0 || y > CurrentLevel.size.y - 1) return false;
        if (CurrentTiles[y][x].type == LevelBase.TileType.Empty) return false;

        return true;
    }

    public void Reload()
    {
        if (IsAnimate || IsPlaying) return;

        SFXManager.instance.PlaySound(4);
        StartCoroutine(PrevSetLevel(LevelIndex));
    }

    public void ReHelp() {
        if (IsAnimate || IsPlaying) return;

        CurrentLevel.decorates[0].decorate.GetComponent<HelpBase>().Reset();
        Reload();
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
            if (Input.GetMouseButtonDown(0)) // �ٱ� �κ� Ŭ���ϸ�
            {
                BarHide = triggerBar.Hide = true;
                UpdatePlayButton();
            }
        }
    }
}

partial class GameManager {
    [SerializeField]
    ParticleSystem gameOverParticle;

    [SerializeField]
    ParticleSystem gameClearParticle;

    public void AddCollision(Vector2 position)
    {
        ParticleSystem p = Instantiate(gameOverParticle, particleTile);
        p.transform.position = position;
    }

    private readonly Color32[] colors = new Color32[]
    {
        new Color32(212, 75, 152, 255),
        new Color32(124, 99, 215, 255),
        new Color32(234, 215, 76, 255),
        new Color32(98, 216, 126, 255),
        new Color32(92, 125, 218, 255)
    };

    public void AddClear()
    {
        ParticleSystem p = Instantiate(gameClearParticle, particleTile);
        float rx = Random.Range(-1.5f, 1.5f), ry = Random.Range(-1.5f, 1.5f);

        p.transform.position = new Vector2(rx, ry);
        ParticleSystem.MainModule main = p.main;

        main.startColor = new ParticleSystem.MinMaxGradient(colors[Random.Range(0, colors.Length)]);
    }
}