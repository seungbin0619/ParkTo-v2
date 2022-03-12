using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    private const int MAX_COUNT = 8; // �� �������� ���� �� �ִ� �� ����
    private const int RANGE = 15; // �� �������� Ÿ�ϻ����� ǥ�õ� �� ����(����)

    [SerializeField]
    private Tilemap selectTile;

    [SerializeField]
    private Transform linePanel;

    private List<Button> buttons = new List<Button>();
    private List<Mask> lines = new List<Mask>();

    private int Page { set; get; }

    private void Awake()
    {
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
    }

    private void DrawLevels(int page = 0)
    {
        if (ThemeManager.currentTheme == null) return;
        if(page < 0 || ThemeManager.currentTheme.levels.Count < page * MAX_COUNT + 1)
        {
            int sign = (int)Mathf.Sign(page);
            ThemeManager.instance.SetTheme(ThemeManager.index + sign);

            return;
        }

        Page = page;
        for(int i = 0; i < MAX_COUNT; i++)
        {
            int levelIndex = page * MAX_COUNT + i;
            if (ThemeManager.currentTheme.levels.Count > levelIndex) // ���ĺ��� �Ⱥ�����.
            {
                buttons[i].gameObject.SetActive(true);
                if(i > 0) lines[i - 1].gameObject.SetActive(true);
            }

            bool flag = true;

            buttons[i].interactable = flag;
            if (i > 0) lines[i - 1].enabled = flag;
        }

        Vector3Int position = new Vector3Int(-RANGE, 0, 0);
        for (int i = -RANGE; i <= RANGE; i++)
        {
            selectTile.SetTile(position, ThemeManager.currentTheme.grounds[0]);
            position.x++;
        }
    }
}
