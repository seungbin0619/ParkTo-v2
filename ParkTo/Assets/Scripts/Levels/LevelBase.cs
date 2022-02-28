using UnityEngine;

[CreateAssetMenu(fileName = "Level Name", menuName = "Level")]
public class LevelBase : ScriptableObject
{
    public enum TileType
    {
        None = 0x00,
        Normal = 0x11,
        Car = 0x01,
        Trigger = 0x10
    }

    [System.Serializable]
    public struct TileData
    {
        public TileType type;
        public int data;
    }

    public string id;
    public string description;

    public Vector2Int size;
    public TileData[,] tiles; // 레벨 데이터

    public int[] triggers;
    public int seed;
}
