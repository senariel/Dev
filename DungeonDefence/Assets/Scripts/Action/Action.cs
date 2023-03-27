using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ActionEventHandler(Action action);

public class Action : MonoBehaviour
{
    public event ActionEventHandler OnActionBeginPlay;
    public event ActionEventHandler OnActionEndPlay;

    protected Unit owner;

    protected bool isPlaying = false;

    protected virtual void Awake()
    {
        owner = GetComponent<Unit>();
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

    public virtual bool CanPlay(Unit unit)
    {
        return true;
    }

    public void Play()
    {
        isPlaying = true;

        OnBeginPlay();
    }

    public void Finish()
    {
        isPlaying = false;

        OnEndPlay();
    }

    protected virtual void OnBeginPlay() { if (OnActionBeginPlay != null) OnActionBeginPlay(this); }
    protected virtual void OnEndPlay() { if (OnActionEndPlay != null) OnActionEndPlay(this); }

    public virtual void OnUnitActiveChanged(bool isActivated) { }
}
