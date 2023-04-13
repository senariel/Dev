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
    public event GetTileHandler GetStartTileEvent;
    public event GetTileHandler GetFinishTileEvent;
    //----- Event Handler end


    // 스테이지 진행 상태
    public EStageState GameState { get; set; }
    // 방어 준비 시간
    public float FortifyTime;

    // 현재 웨이브 인덱스
    protected int currentWaveIndex = -1;


    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("[GameManager : Awake]");

        GameState = DDGame.EStageState.None;

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
        GameState = newState;

        // 이벤트 전파
        // OnStateChanged() 는 코루틴으로 동작하기 때문에 전달을 먼저 한다. 
        StateChangeEvent?.Invoke(GameState);

        // 상태 변화 처리
        StartCoroutine(UpdateGameState());
    }

    // 상태별 대기
    protected IEnumerator UpdateGameState()
    {
        // 일단 다음 프레임에
        yield return null;

        // 대기 시간이 종료되면
        // 다음 상태로 넘어간다.
        switch (GameState)
        {
            // 준비 상태
            case EStageState.Fortify:
                // 초기 플레이어 유닛 생성
                SpawnPlayerUnits();
                yield return new WaitForSeconds(FortifyTime);
                // 대기 시간이 지나면 침략 상태로 변경
                SetState(EStageState.Invade);
                break;

            // 침략 상태
            case EStageState.Invade:
                // 웨이브 시작
                yield return StartWave();
                break;

            // 종료 상태
            case EStageState.End:
                break;
        }
    }

    // 게임 시작 가능 여부
    public bool IsGameStartable()
    {
        // 타일관리자 유효성 확인
        if (!TileManager || TileManager.IsGameStartable() == false)
            return false;

        // if (!Inventory)
        //     return false;

        return true;
    }

    // 플레이어의 기본 유닛 생성
    protected void SpawnPlayerUnits()
    {
        // 최종 경비
        Tile finishTile = GetFinishTile();
        if (finishTile && stageBoss)
        {
            bossInstance = SpawnUnitOnTile(stageBoss, finishTile.TileIndex, ETeamID.Defence);
            bossInstance?.Activate();
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
        Tile startTile = GetStartTile();

        foreach (UnitData unitData in waveData.UnitSquad)
        {
            if (unitData != null)
            {
                Unit unit = SpawnUnitOnTile(unitData, startTile.TileIndex, ETeamID.Offence);
                unit.Activate();
            }

            yield return new WaitForSeconds(waveData.Delay);
        }
    }

    // 타일 위치에 유닛 생성하기
    public Unit SpawnUnitOnTile(UnitData unitData, int tileIndex, ETeamID teamID)
    {
        if (!unitData || !unitData.Prefab) return null;

        Unit unit = unitData.CreateUnit();
        if (unit)
        {
            // 팀 인덱스
            unit.TeamID = teamID;

            // 배치
            if (tileIndex > -1)
            {
                PlaceUnitOnTile(unit, tileIndex);
            }
        }

        return unit;
    }

    // 타일 위치로 유닛 배치하기
    public void PlaceUnitOnTile(Unit target, int tileIndex)
    {
        target.TileIndex = tileIndex;
        target.Direction = (target.TeamID == 0) ? TileManager.DefenceDirection : TileManager.OffenceDirection;

        // 유닛 배치
        Vector3 spawnPosition = TileManager.GetTilePosition(tileIndex, true);

        // 유닛 생성 및 진행 방향 확인
        Quaternion spawnRotation = Quaternion.LookRotation(target.Direction);

        target.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        unitList.Add(target);
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

    protected Tile GetStartTile()
    {
        return GetStartTileEvent?.Invoke();
    }

    protected Tile GetFinishTile()
    {
        return GetFinishTileEvent?.Invoke();
    }
}

