using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TileManager : MonoBehaviour
{
    [HideInInspector] public Vector2Int tileCount;
    [HideInInspector] public Vector3 tileSize;
    [HideInInspector] public GameObject[] tilePrefabs = new GameObject[0];

    // 최하단 타일(파괴 불가, 걷기용)
    public GameObject floorTilePrefab = null;

    private GameObject floorTileContainer = null;
    private GameObject[] floorTiles = new GameObject[0];
    private GameObject tileContainer = null;
    private GameObject[] tiles = new GameObject[0];
    private GameObject _startTile = null;
    private GameObject _finishTile = null;

    void AWake()
    {
        Debug.Log("[TileManager : AWake]");

        UpdateFloorTiles();

        // 최초 타일 생성/배치
        UpdateTiles();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool IsGameStartable()
    {
        return false;
    }

    public Transform GetStartTransform()
    {
        return _startTile?.transform;
    }

    public bool UpdateFloorTiles()
    {
        if (floorTilePrefab == null)
            return false;

        // 컨테이너 생성
        if (floorTileContainer == null)
        {
            // 검색 우선
            Transform tf = transform.Find("Bottom");
            if (tf != null)
            {
                floorTileContainer = tf.gameObject;
            }
            else
            {
                floorTileContainer = new("Bottom");
            }

            if (floorTileContainer == null)
                return false;

            // 연결
            floorTileContainer.transform.SetParent(transform);
        }

        // 타일 전체 갯수
        int tileTotal = tileCount.x * tileCount.y;

        // 초과 타일 제거
        if (floorTiles.Length > tileTotal)
        {
            for (int i = tileTotal - 1; i < floorTiles.Length; ++i)
            {
                if (floorTiles[i])
                {
                    floorTiles[i].transform.SetParent(null);
                    DestroyImmediate(floorTiles[i]);
                    floorTiles[i] = null;
                }
            }
        }

        System.Array.Resize(ref floorTiles, tileTotal);

        // 타일 생성/이동
        // 기본 크기 계산
        BoxCollider tileCollider = floorTilePrefab.GetComponent<BoxCollider>();
        Vector3 baseScale = tileCollider ? new(
                            tileSize.x / (tileCollider.size.x / floorTilePrefab.transform.localScale.x),
                            tileSize.y / (tileCollider.size.y / floorTilePrefab.transform.localScale.y),
                            tileSize.z / (tileCollider.size.z / floorTilePrefab.transform.localScale.z)) : Vector3.one;

        // 기본 위치 계산. pivot 이 바닥 가운데이므로 타일의 반만큼 더 이동해야 한다.
        Vector3 basePosition = new(
            tileCount.x * tileSize.x * 0.5f - (tileSize.x * 0.5f),
            (tileCollider.size.y * baseScale.y) * -1.00f,
            tileCount.y * tileSize.z * 0.5f - (tileSize.z * 0.5f));

        int index = -1;
        for (int v = 0; v < tileCount.x; ++v)
        {
            for (int h = 0; h < tileCount.y; ++h)
            {
                // 타일 인덱스
                ++index;

                // 타일 생성
                GameObject tile = floorTiles[index] ? floorTiles[index] : GameObject.Instantiate(floorTilePrefab, floorTileContainer.transform);
                if (tile != null)
                {
                    tile.name = "BottomTile_" + index;

                    // 배치위치 계산
                    Vector3 spawnPosition = new(
                        (h * tileSize.x) - basePosition.x,
                        basePosition.y,
                        (-v * tileSize.z) + basePosition.z);

                    tile.transform.SetLocalPositionAndRotation(spawnPosition, Quaternion.identity);
                    tile.transform.localScale = baseScale;

                    // 바닥 타일은 정적으로 고정
                    tile.isStatic = true;

                    floorTiles[index] = tile;
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool UpdateTiles()
    {
        // 컨테이너 생성
        if (tileContainer == null)
        {
            // 검색 우선
            Transform tf = transform.Find("Tiles");
            if (tf != null)
            {
                tileContainer = tf.gameObject;
            }
            else
            {
                tileContainer = new("Tiles");
            }

            if (tileContainer == null)
                return false;

            // 연결
            tileContainer.transform.SetParent(transform);
        }

        // 타일 전체 갯수
        int tileTotal = tileCount.x * tileCount.y;

        // 초과 타일 프리팹 제거
        System.Array.Resize(ref tilePrefabs, tileTotal);

        // 초과 타일 제거
        if (tiles.Length > tileTotal)
        {
            for (int i = tileTotal - 1; i < tiles.Length; ++i)
            {
                if (tiles[i])
                {
                    tiles[i].transform.SetParent(null);
                    DestroyImmediate(tiles[i]);
                    tiles[i] = null;
                }
            }
        }
        System.Array.Resize(ref tiles, tileTotal);

        int index = -1;
        for (int v = 0; v < tileCount.x; ++v)
        {
            for (int h = 0; h < tileCount.y; ++h)
            {
                // 타일 인덱스
                ++index;

                GameObject tilePrefab = tilePrefabs[index];
                if (tilePrefab)
                {
                    // 타일 생성/이동
                    // 기본 크기 계산
                    BoxCollider tileCollider = tilePrefab.GetComponent<BoxCollider>();
                    Vector3 baseScale = tileCollider ? new(
                                        tileSize.x / (tileCollider.size.x / tilePrefab.transform.localScale.x),
                                        tileSize.y / (tileCollider.size.y / tilePrefab.transform.localScale.y),
                                        tileSize.z / (tileCollider.size.z / tilePrefab.transform.localScale.z)) : Vector3.one;

                    // 기본 위치 계산. pivot 이 바닥 가운데이므로 타일의 반만큼 더 이동해야 한다.
                    Vector3 basePosition = new(
                        tileCount.x * tileSize.x * 0.5f - (tileSize.x * 0.5f),
                        (tileCollider.size.y * baseScale.y) * -1.00f,
                        tileCount.y * tileSize.z * 0.5f - (tileSize.z * 0.5f));

                    // 타일 생성
                    GameObject tile = null;
                    // 기존 타일 확인
                    if (tiles[index])
                    {
                        // 다른 타일이라면 기존 타일 제거
                        if (Object.ReferenceEquals(tilePrefab, tiles[index]) == false)
                        {
                            DestroyImmediate(tiles[index]);
                            tiles[index] = null;
                        }
                        else
                        {
                            tile = tiles[index];
                        }
                    }

                    // 타일 생성
                    if (!tile)
                    {
                        tile = GameObject.Instantiate(tilePrefab, tileContainer.transform);
                    }

                    // 실패
                    if (!tile)
                    {
                        return false;
                    }

                    tile.name = "Tile_" + index;

                    // 배치위치 계산
                    Vector3 spawnPosition = new(
                        (h * tileSize.x) - basePosition.x,
                        basePosition.y,
                        (-v * tileSize.z) + basePosition.z);

                    tile.transform.SetLocalPositionAndRotation(spawnPosition, Quaternion.identity);
                    tile.transform.localScale = baseScale;

                    // 타일은 정적으로 고정
                    tile.isStatic = true;

                    tiles[index] = tile;
                }
                else
                {
                    if (tiles[index])
                    {
                        DestroyImmediate(tiles[index]);
                        tiles[index] = null;
                    }
                }
            }
        }

        return true;
    }
}

[CustomEditor(typeof(TileManager))]
public class TileManagerEditor : Editor
{
    private TileManager script = null;
    bool isFoldOut = false;

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

        // 바닥 타일 갱신
        script.UpdateFloorTiles();

        // 타일 갱신
        script.UpdateTiles();

        isFoldOut = EditorGUILayout.Foldout(isFoldOut, "Tile Settings");
        if (isFoldOut)
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.window));

            // EditorGUI.indentLevel++;

            int TileIndex = 0;
            for (int i = 0; i < script.tileCount.x; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                for (int j = 0; j < script.tileCount.y; ++j)
                {
                    script.tilePrefabs[TileIndex] = EditorGUILayout.ObjectField(script.tilePrefabs[TileIndex], typeof(GameObject), false) as GameObject;
                    TileIndex++;
                }

                EditorGUILayout.EndHorizontal();
            }

            // EditorGUI.indentLevel--;

            GUILayout.EndVertical();
        }
    }
}
