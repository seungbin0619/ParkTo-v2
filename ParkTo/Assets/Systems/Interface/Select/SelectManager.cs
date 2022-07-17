using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SelectManager : SingleTon<SelectManager>
{
    public static int delta = 1; // 어디서 왔는지?
    public static int lastSelectedLevel = -1;

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

    // 그래픽적 요소?
    private SpriteRenderer car;
    private float carVelocity = 0;
    private float MAX_VELOCITY = 10f;
    private float ACCELARATE = 15f;

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
        DrawLevels(0);

        if(lastSelectedLevel == -1) SelectIndex(delta == 1 ? 0 : LevelCount - 1);
        else SelectIndex(lastSelectedLevel);

        Vector3 targetPosition = buttons[SelectedIndex].transform.position;
        targetPosition.x -= delta;
        //carVelocity = MAX_VELOCITY * delta;

        car.transform.position = targetPosition;

        lastSelectedLevel = -1;
    }

    public void SelectIndex(int position) {
        if(entered) return;
        if(position < 0) position = 0;
        else if(position >= LevelCount) position = LevelCount - 1;

        SelectedIndex = position;
    }

    public void EnterGame() {
        if(entered) return;

        entered = true;
        GameManager.SelectedLevel = SelectedIndex + Page * MAX_COUNT;
        SettingManager.instance.Goto("Game");
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) 
            SelectIndex(SelectedIndex - 1);
        else if(Input.GetKeyDown(KeyCode.RightArrow)|| Input.GetKeyDown(KeyCode.D)) 
            SelectIndex(SelectedIndex + 1);
        
        if(Input.GetKeyDown(KeyCode.Space) ||Input.GetKeyDown(KeyCode.Return) ||Input.GetKeyDown(KeyCode.KeypadEnter))
            EnterGame();

        Vector3 currentPosition = car.transform.position;
        Vector3 targetPosition = buttons[SelectedIndex].transform.position;
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

    private void DrawLevels(int page = 0)
    {
        if (ThemeManager.currentTheme == null) return;
        if(page < 0 || ThemeManager.currentTheme.levels.Count < page * MAX_COUNT + 1) // 페이지 이동
        {
            int sign = (int)Mathf.Sign(page); // 페이지가 0 아래면 전으로
            delta = sign;

            ThemeManager.instance.SetTheme(ThemeManager.index + sign);
            // 페이지 리로드?

            return;
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
    }
}
