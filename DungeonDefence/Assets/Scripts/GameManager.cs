using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DDGame;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct WaveSpawnData
    {
        public float Delay;
        public List<UnitData> UnitSquad;
    }

    //----- Serialize Fields begin
    // 현 스테이지의 보스 정보.
    // 추후엔 인벤토리에서 올 수 있음.
    public UnitData stageBoss;
    public List<WaveSpawnData> waveList;
    // 웨이브 간 대기 시간
    public float waveDelay;
    //----- Serialize Fields end

    //----- Manager begin
    public TileManager TileManager { get; private set; }
    public Inventory Inventory { get; private set; }
    //----- Manager end

    //----- Cached Object begin
    // 생성된 유닛 목록
    protected List<Unit> unitList = new();
    // 생성된 보스 유닛
    protected Unit bossInstance = null;
    //----- Cached OBject end

    //----- Event Handler begin
    public event StageStateChangeEventHandler StateChangeEvent;
    //----- Event Handler end


    // 스테이지 진행 상태
    public EStageState State { get; set; }
    // 상태별 대기 시간. 0은 무제한
    public int[] stateWaitTime = new int[System.Enum.GetValues(typeof(EStageState)).Length];

    // 현재 웨이브 인덱스
    protected int currentWaveIndex = -1;


    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("[GameManager : Awake]");

        State = DDGame.EStageState.None;
        
        TileManager = GetComponentInChildren<TileManager>();
    }

    void Start()
    {
        if (IsGameStartable() == false)
            return;

        // 준비 상태로 변경
        SetState(DDGame.EStageState.Fortify);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 스테이지 상태 변화
    public void SetState(DDGame.EStageState newState)
    {
        State = newState;

        // 이벤트 전파
        // OnStateChanged() 는 코루틴으로 동작하기 때문에 전달을 먼저 한다. 
        StateChangeEvent?.Invoke(State);

        // 상태 변화 처리
        StartCoroutine(OnStateChanged(State));
    }

    // 상태별 대기
    protected IEnumerator OnStateChanged(EStageState state)
    {
        // 대기 시간이 종료되면
        // 다음 상태로 넘어간다.
        switch (state)
        {
            case EStageState.Fortify:
                // 초기 플레이어 유닛 생성
                SpawnPlayerUnits();
                break;

            case EStageState.Invade:
                // 웨이브 시작
                yield return StartWave();
                break;
        }

        // 완료 후 대기 시간동안 대기
        float waitTime = stateWaitTime[(int)State];
        if (waitTime > 0.0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        yield break;
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
        // 최종 경비
        Tile finishTile = TileManager.GetFinishTile();
        if (finishTile && stageBoss && stageBoss.Prefab)
        {
            bossInstance = SpawnUnitOnTile(stageBoss, finishTile.TileIndex, ETeamID.Defence);
            bossInstance.Activate();
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

        foreach (UnitData unitData in waveData.UnitSquad)
        {
            if (unitData != null)
            {
                SpawnUnitOnTile(unitData, startTile.TileIndex, ETeamID.Offence);
            }

            yield return new WaitForSeconds(waveData.Delay);
        }
    }

    // 타일 위치에 유닛 생성하기
    public Unit SpawnUnitOnTile(UnitData unitData, int tileIndex, ETeamID teamID)
    {
        if (!unitData || !unitData.Prefab) return null;

        // 유닛 생성
        GameObject unitInstance = GameObject.Instantiate<GameObject>(unitData.Prefab);
        if (!unitInstance) return null;

        Unit unit = unitInstance.GetComponent<Unit>();
        if (unit)
        {
            unitList.Add(unit);            

            // 팀 인덱스
            unit.TeamID = teamID;

            // 배치
            PlaceUnitOnTile(unit, tileIndex);
        }

        return unit;
    }

    // 타일 위치로 유닛 배치하기
    public void PlaceUnitOnTile(Unit target, int tileIndex)
    {
        // 유닛 배치
        Vector3 spawnPosition = TileManager.GetTilePosition(tileIndex, true);
        // spawnPosition.y += (target.Prefab.GetComponent<CapsuleCollider>().height * 0.5f);

        // 유닛 생성 및 진행 방향 확인
        Quaternion spawnRotation = (target.TeamID == 0) ? 
            TileManager.GetFinishTile().transform.rotation : 
            TileManager.GetStartTile().transform.rotation;

        target.TileIndex = tileIndex;
        target.transform.SetPositionAndRotation( spawnPosition,  spawnRotation);
    }

    // 타격 보고
    public void NotifyHit(Unit target, DamageData damage)
    {
        // 검증 단계 필요

        // 계산은 일단 단순하게
        target.TakeDamage(damage.instigator, damage.ApplyTo(target));
    }

    // 사망보고
    public void NotifyDead(Unit unit)
    {
        if (unit)
        {
            unitList.Remove(unit);
            unit.Death();

            if (unit == bossInstance)
            {
                SetState(EStageState.End);
            }
        }
    }

    // 유닛이 최종 목적지에 도착함
    public void UnitReachedFinish(Unit enemy)
    {
        // Destroy(enemy.gameObject);
    }

    // 특정 타일 위에 있는 유닛
    public List<Unit> GetUnitsOnTile(int tileIndex)
    {
        List<Unit> list = new();

        foreach (Unit unit in unitList)
        {
            if (!unit) continue;

            if (unit.TileIndex == tileIndex)
            {
                list.Add(unit);
            }
        }

        return list;
    }
}

