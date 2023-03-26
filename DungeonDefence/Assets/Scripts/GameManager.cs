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

    [SerializeField] protected List<WaveSpawnData> waveList = new();

    public TileManager TileManager { get; set; }
    public Inventory Inventory { get; set; }

    private List<GameObject> enemyList = new();


    // Start is called before the first frame update
    void Awake()
    {
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
        Debug.Log("IsGameStartable : " + TileManager + " / " + Inventory);
        // 타일관리자 유효성 확인
        if (!TileManager || TileManager.IsGameStartable() == false)
            return false;

        if (!Inventory)
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
        Tile startTile = TileManager.GetStartTile();

        foreach (GameObject unitPrefab in waveData.unitPrefabList)
        {
            if (unitPrefab != null)
            {
                GameObject unit = SpawnUnitOnTile(unitPrefab, startTile);
                if (unit != null)
                {
                    enemyList.Add(unit);

                    Unit script = unit.GetComponent<Unit>();
                    if (script)
                    {
                        script.Activate();
                    }
                }
            }

            yield return new WaitForSeconds(waveData.delay);
        }

        yield break;
    }

    public GameObject SpawnUnitOnTile(GameObject unitPrefab, Tile tile)
    {
        // 유닛 생성
        Vector3 spawnPosition = TileManager.GetTilePosition(tile.TileIndex, true);
        spawnPosition.y += (unitPrefab.GetComponent<CapsuleCollider>().height * 0.5f);

        GameObject unit = GameObject.Instantiate<GameObject>(
            unitPrefab,
            spawnPosition,
            tile.transform.rotation);

        if (unit)
        {
            Unit script = unit.GetComponent<Unit>();
            if (script)
            {
                script.TileIndex = tile.TileIndex;
                script.Direction = tile.transform.forward;
            }
        }

        return unit;
    }

    // 유닛이 목적지에 도착함
    public void UnitReachedFinish(Unit enemy)
    {
        Destroy(enemy.gameObject);
    }
}

