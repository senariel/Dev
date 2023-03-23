using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TileManager : MonoBehaviour
{
    [HideInInspector] public Vector2Int tileCount;
    [HideInInspector] public Vector3 tileSize;
    [HideInInspector] public GameObject[] tilePrefabs = new GameObject[0];

    // 타일이 설치 될 게임 오브젝트
    public GameObject stage = null;

    // 최하단 타일(파괴 불가, 걷기용)
    public GameObject floorTilePrefab = null;

    [SerializeField, HideInInspector] protected GameObject floorTileContainer = null;
    [SerializeField, HideInInspector] protected GameObject[] floorTiles = new GameObject[0];
    [SerializeField, HideInInspector] protected GameObject tileContainer = null;
    [SerializeField, HideInInspector] protected GameObject[] tiles = new GameObject[0];
    [SerializeField, HideInInspector] protected Tile startTile = null;
    [SerializeField, HideInInspector] protected Tile finishTile = null;

    protected GameManager gameManager = null;
    private bool isReady = false;

    void Awake()
    {
        if (!stage)
            return;

        GameObject gm = GameObject.Find("GameManager");
        if (!gm)
            return;

        gameManager = gm.GetComponent<GameManager>();
        if (gameManager)
        {
            gameManager.TileManager = this;
        }

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
            Transform tf = stage.transform.Find("Bottom");
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
            floorTileContainer.transform.SetParent(stage.transform);
            floorTileContainer.layer = LayerMask.NameToLayer("Tile_Block");
        }
        floorTileContainer.transform.position = new Vector3(0.0f, tileSize.y / -2.0f, 0.0f);

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
                    tile.transform.SetLocalPositionAndRotation(GetTilePosition(index), floorTilePrefab.transform.rotation);
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
            Transform tf = stage.transform.Find("Tiles");
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
            tileContainer.transform.SetParent(stage.transform);
            tileContainer.layer = LayerMask.NameToLayer("Tile_Block");
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

                    // 타일 생성
                    GameObject tile = null;

                    // 기존 타일 확인
                    if (tiles[index])
                    {
                        // 다른 타일이라면 기존 타일 제거
                        if (tilePrefab.CompareTag(tiles[index].tag) == false)
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

                    Tile tileScript = tile.GetComponent<Tile>();
                    if (!tileScript)
                    {
                        return false;
                    }

                    // 인덱스 설정
                    tile.name = "Tile_" + index;
                    tile.transform.SetLocalPositionAndRotation(GetTilePosition(index), tilePrefab.transform.rotation);
                    tile.transform.localScale = baseScale;
                    tile.isStatic = true;   // 타일은 정적으로 고정
                    tileScript.tileIndex = index;


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
                        // 일반 타일은 파괴 가능
                        tileScript.breakable = true;
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

    // 타일로 이동 할 수 있는지 여부
    // tileIndex : 현재 유닛의 위치 인덱스
    // dir : 이동하고자 하는 방향
    // unit : 이동하고자 하는 유닛
    public int CheckMovableTile(int tileIndex, Vector3 dir, Unit unit)
    {
        int nextIndex = -1;
        if ((dir - Vector3.forward).sqrMagnitude <= 0.01f)
        {
            nextIndex = tileIndex + tileCount.x;
        }
        else if ((dir - Vector3.back).sqrMagnitude <= 0.01f)
        {
            nextIndex = tileIndex - tileCount.x;
        }
        else if ((dir - Vector3.left).sqrMagnitude <= 0.01f)
        {
            nextIndex = (tileIndex % tileCount.x > 0) ? tileIndex - 1 : -1;
        }
        else if ((dir - Vector3.right).sqrMagnitude <= 0.01f)
        {
            nextIndex = (tileIndex % tileCount.x < (tileCount.x - 1)) ? tileIndex + 1 : -1;
        }

        // 타일 레이어 판정
        if (nextIndex > -1)
        {
            Tile tile = GetTile(nextIndex);
            if (tile)
            {
                if (LayerMask.LayerToName(tile.gameObject.layer) == "Tile_Block")
                {
                    nextIndex = -1;
                }
            }
        }

        return nextIndex;
    }

    public Tile GetTile(int tileIndex)
    {
        if (tiles.Length > tileIndex && tiles[tileIndex])
        {
            return tiles[tileIndex].GetComponent<Tile>();
        }

        return null;
    }

    // 타일 위치 반환. 로컬 좌표임
    // 20 21 22 23 24
    // 15 16 17 18 19
    // 10 11 12 13 14
    // 05 06 07 08 09
    // 00 01 02 03 04
    public Vector3 GetTilePosition(int tileIndex, bool isUnitPosition = false)
    {
        float x = ((tileIndex % tileCount.x) * tileSize.x) - (tileSize.x * tileCount.x * 0.5f) + (tileSize.x * 0.5f);
        float z = ((tileIndex / tileCount.y) * tileSize.z) - (tileSize.z * tileCount.y * 0.5f) + (tileSize.z * 0.5f);

        return new Vector3(x, 0.0f, z) + (isUnitPosition ? tileContainer.transform.position : Vector3.zero);
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
