using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public partial class SelectManager : SingleTon<SelectManager>
{
    public static int Delta { get; set; } = 1; // 어디서 왔는지?
    public static int LastSelectedLevel {get; set; } = -1;
    public static int NextPage {get; set; } = 0;
    public static bool IsFirstEnter { get; set; } = false;

    public const int MAX_COUNT = 8;
    private const int RANGE = 15;

    [SerializeField]
    private Transform tile;

    [SerializeField]
    private Tilemap selectTile;

    [SerializeField]
    private Transform linePanel;

    private List<Button> buttons = new List<Button>();
    private List<Mask> lines = new List<Mask>();

    [SerializeField]
    private List<Button> pageButton;

    private bool entered = false;
    private int Page { get; set; }
    private int SelectedIndex { get; set; }
    private int LevelCount { get; set; }

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < linePanel.childCount; i++)
        {
            Transform child = linePanel.GetChild(i);

            if (i % 2 == 0) buttons.Add(child.GetComponent<Button>());
            else lines.Add(linePanel.GetChild(i).GetComponent<Mask>());
        }
    }

    private void Start()
    {
        if(IsFirstEnter) { // 시작 버튼으로 진입한 경우
            int clearedLevel = DataManager.GetData("Game", "Theme" + ThemeManager.index, 0);

            DrawLevels(clearedLevel / MAX_COUNT);
            SelectIndex(MAX_COUNT - 1); // 클리어 한 가장 마지막 레벨로 이동
        }
        else if(LastSelectedLevel == -1) { // 좌우 버튼으로 온 경우
            DrawLevels(NextPage);
            SelectIndex(Delta == 1 ? 0 : LevelCount - 1);
        }
        else { // 레벨 진행 도중에 나온 경우
            DrawLevels(LastSelectedLevel / MAX_COUNT);
            SelectIndex(LastSelectedLevel % MAX_COUNT);
        }

        Vector3 targetPosition = buttons[SelectedIndex].transform.position;
        targetPosition.x -= Delta;
        car.transform.position = targetPosition;

        LastSelectedLevel = -1;
        IsFirstEnter = false;
    }

    public void SelectIndex(int position) {
        if(entered) return;

        if(position < 0) position = 0;
        else if(position >= LevelCount) position = LevelCount - 1;

        SelectedIndex = position;
    }

    public void SelectIndexDelta(bool delta) {
        if(entered) return;
        
        int position = SelectedIndex + (delta ? 1 : -1);
        if(position < 0 || position >= LevelCount) {
            ChangePageDelta(delta);
            return;
        }

        SelectIndex(position);
    }

    public void EnterGame() {
        if(entered) return;

        entered = true;
        GameManager.SelectedLevel = SelectedIndex + Page * MAX_COUNT;
        SettingManager.instance.Goto("Game");
    }

    public void ChangePageDelta(bool delta) { // 페이지 바뀔 때
        if(entered) return;
        if(!pageButton[delta ? 1 : 0].interactable) return;

        Delta = delta ? 1 : -1;
        NextPage = Page + Delta;
        SelectedIndex = Delta * LevelCount;

        SettingManager.instance.Goto("Select");
    }

    private void DrawLevels(int page = 0)
    {
        if(entered) return;
        if (ThemeManager.currentTheme == null) return;

        //Debug.Log((ThemeManager.currentTheme.levels.Count - 1) / MAX_COUNT + " " + page);
        if(page < 0 || (ThemeManager.currentTheme.levels.Count - 1) / MAX_COUNT < page) // 페이지 이동
        {
            // 페이지가 0 아래면 전으로
            int sign = page < 0 ? -1 : 1, targetTheme = ThemeManager.index + sign;

            ThemeManager.instance.SetTheme(targetTheme);
            page = page < 0 ? (ThemeManager.currentTheme.levels.Count - 1) / MAX_COUNT : 0;
        }

        Page = page;
        int clearedLevel = DataManager.GetData("Game", "Theme" + ThemeManager.index, 0);

        for(int i = 0; i < MAX_COUNT; i++)
        {
            int levelIndex = page * MAX_COUNT + i;
            bool flag = clearedLevel >= levelIndex;
            if(flag) LevelCount = i + 1;

            if (ThemeManager.currentTheme.levels.Count > levelIndex) // 레벨 개수만큼 보이기.
            {
                buttons[i].gameObject.SetActive(true);
                if(i > 0) lines[i - 1].gameObject.SetActive(true);
            } else break;
            
            buttons[i].interactable = flag;
            if (i > 0) lines[i - 1].enabled = !flag;
        }

        Canvas.ForceUpdateCanvases();

        Vector3Int position = new Vector3Int(-RANGE, 0, 0);
        for (int i = -RANGE; i <= RANGE; i++)
        {
            selectTile.SetTile(position, ThemeManager.currentTheme.grounds[0]);
            position.x++;
        }

        car = Instantiate(ThemeManager.currentTheme.carBase);
        car.color = ThemeManager.currentTheme.cars[Random.Range(0, ThemeManager.currentTheme.cars.Length)];
        car.transform.rotation = Quaternion.Euler(0, 0, -90f);

        car.gameObject.AddComponent<CarSelect>();

        pageButton[0].interactable = ThemeManager.index > 0 || page > 0;
        bool interactable = true;
        // 마지막 페이지가 아니면 true
        interactable &= 
            !(ThemeManager.index == ThemeManager.instance.themes.Count - 1 && 
            page == ThemeManager.currentTheme.levels.Count / MAX_COUNT);

        // 현재 테마의 레벨을 8개 이상 클리어
        interactable &= clearedLevel >= 8;

        pageButton[1].interactable = interactable;
    }

    private void ReadKey() {
        if(ActionManager.IsPlaying) return;
        
        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) SelectIndexDelta(false);
        else if(Input.GetKeyDown(KeyCode.RightArrow)|| Input.GetKeyDown(KeyCode.D)) SelectIndexDelta(true);
        
        if(Input.GetKeyDown(KeyCode.Space) ||Input.GetKeyDown(KeyCode.Return) ||Input.GetKeyDown(KeyCode.KeypadEnter))
            EnterGame();
    }
}

