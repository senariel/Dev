using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct WaveSpawnData
    {
        public float delay;
        public List<GameObject> unitPrefabList;
    }

    public GameObject floor;

    [Header("Setting")]
    [SerializeField] protected List<WaveSpawnData> waveList = new();
    [SerializeField] protected Vector3 SpawnDirection;


    protected TileManager tileManager;
    private List<GameObject> enemyList = new();

    // Start is called before the first frame update
    void Awake()
    {
        tileManager = floor?.GetComponent<TileManager>();

        Debug.Log("[GameManager : Awake]");
    }

    void Start()
    {
        if (IsGameStartable() == false)
            return;

        StartStage();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public TileManager GetTileManager()
    {
        return tileManager;
    }

    // 스테이지 시작
    public void StartStage()
    {
        Debug.Log("[StartGame]");

        // 웨이브 시작
        StartWave(0);
    }

    // 게임 시작 가능 여부
    public bool IsGameStartable()
    {
        // 타일관리자 유효성 확인
        if (tileManager == null || tileManager.IsGameStartable() == false)
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
        Tile startTile = tileManager.GetStartTile();

        foreach (GameObject unitPrefab in waveData.unitPrefabList)
        {
            if (unitPrefab != null)
            {
                // 유닛 생성
                Vector3 spawnPosition = startTile.transform.position;
                spawnPosition.y -= tileManager.tileSize.y * 0.5f;

                GameObject unit = GameObject.Instantiate<GameObject>(
                    unitPrefab,
                    spawnPosition,
                    startTile.transform.rotation);

                if (unit != null)
                {
                    enemyList.Add(unit);
                }
            }

            yield return new WaitForSeconds(waveData.delay);
        }

        yield break;
    }

    // 유닛이 목적지에 도착함
    public void UnitReachedFinish(Unit enemy)
    {
        Destroy(enemy.gameObject);
    }
}

