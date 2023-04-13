using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(TileManager))]
public class TileManagerEditor : Editor
{
    private TileManager script = null;
    private bool isFoldOut_BottomTile = false;
    private bool isFoldOut_UpperTile = false;
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

        int tileCount = script.tileCount.x * script.tileCount.y;

        isFoldOut_BottomTile = EditorGUILayout.Foldout(isFoldOut_BottomTile, "Tile Map - Bottom");
        if (isFoldOut_BottomTile)
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.window));

            System.Array.Resize(ref script.Tilemap_Bottom, tileCount);

            for (int i = 0; i < script.tileCount.x; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                // 역순으로 돌아야 한다.
                int index = (script.tileCount.y - i - 1) * script.tileCount.x;
                for (int j = 0; j < script.tileCount.y; ++j)
                {
                    script.Tilemap_Bottom[index + j] = EditorGUILayout.ObjectField(script.Tilemap_Bottom[index + j], typeof(TileData), false) as TileData;
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        isFoldOut_UpperTile = EditorGUILayout.Foldout(isFoldOut_UpperTile, "Tile Map - Upper");
        if (isFoldOut_UpperTile)
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.window));

            // EditorGUI.indentLevel++;

            System.Array.Resize(ref script.Tilemap_Upper, tileCount);

            // 역순으로 돌아야 한다.
            for (int i = 0; i < script.tileCount.x; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                int index = (script.tileCount.y - i - 1) * script.tileCount.x;
                for (int j = 0; j < script.tileCount.y; ++j)
                {
                    script.Tilemap_Upper[index + j] = EditorGUILayout.ObjectField(script.Tilemap_Upper[index + j], typeof(TileData), false) as TileData;
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
            script.GenerateBottomTiles();

            // 타일 갱신
            script.GenerateUpperTiles();
            
            EditorUtility.SetDirty(script.stage);
        }
        EditorGUILayout.EndHorizontal();
    }
}
