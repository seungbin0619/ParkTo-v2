using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Name", menuName = "Level")]
public class LevelBase : ScriptableObject
{
    public enum TileType
    {
        Empty = 0x00,
        Goal = 0x01,
        Trigger = 0x10,
        Normal = 0x11
    }

    public enum TriggerType
    {
        NORMAL = -1,
        BAN,
        GOAL,
        TURNLEFT,
        TURNRIGHT,
        STOP,
        BACKWARD,
    }

    [System.Serializable]
    public struct TileList
    {
        public TileData[] tile;
    }

    [System.Serializable]
    public struct TileData
    {
        public TileType type;
        public int data;
    }

    [System.Serializable]
    public struct CarData
    {
        public Vector2Int position;
        public int rotation;
    }

    public string id;
    public string description;

    public Vector2Int size;
    public TileList[] tiles; // 레벨 데이터

    public TriggerType[] triggers;
    public CarData[] cars;

    public int seed;
}