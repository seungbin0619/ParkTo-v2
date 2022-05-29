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
        GOAL = -2,
        NORMAL = -1,
        BAN, // 트리거 설치 금지
        TURNLEFT, // 좌회전
        TURNRIGHT, // 우회전
        STOP, // 정지
        BACKWARD, // 후진
        SLOW, // 천천히
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
    public struct DecorateData
    {
        public GameObject decorate;
        public Vector2Int position;
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
    public DecorateData[] decorates;

    public CarData[] cars;
    public int seed;
}