using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct WaveSpawnData
    {
        public float delay;
        public List<GameObject> unitPrefabList;
    }

    [HideInInspector] public Vector2Int tileCount;
    [HideInInspector] public Vector3 tileSize;
    [HideInInspector] public List<GameObject> tiles;

    [Header("Setting")]
    [SerializeField] protected GameObject floor;
    [SerializeField] protected List<WaveSpawnData> waveList = new();
    [SerializeField] protected Vector3 SpawnDirection;

    private bool IsPlayable { get; set; } = false;
    private GameObject _startTile = null;
    private GameObject _finishTile = null;
    private List<GameObject> _tileList = new();
    private List<GameObject> _enemyList = new();

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("[GameManager : Awake]");

        // 최초 타일 생성/배치
        if (SpawnTiles() == false)
            return;

        IsPlayable = true;
    }

    void Start()
    {
        if (IsPlayable == false)
            return;

        Debug.Log("[Start]");

        // 웨이브 시작
        StartWave(0);
    }

    // Update is called once per frame
    void Update()
    {

    }


    // 게임 시작 전 스테이지 블럭 배치
    private bool SpawnTiles()
    {
        // 바닥 생성
        if (floor == null)
        {
            floor = new GameObject("Floor");
            floor.transform.position = Vector3.zero;
            floor.transform.rotation = Quaternion.identity;
        }

        // 기존 타일 제거
        _startTile = null;
        _finishTile = null;
        foreach (GameObject tile in _tileList)
        {
            GameObject.Destroy(tile);
        }
        _tileList.Clear();

        // 신규 타일 생성
        int tileIndex = -1;

        // pivot 이 바닥 가운데이므로 타일의 반만큼 더 이동해야 한다.
        float DefaultPosition_X = tileCount.x * tileSize.x * 0.5f - (tileSize.x * 0.5f);
        float DefaultPosition_Z = tileCount.y * tileSize.z * 0.5f - (tileSize.z * 0.5f);

        // 반복으로 타일 붙이기
        for (int v = 0; v < tileCount.x; ++v)
        {
            for (int h = 0; h < tileCount.y; ++h)
            {
                GameObject tileTemplate = tiles[++tileIndex];
                GameObject tile = null;

                if (tileTemplate != null)
                {
                    // 배치위치 계산
                    Vector3 spawnPosition = new(
                        h * tileSize.x - DefaultPosition_X,
                        0.1f,
                        -v * tileSize.z + DefaultPosition_Z);

                    // 타일 생성
                    tile = GameObject.Instantiate(tileTemplate, spawnPosition, Quaternion.identity, floor.transform);
                    if (tile != null)
                    {
                        Collider tileCollider = tile.GetComponent<Collider>();
                        if (tileCollider != null)
                        {
                            Vector3 NewScale = new(
                                tileSize.x / tileCollider.bounds.size.x,
                                tileSize.y / tileCollider.bounds.size.y,
                                tileSize.z / tileCollider.bounds.size.z);
                            tile.transform.localScale = NewScale;
                        }

                        // 시작 타일 갱신
                        if (tile.CompareTag("Start"))
                        {
                            _startTile = tile;
                        }

                        // 종료 타일 갱신
                        else if (tile.CompareTag("Finish"))
                        {
                            _finishTile = tile;
                        }
                    }
                }

                _tileList.Add(tile);
            }
        }

        Debug.Log("[GameManager : SpawnTiles] " + _startTile + " -> " + _finishTile);

        // 생성된 타일 갯수가 맞지 않으면 실패
        if (_tileList.Count != (tileCount.x * tileCount.y))
            return false;

        // 시작/종료 타일이 없으면 실패
        if (_startTile == null || _finishTile == null)
            return false;

        return true;
    }

    // 웨이브 생성
    bool StartWave(int waveIndex)
    {
        Debug.Log("[StartWave] " + waveIndex);
        if (waveList.Count > waveIndex)
        {
            StartCoroutine(SpawnWave(waveList[waveIndex]));
        }

        Debug.Log("[StartWave] end");

        return true;
    }

    // 웨이브 생성하기. 코루틴 반복용
    IEnumerator SpawnWave(WaveSpawnData waveData)
    {
        foreach (GameObject unitPrefab in waveData.unitPrefabList)
        {
            if (_startTile == null)
                break;

            Debug.Log("\tInstantiate [" + unitPrefab + "] " + unitPrefab.transform.forward + " / " + _startTile.transform.forward + " / " + Vector3.forward);

            if (unitPrefab != null)
            {
                // Start Rotation 은 어떻게 가져올까
                GameObject unit = GameObject.Instantiate<GameObject>(
                    unitPrefab,
                    _startTile.transform.position,
                    _startTile.transform.rotation);

                if (unit != null)
                {
                    _enemyList.Add(unit);
                }
            }

            yield return new WaitForSeconds(waveData.delay);
        }

        Debug.Log("[GameManager : SpawnWave] End");
        yield break;
    }
}

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    GameManager _source = null;
    GUIStyle _headerStyle = null;
    bool m_FoldOut = false;

    void OnEnable()
    {
        _source = target as GameManager;
        _headerStyle = new GUIStyle(EditorStyles.label);
        // _headerStyle.fontSize = 16;
        _headerStyle.fontStyle = FontStyle.Bold;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        m_FoldOut = EditorGUILayout.Foldout(m_FoldOut, "Tile Settings");
        if (m_FoldOut)
        {
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.window));

            // EditorGUI.indentLevel++;

            _source.tileCount = EditorGUILayout.Vector2IntField("Tile Count", _source.tileCount);
            _source.tileSize = EditorGUILayout.Vector3Field("Tile Size", _source.tileSize);

            int tilesMax = _source.tileCount.x * _source.tileCount.y;
            if (_source.tiles.Count > tilesMax)
            {
                _source.tiles.RemoveAt(tilesMax);
            }
            else if (_source.tiles.Count < tilesMax)
            {
                _source.tiles.AddRange(new GameObject[tilesMax - _source.tiles.Count]);
            }

            int TileIndex = 0;
            for (int i = 0; i < _source.tileCount.x; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                for (int j = 0; j < _source.tileCount.y; ++j)
                {
                    _source.tiles[TileIndex] = EditorGUILayout.ObjectField(_source.tiles[TileIndex], typeof(GameObject), false) as GameObject;
                    TileIndex++;
                }

                EditorGUILayout.EndHorizontal();
            }

            // EditorGUI.indentLevel--;

            GUILayout.EndVertical();
        }
    }
}
