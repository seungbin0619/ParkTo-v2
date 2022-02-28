using UnityEngine;
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

        if(GUI.changed || level.tiles == null) level.tiles = new LevelBase.TileData[level.size.y, level.size.x];

        EditorGUILayout.BeginVertical();
        for (int y = 0; y < level.size.y; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < level.size.x; x++)
            {
                EditorGUILayout.BeginVertical();

                level.tiles[y, x].type = (LevelBase.TileType)EditorGUILayout.EnumPopup(level.tiles[y, x].type, GUILayout.MinWidth(20));
                level.tiles[y, x].data = EditorGUILayout.IntField(level.tiles[y, x].data, GUILayout.MinWidth(20));

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggers"), true);
        level.seed = EditorGUILayout.IntField("Seed", level.seed);

        EditorGUILayout.Space();
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
