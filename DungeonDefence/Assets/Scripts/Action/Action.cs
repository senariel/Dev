using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void ActionEventHandler(Action action);

[RequireComponent(typeof(Unit), typeof(Animator))]
public class Action : MonoBehaviour
{
    public event ActionEventHandler OnActionBeginPlay;
    public event ActionEventHandler OnActionEndPlay;

    protected GameManager GameManager;
    protected Unit owner;
    protected Animator animator;

    public bool IsPlaying = false;

    protected virtual void Awake()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        owner = GetComponent<Unit>();
        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void OnEnable()
    {
        if (owner)
        {
            owner.OnUnitActivated += OnUnitActiveChanged;
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (owner)
        {
            // 최초 유닛 활성 상태 갱신.
            // 이후에는 delegate 로 받음
            OnUnitActiveChanged(owner.IsActivated);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    protected virtual void OnDisable()
    {
        if (owner)
        {
            owner.OnUnitActivated -= OnUnitActiveChanged;
        }
    }

    public virtual bool CanPlay(Unit unit)
    {
        // 유닛이 정상 배치되어 활성화 되어 있다면
        return (unit.IsAlive() && unit.TileIndex > -1);
    }

    // from Unit
    // 행동 시작
    public void Play()
    {
        IsPlaying = true;

        OnBeginPlay();
    }

    // 정지
    public void Stop()
    {

    }

    // 종료
    public void Finish()
    {
        IsPlaying = false;

        OnEndPlay();
    }

    //     protected virtual GameObject FindActionTarget()
    // {
    //     // 행동 대상 우선 순위
    //     // #1. 전투
    //     // 사정거리 판단?
    //     // 현재는 겹쳐진 타일 위의 유닛
    //     List<GameObject> list = GameManager.GetUnitsOnTile(TileIndex);

    //     // 상호작용 우선 순위?
    //     foreach (GameObject obj in list)
    //     {
    //         // 일단은 적만 상대합니다.
    //         if (obj.layer != LayerMask.NameToLayer("Unit")) continue;

    //         Unit unit = obj.GetComponent<Unit>();
    //         if (unit && IsEnemy(unit))
    //         {
    //             return obj;
    //         }
    //     }

    //     return null;
    // }


    protected virtual void OnBeginPlay() { if (OnActionBeginPlay != null) OnActionBeginPlay(this); }
    protected virtual void OnEndPlay() { if (OnActionEndPlay != null) OnActionEndPlay(this); }

    public virtual void OnUnitActiveChanged(bool isActivated) { }
}
