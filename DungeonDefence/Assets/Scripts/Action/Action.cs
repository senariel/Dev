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
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

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

    protected virtual void OnBeginPlay() { OnActionBeginPlay(this); }
    protected virtual void OnEndPlay() { OnActionEndPlay(this); }

}
