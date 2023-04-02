using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(TileManager))]
public class TileManagerEditor : Editor
{
    private TileManager script = null;
    private bool isFoldOut = false;
    private GameObject[] tileMap = new GameObject[0];

    void OnEnable()
    {
        script = target as TileManager;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        // 갯수 및 사이즈
        script.tileCount = EditorGUILayout.Vector2IntField("TileMap Size", script.tileCount);
        script.tileSize = EditorGUILayout.Vector3Field("Tile Size", script.tileSize);

        isFoldOut = EditorGUILayout.Foldout(isFoldOut, "Tile Map");
        if (isFoldOut)
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.window));

            // EditorGUI.indentLevel++;

            System.Array.Resize(ref script.tilePrefabs, script.tileCount.x * script.tileCount.y);

            // 역순으로 돌아야 한다.
            for (int i = 0; i < script.tileCount.x; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                int index = (script.tileCount.y - i - 1) * script.tileCount.x;
                for (int j = 0; j < script.tileCount.y; ++j)
                {
                    script.tilePrefabs[index + j] = EditorGUILayout.ObjectField(script.tilePrefabs[index + j], typeof(GameObject), false) as GameObject;
                }

                EditorGUILayout.EndHorizontal();
            }

            // EditorGUI.indentLevel--;

            GUILayout.EndVertical();
        }

        // 초기화/생성 버튼 추가
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Tiles"))
        {
            script.ResetTiles();
            EditorUtility.SetDirty(script.stage);
        }
        if (GUILayout.Button("Reset All Tiles"))
        {
            script.ResetFloorTiles();
            script.ResetTiles();
            EditorUtility.SetDirty(script.stage);
        }
        if (GUILayout.Button("Generate Tiles"))
        {
            // 바닥 타일 갱신
            script.GenerateFloorTiles();

            // 타일 갱신
            script.GenerateTiles();
            
            EditorUtility.SetDirty(script.stage);
        }
        EditorGUILayout.EndHorizontal();
    }
}
