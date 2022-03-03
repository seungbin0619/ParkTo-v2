using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : SingleTon<GameManager>
{
    #region [ 오브젝트 ]

    [SerializeField] 
    private Transform levelGrid;

    private Transform carTile;
    private Tilemap triggerTile;
    private Tilemap groundTile;
    private Transform predictTile;

    [SerializeField] 
    private Car carPrefab;

    #endregion

    #region [ 게임 데이터 ]

    public bool IsGameOver { private set; get; } // 게임 오버 상태인가?
    public bool IsDrew { private set; get; }    // 맵이 그려져 있는 상태인가?

    public LevelBase.TileData[][] currentTiles { private set; get; }
    public List<Car> CurrentCars { private set; get; }
    public List<Trigger> CurrentTriggers { private set; get; }
    public List<Goal> CurrentGoals { private set; get; }
    public LevelBase CurrentLevel { private set; get; }

    #endregion

    #region [ Undo 관련 ]

    public struct Behavior
    {
        public BehaviorType type;
        public List<object> args;
    }
    public enum BehaviorType
    {
        MOVE,    // 차의 이동
        TRIGGER, // 트리거 배치
        STATE    // 트리거에 의한 상태 변경
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

        if(ThemeManager.index != theme) // 씬이 바뀌는 경우
        {
            ThemeManager.instance.SetTheme(theme);
            // 다시 로드하기.

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
        SetLevel(0, 0);
        DrawLevel();
    }

    #region [ 레벨 Draw ]

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


                        break;
                    case LevelBase.TileType.Trigger:


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

    }

    #endregion

    #region [ Undo 기능 ] 

    public void AddBehavior(BehaviorType type, params object[] args)
    {
        Behavior beh = new Behavior();
        beh.type = type;
        beh.args = new List<object>();
        beh.args.AddRange(args);

        behaviors.Add(beh);
    }

    #endregion
}