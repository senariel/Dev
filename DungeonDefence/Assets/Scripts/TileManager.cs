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

    [SerializeField, HideInInspector] protected GameObject floorTileContainer = null;
    [SerializeField, HideInInspector] protected GameObject[] floorTiles = new GameObject[0];
    [SerializeField, HideInInspector] protected GameObject tileContainer = null;
    [SerializeField, HideInInspector] protected GameObject[] tiles = new GameObject[0];
    [SerializeField, HideInInspector] protected Tile startTile = null;
    [SerializeField, HideInInspector] protected Tile finishTile = null;
    private bool isReady = false;

    void Awake()
    {
        if (GenerateFloorTiles() == false)
            return;

        // 최초 타일 생성/배치
        if (GenerateTiles() == false)
            return;

        isReady = true;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // 타일이 정상 설치되었는지 확인
    public bool IsGameStartable()
    {
        return isReady;
    }

    // 시작 타일 반환
    public Tile GetStartTile()
    {
        return startTile;
    }

    // 최종 목표 타일 반환
    public Tile GetFinishTile()
    {
        return finishTile;
    }

    public void ResetFloorTiles()
    {
        if (floorTileContainer)
        {
            DestroyImmediate(floorTileContainer);
            floorTileContainer = null;
        }
    }

    public void ResetTiles()
    {
        if (tileContainer)
        {
            DestroyImmediate(tileContainer);
            tileContainer = null;
        }
    }

    // 바닥 타일 갱신
    public bool GenerateFloorTiles()
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
                Debug.Log("[TileManager : UpdateFloorTiles] container is null but i found : " + floorTilePrefab);
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
        floorTileContainer.transform.position = new Vector3(0.0f, tileSize.y / -2.0f, 0.0f);

        // 타일 전체 갯수
        int tileTotal = tileCount.x * tileCount.y;

        Debug.Log("[TileManager : UpdateFloorTiles] " + floorTiles.Length);

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
            0.0f,
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

                    tile.transform.SetLocalPositionAndRotation(spawnPosition, floorTilePrefab.transform.rotation);
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

    // 타일 갱신
    public bool GenerateTiles()
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
        tileContainer.transform.localPosition = new Vector3(0.0f, tileSize.y / 2.0f, 0.0f);

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
                        0.0f,
                        tileCount.y * tileSize.z * 0.5f - (tileSize.z * 0.5f));

                    // 타일 생성
                    GameObject tile = null;

                    // 기존 타일 확인
                    if (tiles[index])
                    {
                        // 다른 타일이라면 기존 타일 제거
                        if (tilePrefab.CompareTag(tiles[index].tag) == false)
                        {
                            Debug.Log("\t\tDiff Tag");

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

                    tile.transform.SetLocalPositionAndRotation(spawnPosition, tilePrefab.transform.rotation);
                    tile.transform.localScale = baseScale;

                    // 타일은 정적으로 고정
                    tile.isStatic = true;

                    // 시작 위치 갱신
                    if (tile.CompareTag("Tile_Start"))
                    {
                        startTile = tile.GetComponent<Tile>();
                    }
                    else if (tile.CompareTag("Tile_Finish"))
                    {
                        finishTile = tile.GetComponent<Tile>();
                    }
                    else
                    {
                        Tile tileScript = tile.GetComponent<Tile>();
                        if (tileScript)
                        {
                            // 일반 타일은 파괴 가능
                            tileScript.breakable = true;
                        }
                    }

                    tiles[index] = tile;
                }
                else
                {
                    // 빈 타일인데 기존 타일이 있다면 제거
                    if (tiles[index])
                    {
                        DestroyImmediate(tiles[index]);
                        tiles[index] = null;
                    }
                }
            }
        }

        return (startTile && finishTile);
    }

    // 타일 파괴하기
    public void BreakTile(Tile tile)
    {
        if (tile)
        {
            StartCoroutine(PlayBreakTile(tile));
            Destroy(tile.gameObject);
        }
    }

    // 타일 파괴 연출
    private IEnumerator PlayBreakTile(Tile tile)
    {
        if (tile && tile.breakFX)
        {
            GameObject FXObj = Instantiate(tile.breakFX, tile.transform.position, tile.breakFX.transform.rotation);
            if (FXObj)
            {
                ParticleSystem fx = FXObj.GetComponent<ParticleSystem>();
                if (fx)
                {
                    fx.Play(true);

                    yield return new WaitForSeconds(fx.main.duration);

                    fx.Stop();
                    Destroy(FXObj);
                }
            }
        }

        yield break;
    }
}

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

        isFoldOut = EditorGUILayout.Foldout(isFoldOut, "Tile Settings");
        if (isFoldOut)
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.window));

            // EditorGUI.indentLevel++;

            System.Array.Resize(ref script.tilePrefabs, script.tileCount.x * script.tileCount.y);

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

        // 초기화/생성 버튼 추가
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Tiles"))
        {
            script.ResetTiles();
        }
        if (GUILayout.Button("Reset All Tiles"))
        {
            script.ResetFloorTiles();
            script.ResetTiles();
        }
        if (GUILayout.Button("Generate Tiles"))
        {
            // 바닥 타일 갱신
            script.GenerateFloorTiles();

            // 타일 갱신
            script.GenerateTiles();
        }
        EditorGUILayout.EndHorizontal();
    }
}
