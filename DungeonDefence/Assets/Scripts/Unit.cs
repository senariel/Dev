using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DDGame;

public delegate void UnitActivateEventHandler(bool isActivated);

[DisallowMultipleComponent]
public class Unit : MonoBehaviour
{
    //----- Status begin
    // 기본 유닛 데이터
    public UnitData UnitData { get => UnitData; set { Deactivate(); UnitData = value; Activate(); } }
    // 현재 체력
    public int HP { get; private set; }
    // 팀 번호. 플레이어는 0
    public ETeamID TeamID { get; set; }
    //----- Status end

    //----- Manager cache begin
    public GameManager GameManager { get; private set; }
    public TileManager TileManager { get; private set; }
    //----- Manager cache end

    //----- EventHandler begin
    public event UnitActivateEventHandler OnUnitActivated;

    protected ActionEventHandler actionBeginHandler;
    protected ActionEventHandler actionEndHandler;
    //----- EventHandler end

    //----- Action begin
    // 현재 수행 중인 행동
    public Action currentAction { get; private set; }
    //----- Action end

    // 현재 위치(타일 인덱스)
    public int TileIndex { get; set; }
    public Vector3 Direction {get => Direction; set {Direction = value.normalized;} }
    // 활성화 여부
    public bool IsActivated { get; private set; }

    //----- Serialize Fields begin
    // Start() 시 자동으로 Activate 될지 여부
    public bool autoActivate = false;
    // 액션 간 지연 시간. 모든 유닛 공통이 아니라면 유닛 정보로 넣어야 함
    public float actionDelay = 1.0f;
    //----- Serialize Fields end


    void Awake()
    {
        GameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();
        TileManager = GameManager.TileManager;

        // TileIndex = -1;
        // Direction = Vector3.zero;
        // ActionTarget = null;
        // currentHP = HP;

        actionBeginHandler = new ActionEventHandler(NotifyActionBeginPlay);
        actionEndHandler = new ActionEventHandler(NotifyActionEndPlay);
    }

    // Start is called before the first frame update
    void Start()
    {
        // 자동 활성화
        // 외부에서 활성화 시킬 경우 이전 단계에서 이미 활성화 되어있을 수 있음. 
        if (!IsActivated && autoActivate)
        {
            Activate();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDisable()
    {
        Debug.Log("Disable " + gameObject);
    }

    void OnDestroy()
    {
        Debug.Log("Destroy " + gameObject);
    }


    // 유닛 데이터 설정
    public virtual void OnUnitDataChanged(UnitData prevData, UnitData newData)
    {
        // 이름 설정
        gameObject.name = UnitData.UnitName;

    }

    // 유닛 활성화
    public virtual void Activate()
    {
        if (IsActivated) return;

        IsActivated = true;

        // 스탯 설정
        HP = UnitData.HP;

        // 액션 스크립트 생성/등록
        foreach (Object action in UnitData.Actions)
        {
            if (!action) continue;

            // 액션 생성.
            gameObject.AddComponent(System.Type.GetType(action.name));
        }

        OnActivated();

        // 액션 루프
        StartCoroutine(UpdateAction());
    }

    protected void Deactivate()
    {
        if (!IsActivated) return;

        IsActivated = false;

        // 이전 액션 제거
        Action[] prevActions = gameObject.GetComponentsInChildren<Action>(true);
        foreach (Action action in prevActions)
        {
            Debug.Log("destroy child component : " + action);
            Destroy(action);
        }

        OnDeactivated();
    }

    // 활성화 처리
    protected virtual void OnActivated()
    {
        if (OnUnitActivated != null) OnUnitActivated(true);
    }

    protected virtual void OnDeactivated()
    {
        if (OnUnitActivated != null) OnUnitActivated(false);
    }

    // 액션 갱신
    protected virtual IEnumerator UpdateAction()
    {
        while (IsAlive())
        {
            // 수행 중인 행동이 없다면 다음 행동 결정
            Action nextAction = ChooseAction();
            if (nextAction != null)
            {
                StartAction(nextAction);
                yield return new WaitUntil(() => (currentAction == null));
            }

            yield return new WaitForSeconds(actionDelay);
        }

        yield break;
    }

    //----- Action Begin
    // 수행 가능한 액션을 선택한다.
    // 기본은 리스트 우선순위
    protected virtual Action ChooseAction()
    {
        List<Action> list = new();
        GetComponents<Action>(list);

        foreach (Action action in list)
        {
            if (action.CanPlay(this))
            {
                return action;
            }
        }

        return null;
    }

    // 액션 수행 주기
    // Start - onactionstarted - play ~ onactionfinished - finish
    public void StartAction(Action action)
    {
        // Debug.Log("[StartAction] " + action);

        currentAction = action;

        // 이벤트 핸들러 연결
        currentAction.OnActionBeginPlay += actionBeginHandler;
        currentAction.OnActionEndPlay += actionEndHandler;

        // play 와 동시에 stop 이 될 수 있으므로 notify 를 먼저 호출
        OnActionStarted();

        currentAction.Play();
    }

    public void FinishAction(Action action)
    {
        if (currentAction != action)
        {
            Debug.Log("\tInvalid action  " + action + " / " + currentAction);
            return;
        }

        // 액션이 종료되어 있지 않다면 중지를 시도 합니다.
        if (action.IsPlaying)
        {
            action.Stop();
            return;
        }

        // Debug.Log("[FinishAction] " + action);

        // 이벤트 핸들러 연결 해제
        currentAction.OnActionBeginPlay -= actionBeginHandler;
        currentAction.OnActionEndPlay -= actionEndHandler;

        // 코루틴 관리를 위해 마지막에..
        currentAction = null;
    }

    protected virtual void OnActionStarted() { }
    protected virtual void OnActionFinished() { }

    protected virtual bool IsActionPlaying()
    {
        return (currentAction != null && currentAction.IsPlaying);
    }

    // from Action
    public void NotifyActionBeginPlay(Action action)
    {

    }

    // from Action
    public void NotifyActionEndPlay(Action action)
    {
        FinishAction(currentAction);
    }
    //----- Action End

    // 살아있나요
    public bool IsAlive()
    {
        // 체력이 0 초과?
        return IsActivated && (HP > 0);
    }

    // from GameManager
    // 데미지 받기.
    // GameManager 에서 가공이 된 최종 적용 값임
    public void TakeDamage(GameObject instigator, int amount)
    {
        // 체력 감소
        HP -= amount;

        // 사망시 보고
        if (HP <= 0)
        {
            GameManager.NotifyDead(this);
        }
        else
        {
            // 피격 연출
            OnTakeDamage(instigator, amount);
        }
    }

    // 피격 연출하기
    protected virtual void OnTakeDamage(GameObject instigator, int amount)
    {
        Debug.Log("[TakeDamage " + gameObject + "] " + amount + " by " + instigator + " : " + HP + " / " + UnitData.HP);
    }

    // from GameManager
    // 사망 선고도 GameManager 에게 받아야 함
    public void Death()
    {
        HP = 0;

        StartCoroutine(OnDeath());
    }

    // 사망 연출
    protected virtual IEnumerator OnDeath()
    {
        // 연출 후
        yield return new WaitForSeconds(1);

        // 제거
        Destroy(gameObject);
    }

    public bool IsEnemy(Unit unit)
    {
        return IsEnemy(unit.TeamID);
    }

    public bool IsEnemy(ETeamID teamID)
    {
        return (teamID == ETeamID.NoTeam) || (teamID != TeamID);
    }
}