public partial class SelectManager { // 차 관련
    // 그래픽적 요소?
    private SpriteRenderer car;
    private float carVelocity = 0;
    private float MAX_VELOCITY = 10f;
    private float ACCELARATE = 15f;

    private void Update() {
        ReadKey();

        Vector3 currentPosition = car.transform.position;
        Vector3 targetPosition;
        if(SelectedIndex < 0) targetPosition = buttons[0].transform.position + Vector3.left * 10f;
        else if(SelectedIndex >= LevelCount) 
            targetPosition = buttons[LevelCount - 1].transform.position + Vector3.right * 10f;
        else targetPosition = buttons[SelectedIndex].transform.position;

        int direction = (int)Mathf.Sign(targetPosition.x - car.transform.position.x);

        // 가까워지면 느려진다. (부드럽게 멈추기)
        if(Mathf.Abs((currentPosition - targetPosition).x) <= carVelocity * carVelocity / ACCELARATE * 0.5f) {
            if(carVelocity * direction < 0) carVelocity *= 0.5f;
            else carVelocity -= ACCELARATE * direction * Time.deltaTime;
        } 
        else if(carVelocity * direction < 0 && Mathf.Abs((currentPosition - targetPosition).x) < 0.05f) carVelocity *= 0.5f;
        else carVelocity += ACCELARATE * direction * Time.deltaTime;
            
        // 최대 속도
        if(Mathf.Abs(carVelocity) > MAX_VELOCITY) carVelocity = MAX_VELOCITY * direction;
        
        currentPosition.x += carVelocity * Time.deltaTime;
        car.transform.position = currentPosition;
    }
}