using UnityEngine.Tilemaps;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
# endif

public class ObjectTile : Tile
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/ObjectTile")]
    public static void CreateRoadTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Tile", "New Tile", "Asset", "Save Tile", "Assets");
        if (path == "") return;
        AssetDatabase.CreateAsset(CreateInstance<ObjectTile>(), path);
    }
#endif
}