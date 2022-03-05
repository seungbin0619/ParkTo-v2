using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class GameManager : SingleTon<GameManager>
{
    #region [ ������Ʈ ]

    [SerializeField] 
    private Transform levelGrid;

    private Transform carTile;
    private Tilemap triggerTile;
    private Tilemap groundTile;
    private Transform predictTile;

    [SerializeField]
    private TriggerBar triggerBar;

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

    #region [ ���� ������ ]

    public bool IsGameOver { private set; get; } // ���� ���� �����ΰ�?
    public bool IsDrew { private set; get; }    // ���� �׷��� �ִ� �����ΰ�?

    public LevelBase.TileData[][] currentTiles { private set; get; }
    public List<Car> CurrentCars { private set; get; }
    public List<Trigger> CurrentTriggers { private set; get; }
    public List<Goal> CurrentGoals { private set; get; }
    public LevelBase CurrentLevel { private set; get; }

    #region [ Ʈ���� �� ���� ]

    public bool BarHide { private set; get; }

    #endregion

    #endregion

    #region [ Undo ���� ]

    public struct Behavior
    {
        public BehaviorType type;
        public List<object> args;
    }
    public enum BehaviorType
    {
        MOVE,    // ���� �̵�
        TRIGGER, // Ʈ���� ��ġ
        STATE    // Ʈ���ſ� ���� ���� ����
    }

    private List<Behavior> behaviors;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        carTile = levelGrid.GetChild(0);
        triggerTile = levelGrid.GetChild(1).GetComponent<Tilemap>();
        groundTile = levelGrid.GetChild(2).GetComponent<Tilemap>();
        //predictTile = levelGrid.GetChild(3);
    }

    private void SetLevel(int theme, int index)
    {
        if (theme < 0 || theme >= ThemeManager.instance.themes.Count) return;
        if (index < 0 || index >= ThemeManager.instance.themes[theme].levels.Count) return;

        if(ThemeManager.index != theme) // ���� �ٲ�� ���
        {
            ThemeManager.instance.SetTheme(theme);
            // �ٽ� �ε��ϱ�.

            //return;
        }

        CurrentLevel = ThemeManager.currentTheme.levels[index];

        currentTiles = new LevelBase.TileData[CurrentLevel.size.y][];
        for(int y = 0; y < CurrentLevel.tiles.Length; y++)
            currentTiles[y] = CurrentLevel.tiles[y].tile.Clone() as LevelBase.TileData[];

        CurrentCars = new List<Car>();
        CurrentTriggers = new List<Trigger>();

        behaviors = new List<Behavior>();
    }

    public void test()
    {
        EraseLevel();
        SetLevel(0, 0);
        DrawLevel();
    }

    #region [ ���� Draw ]

    private void DrawLevel()
    {
        Random.InitState(CurrentLevel.seed);
        levelGrid.transform.position = new Vector3(-CurrentLevel.size.x * 0.5f, -CurrentLevel.size.y * 0.5f);

        #region [ �÷� ���� ]

        Color[] shuffledColor = ThemeManager.currentTheme.cars.Clone() as Color[];
        for (int i = 0; i < 10; i++)
        {
            int p1 = Random.Range(0, shuffledColor.Length), p2 = Random.Range(0, shuffledColor.Length);

            Color tmp = shuffledColor[p1];
            shuffledColor[p1] = shuffledColor[p2];
            shuffledColor[p2] = tmp;
        }

        #endregion

        foreach (LevelBase.CarData carData in CurrentLevel.cars)
        {
            Car newCar = Instantiate(carPrefab, carTile);
            newCar.Initialize(carData.position, carData.rotation, shuffledColor[CurrentCars.Count]);

            CurrentCars.Add(newCar);
        }

        for (int y = 0; y < CurrentLevel.size.y; y++)
        {
            for(int x = 0; x < CurrentLevel.size.x; x++)
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

    void DrawGround(Vector2Int position)
    {
        Vector2Int[] d = new Vector2Int[4] { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };

        int count = 0;
        int data = 0x0000;

        for (int i = 0; i < d.Length; i++)
        {
            Vector2Int near = d[i] + position;

            if (near.x < 0 || near.x >= CurrentLevel.size.x) continue;
            if (near.y < 0 || near.y >= CurrentLevel.size.y) continue;
            if (currentTiles[near.y][near.x].type == LevelBase.TileType.Empty) continue;

            count++;
            data |= 1 << i;
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
                    rotation = data / 5 % 2;
                }
                else
                {
                    index = 1;
                    rotation = new int[] { 0, 1, 3, 2 }[data / 3 - 1];
                }

                break;
            case 3:
                index = 0;
                rotation = data < 0x1100 ? 0 : 1;

                break;
            case 4: index = 0; break;
        }

        groundTile.SetTile((Vector3Int)position, ThemeManager.currentTheme.grounds[index]);
        groundTile.GetInstantiatedObject((Vector3Int)position).transform.eulerAngles = new Vector3(0, 0, 90 * rotation);
    }

    private void EraseLevel()
    {
        foreach (Transform child in carTile) Destroy(child.gameObject);
        //foreach (Transform child in predictTile) Destroy(child.gameObject);

        triggerTile.ClearAllTiles();
        groundTile.ClearAllTiles();
    }

    #endregion

    #region [ Trigger Bar ���� ]

    private void UpdateTrigger()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (triggerBar.Position >= mousePosition.y)
        {
            if (BarHide) BarHide = triggerBar.Hide = false;
        }
        else if (!BarHide)
        {
            if (Input.GetMouseButtonDown(0)) // �ٱ� �κ� Ŭ���ϸ�
                BarHide = triggerBar.Hide = true;
        }
    }

    #endregion

    #region [ �ΰ��� ��� ]



    #endregion

    #region [ Undo ��� ] 

    public void AddBehavior(BehaviorType type, params object[] args)
    {
        Behavior beh = new Behavior();
        beh.type = type;
        beh.args = new List<object>();
        beh.args.AddRange(args);

        behaviors.Add(beh);
    }

    #endregion

    private void Update()
    {
        UpdateTrigger();
    }
}