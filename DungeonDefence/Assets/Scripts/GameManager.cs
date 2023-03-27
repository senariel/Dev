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

    // 플레이어의 방어 유닛 카드?
    public GameObject bossUnit;
    public List<WaveSpawnData> waveList = new();
    // 웨이브 간 대기 시간
    public float waveDelay;

    public GameManager(TileManager tileManager, Inventory inventory)
    {
        this.TileManager = tileManager;
        this.Inventory = inventory;

    }
    public TileManager TileManager { get; set; }
    public Inventory Inventory { get; set; }

    protected List<GameObject> unitList;
    protected Unit bossInstance = null;
    // 현재 웨이브 인덱스
    protected int currentWaveIndex = -1;


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
        
        unitList = new();

        // 초기 플레이어 유닛 생성
        SpawnPlayerUnits();

        // 웨이브 시작
        StartCoroutine(StartWave());
    }

    // 게임 시작 가능 여부
    public bool IsGameStartable()
    {
        // 타일관리자 유효성 확인
        if (!TileManager || TileManager.IsGameStartable() == false)
            return false;

        if (!Inventory)
            return false;

        return true;
    }

    // 플레이어의 기본 유닛 생성
    protected void SpawnPlayerUnits()
    {
        Tile finishTile = TileManager.GetFinishTile();
        if (finishTile && bossUnit)
        {
            GameObject unit = SpawnUnitOnTile(bossUnit, finishTile.TileIndex, 0);
            if (unit)
            {
                bossInstance = unit.GetComponent<Unit>();
            }
        }
    }

    // 웨이브 생성
    protected IEnumerator StartWave()
    {
        for (int i = 0; i < waveList.Count; ++i)
        {
            yield return SpawnWave(waveList[i]);

            yield return new WaitForSeconds(waveDelay);
        }
    }

    // 웨이브 생성하기. 코루틴 반복용
    IEnumerator SpawnWave(WaveSpawnData waveData)
    {
        Tile startTile = TileManager.GetStartTile();

        foreach (GameObject unitPrefab in waveData.unitPrefabList)
        {
            if (unitPrefab != null)
            {
                SpawnUnitOnTile(unitPrefab, startTile.TileIndex);
            }

            yield return new WaitForSeconds(waveData.delay);
        }
    }

    // 바닥 타일임에 주의
    public GameObject SpawnUnitOnTile(GameObject unitPrefab, int tileIndex, int teamID = -1, GameObject unitInstance = null)
    {
        // 유닛 생성
        Tile tile = TileManager.GetTile(tileIndex);
        Vector3 spawnPosition = TileManager.GetTilePosition(tileIndex, true);
        spawnPosition.y += (unitPrefab.GetComponent<CapsuleCollider>().height * 0.5f);

        if (unitInstance == null)
        {
            unitInstance = GameObject.Instantiate<GameObject>(unitPrefab);
        }

        if (unitInstance)
        {
            // 유닛 생성 및 진행 방향 확인
            Quaternion spawnRotation = (teamID == 0) ? TileManager.GetFinishTile().transform.rotation : TileManager.GetStartTile().transform.rotation;
            unitInstance.transform.position = spawnPosition;
            unitInstance.transform.rotation = spawnRotation;

            Unit script = unitInstance.GetComponent<Unit>();
            if (script)
            {
                unitList.Add(unitInstance);

                script.TeamID = (teamID == -1) ? unitPrefab.GetComponent<Unit>().TeamID : teamID;
                script.TileIndex = tileIndex;
                script.Direction = unitInstance.transform.forward;
                script.Activate();
            }
        }

        return unitInstance;
    }

    public void NotifyHit(Unit instigator, Unit victim)
    {
        // 계산은 일단 단순하게
        victim.TakeDamage(instigator.gameObject, instigator.Power - victim.Armor);
    }

    public void NotifyDead(Unit unit)
    {
        if (unit)
        {
            unitList.Remove(unit.gameObject);

            if (unit == bossInstance)
            {
                Debug.Log("Game Over");
            }

            unit.Death();
        }
    }

    // 유닛이 목적지에 도착함
    public void UnitReachedFinish(Unit enemy)
    {
        Destroy(enemy.gameObject);
    }

    public List<GameObject> GetUnitsOnTile(int tileIndex)
    {
        List<GameObject> list = new();

        foreach (GameObject obj in unitList)
        {
            Unit unit = obj.GetComponent<Unit>();
            if (!unit) continue;

            if (unit.TileIndex == tileIndex)
            {
                list.Add(obj);
            }
        }

        return list;
    }
}

