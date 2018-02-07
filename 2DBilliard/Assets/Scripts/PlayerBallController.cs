using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBallController : BallController
{
    // 터치 이펙트 프리팹
    public GameObject TouchEffect_Begin, TouchEffect_End;
    // 안내선 프리팹
    public GameObject GuideLine;

    private List<GameObject> TouchedObjectList;
    private GameObject TouchEffect_Begin_Clone, TouchEffect_End_Clone, GuideLine_Clone;
    private Vector2 StartPosition, EndPosition;

    // Use this for initialization
    void Start()
    {
        TouchedObjectList = new List<GameObject>();

        InitializeEffect();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 이펙트 초기화
    protected void InitializeEffect()
    {
        if (TouchEffect_Begin != null)
        {
            TouchEffect_Begin_Clone = MonoBehaviour.Instantiate<GameObject>(TouchEffect_Begin);
        }

        if (TouchEffect_End != null)
        {
            TouchEffect_End_Clone = MonoBehaviour.Instantiate<GameObject>(TouchEffect_End);
        }

        if (GuideLine != null)
        {
            GuideLine_Clone = MonoBehaviour.Instantiate<GameObject>(GuideLine);
            GuideLine_Clone.transform.parent = transform;
        }

        ActivateTouchEffect(false);
        ActivateGuideLine(false);
    }

    public List<GameObject> GetTouchedObjectsList()
    {
        return TouchedObjectList;
    }

    override public void OnTurnStateChanged(GameController.ETurn NewTurnState)
    {
        if (NewTurnState == GameController.ETurn.Action)
            TouchedObjectList.Clear();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TouchedObjectList.Add(collision.gameObject);
    }

    public void OnTouchStart(Vector2 TouchedPosition)
    {
        StartPosition = TouchedPosition;

        ActivateTouchEffect(true);
        SetTouchEffectPosition(0, TouchedPosition);
        SetTouchEffectPosition(1, TouchedPosition);

        ActivateGuideLine(true);
    }

    public void OnTouchMove(Vector2 TouchedPosition)
    {
        EndPosition = TouchedPosition;

        SetTouchEffectPosition(1, TouchedPosition);

        UpdateGuildLine();
    }

    public void OnTouchEnd(Vector2 TouchedPosition)
    {
        EndPosition = TouchedPosition;

        ActivateTouchEffect(false);
        ActivateGuideLine(false);
    }

    protected void ActivateTouchEffect( bool bActivate )
    {
        if (TouchEffect_Begin_Clone == null || TouchEffect_End_Clone == null)
            return;

        TouchEffect_Begin_Clone.SetActive(bActivate);
        TouchEffect_End_Clone.SetActive(bActivate);
    }

    protected void SetTouchEffectPosition( int Index, Vector2 NewPosition )
    {
        if (TouchEffect_Begin_Clone == null || TouchEffect_End_Clone == null)
            return;

        if (Index == 0)
            TouchEffect_Begin_Clone.transform.position = (Vector3)NewPosition;
        else
            TouchEffect_End_Clone.transform.position = (Vector3)NewPosition;
    }

    protected void ActivateGuideLine( bool bActivate )
    {
        if (GuideLine_Clone == null)
            return;

        GuideLine_Clone.SetActive(bActivate);
        GuideLine_Clone.transform.localPosition = Vector3.zero;
    }

    protected void UpdateGuildLine()
    {
        if (GuideLine_Clone == null)
            return;

        float Power = 200;
        float Angle = Vector2.SignedAngle(Vector2.up, StartPosition - EndPosition);

        GuideLine_Clone.transform.localScale = new Vector3(1, Power, 1);
        GuideLine_Clone.transform.eulerAngles = new Vector3(0.0f, 0.0f, Angle);
    }
}
