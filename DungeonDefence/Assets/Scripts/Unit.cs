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
    //공격속도
    public float AttackSpeed;

    // 액션 간 지연 시간
    public float actionDelay = 1.0f;

    // 현재 보유 체력
    protected int currentHP;
    // 현재 수행 중인 행동
    protected Action currentAction = null;

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

        actionBeginHandler = new ActionEventHandler(NotifyActionBeginPlay);
        actionEndHandler = new ActionEventHandler(NotifyActionEndPlay);
    }

    // Start is called before the first frame update
    void Start()
    {
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
    virtual protected void UpdateAction()
    {
        if (currentAction) return;

        // 수행 중인 행동이 없다면 다음 행동 결정
        Action nextAction = ChooseAction();
        if (nextAction != null)
        {
            StartAction(nextAction);
        }
    }

    // 수행 가능한 액션을 선택한다.
    // 기본은 리스트 우선순위
    virtual protected Action ChooseAction()
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


    // 데미지 받기.
    // 데미지량 계산은 GameManager 에서 처리한 값이어야 한다.
    public void TakeDamage(GameObject instigator, int amount)
    {
        currentHP -= amount;

        if (currentHP <= 0)
        {
            Death();
        }
        else
        {
            OnTakeDamage(instigator, amount);
        }
    }

    protected virtual void OnTakeDamage(GameObject instigator, int amount)
    {

    }

    public void Death()
    {
        currentHP = 0;

        OnDeath();
    }

    protected virtual void OnDeath()
    {

    }
}
