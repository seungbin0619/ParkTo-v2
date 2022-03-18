using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Theme Name", menuName = "Theme")]
public class ThemeBase : ScriptableObject
{
    public string id;
    public int index;

    public Color[] colors;
    public Color[] cars;

    public Tile[] grounds;
    public List<LevelBase> levels;
}
