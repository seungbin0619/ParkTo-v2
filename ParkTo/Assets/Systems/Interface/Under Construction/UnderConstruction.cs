using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public partial class UnderConstruction : MonoBehaviour
{
    private const int RANGE = 15;

    [SerializeField]
    private Tilemap selectTile;
    private bool entered = false;

    private void Start()
    {
        Draw();

        Vector3 targetPosition = Vector3.zero;
        targetPosition.x -= 1f;

        car.transform.position = targetPosition;

        SelectManager.LastSelectedLevel = -1;
    }

    private void Draw()
    {
        if(entered) return;
        if (ThemeManager.currentTheme == null) return;

        Vector3Int position = new Vector3Int(-RANGE, 0, 0);
        for (int i = -RANGE; i <= -1; i++)
        {
            selectTile.SetTile(position, ThemeManager.currentTheme.grounds[0]);
            position.x++;
        }
        selectTile.SetTile(position, ThemeManager.currentTheme.grounds[2]);
        selectTile.GetInstantiatedObject(position).transform.eulerAngles = new Vector3(0, 0, 90);

        car = Instantiate(ThemeManager.currentTheme.carBase);
        car.color = ThemeManager.currentTheme.cars[Random.Range(0, ThemeManager.currentTheme.cars.Length)];
        car.transform.rotation = Quaternion.Euler(0, 0, -90f);

        car.gameObject.AddComponent<CarSelect>();
    }

    private void ReadKey() {
        if(ActionManager.IsPlaying) return;
        
        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Return();
    }

    public void Return() {
        if(entered) return;

        entered = true;
        SettingManager.instance.Goto("Select");
    }
}

public partial class UnderConstruction { // 차 관련
    // 그래픽적 요소?
    private SpriteRenderer car;
    private float carVelocity = 0;
    private float MAX_VELOCITY = 10f;
    private float ACCELARATE = 15f;

    private void Update() {
        ReadKey();

        Vector3 currentPosition = car.transform.position;
        Vector3 targetPosition = entered ? Vector3.left * 10 : Vector3.zero;
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
