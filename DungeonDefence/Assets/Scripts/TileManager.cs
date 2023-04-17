using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [HideInInspector] public Vector2Int tileCount;
    [HideInInspector] public Vector3 tileSize;
    [HideInInspector] public TileData[] Tilemap_Bottom = new TileData[0];
    [HideInInspector] public TileData[] Tilemap_Upper = new TileData[0];

    // 타일이 설치 될 게임 오브젝트
    public GameObject stage = null;

    // 공격 유닛 방향
    public Vector3 OffenceDirection;
    // 방어 유닛 방향
    public Vector3 DefenceDirection;

    [SerializeField, HideInInspector] protected GameObject tileContainer_Bottom = null;
    [SerializeField, HideInInspector] protected GameObject tileContainer_Upper = null;
    [SerializeField, HideInInspector] protected Tile[] tiles_Bottom = new Tile[0];
    [SerializeField, HideInInspector] protected Tile[] tiles_Upper = new Tile[0];
    [SerializeField, HideInInspector] protected Tile startTile = null;
    [SerializeField, HideInInspector] protected Tile finishTile = null;

    public bool IsReady { get; private set; }

    void Awake()
    {
        // 스테이지 게임오브젝트가 없다면 작동 불가
        if (!stage)
            return;

        // 바닥 타일 생성/배치
        if (GenerateBottomTiles() == false)
            return;

        // 상단 타일 생성/배치
        if (GenerateUpperTiles() == false)
            return;

        IsReady = true;
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
        return IsReady;
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
        if (tileContainer_Bottom)
        {
            DestroyImmediate(tileContainer_Bottom);
            tileContainer_Bottom = null;
        }
    }

    public void ResetTiles()
    {
        if (tileContainer_Upper)
        {
            DestroyImmediate(tileContainer_Upper);
            tileContainer_Upper = null;
        }
    }

    // 바닥 타일 생성
    public bool GenerateBottomTiles()
    {
        // 컨테이너 생성
        if (tileContainer_Bottom == null)
        {
            // 검색 우선
            Transform tf = stage.transform.Find("Bottom");
            if (tf != null)
            {
                tileContainer_Bottom = tf.gameObject;
            }
            else
            {
                tileContainer_Bottom = new("Bottom");
            }

            if (tileContainer_Bottom == null)
                return false;

            // 연결
            tileContainer_Bottom.transform.SetParent(stage.transform);
            tileContainer_Bottom.layer = LayerMask.NameToLayer("Tile_Bottom");
        }
        // pivot 이 바닥 가운데 인 것이 기준이므로 바닥이 될 타일은 0 점 아래로 깔려야 합니다.
        tileContainer_Bottom.transform.position = new Vector3(0.0f, -tileSize.y, 0.0f);

        GenerateTileMap(tileContainer_Bottom, ref Tilemap_Bottom, ref tiles_Bottom);

        return true;
    }

    // 상단 타일 생성
    public bool GenerateUpperTiles()
    {
        // 컨테이너 생성
        if (tileContainer_Upper == null)
        {
            // 검색 우선
            Transform tf = stage.transform.Find("Upper");
            if (tf != null)
            {
                tileContainer_Upper = tf.gameObject;
            }
            else
            {
                tileContainer_Upper = new("Upper");
            }

            if (tileContainer_Upper == null)
                return false;

            // 연결
            tileContainer_Upper.transform.SetParent(stage.transform);
            tileContainer_Upper.layer = LayerMask.NameToLayer("Tile_Upper");
        }

        // pivot 이 바닥 가운데 인 것이 기준이므로 상단 타일은 바닥이 0이면 됩니다.
        tileContainer_Upper.transform.localPosition = Vector3.zero;

        GenerateTileMap(tileContainer_Upper, ref Tilemap_Upper, ref tiles_Upper);

        return true;
    }

    // 타일 맵 생성
    protected void GenerateTileMap(GameObject container, ref TileData[] tileMap, ref Tile[] tileList)
    {
        // 타일 전체 갯수
        int tileTotal = tileCount.x * tileCount.y;

        // 이전 타일 갱신
        if (tileList.Length > tileTotal)
        {
            for (int i = tileTotal - 1; i < tileList.Length; ++i)
            {
                // 제거된 타일이나 초과된 타일 제거
                if (tileList[i])
                {
                    tileList[i].transform.SetParent(null);
                    DestroyImmediate(tileList[i]);
                    tileList[i] = null;
                }
            }
        }
        System.Array.Resize(ref tileList, tileTotal);

        // 타일 생성/이동
        int index = -1;
        for (int v = 0; v < tileCount.x; ++v)
        {
            for (int h = 0; h < tileCount.y; ++h)
            {
                // 타일 인덱스
                ++index;

                // 데이터 변경 시 우선 제거
                if (tileList[index] && (tileMap[index] == null || tileList[index].TileData != tileMap[index]))
                {
                    tileList[index].transform.SetParent(null);
                    DestroyImmediate(tileList[index]);
                    tileList[index] = null;
                }

                // 빈 칸이면 패스
                if (tileMap[index] == null)
                    continue;

                // 프리팹 가져오기
                GameObject tilePrefab = tileMap[index].Prefab;

                // 기본 크기 계산
                Bounds bounds = tilePrefab ? tilePrefab.GetComponent<MeshRenderer>().localBounds : new Bounds(Vector3.zero, tileSize);
                Vector3 baseScale = new(tileSize.x / bounds.size.x, tileSize.y / bounds.size.y, tileSize.z / bounds.size.z);

                // 타일 생성
                Tile tile = tileList[index] ? tileList[index] : tileMap[index].CreateTile(tileSize);
                if (tile)
                {
                    GameObject tileObj = tile.gameObject;
                    tileObj.name = "Tile_" + index;
                    tileObj.layer = tile.TileData.UseCustomLayer ? tile.TileData.CustomLayer : container.layer;            // 레이어 셋팅
                    tileObj.transform.SetParent(container.transform);
                    tileObj.transform.localScale = baseScale;
                    tileObj.transform.SetLocalPositionAndRotation(GetTilePosition(index, false, tileObj), tilePrefab ? tilePrefab.transform.rotation : Quaternion.identity);
                    tileObj.isStatic = true;                    // 바닥 타일은 정적으로 고정

                    tile.TileIndex = index;

                    tileList[index] = tile;
                }
            }
        }
    }

    // 타일 파괴하기
    public void NotifyBreakTile(Tile tile)
    {
        // 바로 파괴
        if (tile)
        {
            tile.Break();
        }
    }

    // 주변 타일 찾기
    // tileIndex : 현재 위치 인덱스
    // dir : 이동하고자 하는 방향
    // unit : 이동하고자 하는 유닛
    public int GetTileIndexAround(int tileIndex, Vector3 dir)
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

        // 범위를 벗어난 인덱스
        if (nextIndex < 0 || nextIndex >= (tileCount.x * tileCount.y))
            return -1;

        return nextIndex;
    }

    public Tile GetTile(int tileIndex)
    {
        if (tileIndex > -1 && tiles_Upper.Length > tileIndex && tiles_Upper[tileIndex] && tiles_Upper[tileIndex] != null)
        {
            return tiles_Upper[tileIndex].GetComponent<Tile>();
        }

        return null;
    }

    // 타일 위치 반환. 로컬 좌표임
    // 20 21 22 23 24
    // 15 16 17 18 19
    // 10 11 12 13 14
    // 05 06 07 08 09
    // 00 01 02 03 04
    public Vector3 GetTilePosition(int tileIndex, bool isUnitPosition = false, GameObject obj = null)
    {
        float x = ((tileIndex % tileCount.x) * tileSize.x) - (tileSize.x * tileCount.x * 0.5f) + (tileSize.x * 0.5f);
        float z = ((tileIndex / tileCount.y) * tileSize.z) - (tileSize.z * tileCount.y * 0.5f) + (tileSize.z * 0.5f);

        Vector3 position = new Vector3(x, 0.0f, z);
        if (obj && !isUnitPosition)
        {
            BoxCollider box = obj.GetComponent<BoxCollider>();
            position.y = ((box.size.y * 0.5f) - box.center.y) * obj.transform.localScale.y;
        }
        else
        {
            //position.y += 0.1f;
        }

        return position;
    }
}
