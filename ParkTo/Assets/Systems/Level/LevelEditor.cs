using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LevelBase))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawScriptField();
        LevelBase level = (LevelBase)target;
        SerializedObject serializedObject = new SerializedObject(target);

        level.id = EditorGUILayout.TextField("Id", level.id);
        level.description = EditorGUILayout.TextField("Description", level.description);
        EditorGUILayout.Space();

        level.size = EditorGUILayout.Vector2IntField("Size", level.size);
        level.size.x = Mathf.Clamp(level.size.x, 0, int.MaxValue);
        level.size.y = Mathf.Clamp(level.size.y, 0, int.MaxValue);

        if (GUI.changed || level.tiles == null)
        {
            level.tiles = new LevelBase.TileList[level.size.y];
            for (int y = 0; y < level.size.y; y++)
            {
                level.tiles[y].tile = new LevelBase.TileData[level.size.x];
                for (int x = 0; x < level.size.x; x++)
                    level.tiles[y].tile[x] = new LevelBase.TileData() { type = LevelBase.TileType.Normal };
            }
        }
        
        EditorGUILayout.BeginVertical();
        for (int y = level.size.y - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < level.size.x; x++)
            {
                EditorGUILayout.BeginVertical();

                level.tiles[y].tile[x].type = (LevelBase.TileType)EditorGUILayout.EnumPopup(level.tiles[y].tile[x].type, GUILayout.MinWidth(20));
                if(level.tiles[y].tile[x].type != LevelBase.TileType.Empty && level.tiles[y].tile[x].type != LevelBase.TileType.Normal)
                    level.tiles[y].tile[x].data = EditorGUILayout.IntField(level.tiles[y].tile[x].data, GUILayout.MinWidth(20));

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggers"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cars"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("decorates"), true);
        level.seed = EditorGUILayout.IntField("Seed", level.seed);

        EditorGUILayout.Space();
        EditorUtility.SetDirty(target);
    }

    public void OnDestroy()
    {
        EditorUtility.SetDirty(target);
    }

    private void DrawScriptField()
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((LevelBase)target), typeof(LevelBase), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
    }
}
#endif
