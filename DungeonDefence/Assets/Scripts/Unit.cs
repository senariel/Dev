using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void UnitActivateEventHandler(bool isActivated);

public class Unit : MonoBehaviour
{
    public event UnitActivateEventHandler OnUnitActivated;

    protected GameManager gameManager;
    protected TileManager tileManager;

    // Start() 시 자동으로 Activate 될지 여부
    public bool autoActivate = false;

    public int HP;
    // 공격력
    public int Power;
    // 방어력
    public int Armor;
    //공격속도(초당 공격 횟수)
    public float AttackSpeed;
    // 팀 번호. 플레이어는 0
    public int TeamID;

    // 액션 간 지연 시간
    public float actionDelay = 1.0f;

    // 현재 보유 체력
    protected int currentHP;
    // 현재 수행 중인 행동
    protected Action currentAction = null;

    // 액션의 대상(유닛, 블럭 등등)
    public GameObject ActionTarget {get; set;}

    // 현재 위치(타일 인덱스)
    public int TileIndex { get; set; }
    // 활성화 여부
    public bool IsActivated { get; set; }

    // 유닛의 진행 방향
    public Vector3 Direction { get; set; }

    protected ActionEventHandler actionBeginHandler;
    protected ActionEventHandler actionEndHandler;


    void Awake()
    {
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();
        tileManager = gameManager.TileManager;

        TileIndex = -1;
        Direction = Vector3.zero;
        ActionTarget = null;

        actionBeginHandler = new ActionEventHandler(NotifyActionBeginPlay);
        actionEndHandler = new ActionEventHandler(NotifyActionEndPlay);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHP = HP;

        if (autoActivate)
        {
            Activate(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Activate(bool bActive = true)
    {
        if (IsActivated == bActive)
            return;

        IsActivated = bActive;
        if (IsActivated)
        {
            OnActivated();

            UpdateAction();
        }
        else
        {
            OnDeactivated();
        }
    }

    protected virtual void OnActivated()
    {
        if (OnUnitActivated != null) OnUnitActivated(true);
    }

    protected virtual void OnDeactivated()
    {
        if (OnUnitActivated != null) OnUnitActivated(false);
    }

    // 액션 갱신
    protected virtual void UpdateAction()
    {
        if (currentAction) return;

        ActionTarget = UpdateActionTarget();

        // 수행 중인 행동이 없다면 다음 행동 결정
        Action nextAction = ChooseAction();
        if (nextAction != null)
        {
            StartAction(nextAction);
        }
        else
        {
            // 수행 할 액션을 찾지 못했다면 대기
            Invoke("UpdateAction", actionDelay);
        }
    }

    protected virtual GameObject UpdateActionTarget()
    {
        // 사정거리 판단
        // 현재는 겹쳐진 타일 위의 유닛
        List<GameObject> list = gameManager.GetUnitsOnTile(TileIndex);

        // 상호작용 우선 순위?
        foreach (GameObject obj in list)
        {
            // 일단은 적만 상대합니다.
            if (obj.layer != LayerMask.NameToLayer("Unit")) continue;
            
            Unit unit = obj.GetComponent<Unit>();
            if (unit && IsEnemy(unit))
            {
                return obj;
            }
        }

        return null;
    }

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

    public void StartAction(Action action)
    {
        // Debug.Log("[StartAction] " + action);

        currentAction = action;

        // 이벤트 핸들러 연결
        currentAction.OnActionBeginPlay += actionBeginHandler;
        currentAction.OnActionEndPlay += actionEndHandler;

        currentAction.Play();

        OnActionStarted();
    }

    public void FinishAction(Action action)
    {
        if (currentAction != action)
        {
            Debug.Log("\tInvalid action  " + action + " / " + currentAction);
            return;
        }

        // Debug.Log("[EndAction] " + action);

        // 이벤트 핸들러 연결 해제
        currentAction.OnActionBeginPlay -= actionBeginHandler;
        currentAction.OnActionEndPlay -= actionEndHandler;

        Action prev = currentAction;
        currentAction = null;

        OnActionFinished(prev);

        // 유닛이 여전히 활성 상태라면 다음 액션을 갱신합니다.
        if (IsActivated)
        {
            Invoke("UpdateAction", actionDelay);
        }
    }

    protected virtual void OnActionStarted() { }
    protected virtual void OnActionFinished(Action prev) { }

    public void NotifyActionBeginPlay(Action action)
    {

    }

    public void NotifyActionEndPlay(Action action)
    {
        FinishAction(currentAction);
    }


    public bool IsAlive()
    {
        // 체력이 0 초과?
        return currentHP > 0;
    }

    // 데미지 받기.
    // 데미지량 계산은 GameManager 에서 처리한 값이어야 한다.
    public void TakeDamage(GameObject instigator, int amount)
    {
        currentHP -= amount;

        if (currentHP <= 0)
        {
            gameManager.NotifyDead(this);
        }
        else
        {
            OnTakeDamage(instigator, amount);
        }
    }

    protected virtual void OnTakeDamage(GameObject instigator, int amount)
    {
        Debug.Log("[TakeDamage " + gameObject + "] " + amount + " by " + instigator + " : " + currentHP + " / " + HP);
    }

    public void Death()
    {
        currentHP = 0;

        OnDeath();
    }

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    public bool IsEnemy(Unit unit)
    {
        return IsEnemy(unit.TeamID);
    }

    public bool IsEnemy(int teamID)
    {
        return teamID != TeamID;
    }
}
