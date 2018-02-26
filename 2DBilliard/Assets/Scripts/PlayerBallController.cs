using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBallController : BallController
{
    // 터치 이펙트 프리팹
    public GameObject TouchEffect_Begin, TouchEffect_End;
    // 안내선 프리팹
    public GameObject GuideLine;
    // 안내선 표시 갯수
    public int SegmentCount = 100;
    // Length scale for each segment
    public float SegmentScale = 1;
    // Reference to the LineRenderer we will use to display the simulated path
    public LineRenderer SightLine;

    private List<GameObject> TouchedObjectList;
    private GameObject TouchEffect_Begin_Clone, TouchEffect_End_Clone, GuideLine_Clone;

    // Use this for initialization
    void Start()
    {
        TouchedObjectList = new List<GameObject>();

        SightLine = GetComponent<LineRenderer>();

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
            if (TouchEffect_Begin_Clone != null)
                TouchEffect_Begin_Clone.layer = Physics2D.IgnoreRaycastLayer;
        }

        if (TouchEffect_End != null)
        {
            TouchEffect_End_Clone = MonoBehaviour.Instantiate<GameObject>(TouchEffect_End);
            if (TouchEffect_End_Clone != null)
                TouchEffect_End_Clone.layer = Physics2D.IgnoreRaycastLayer;
        }

        //if (GuideLine != null)
        //{
        //    GuideLine_Clone = MonoBehaviour.Instantiate<GameObject>(GuideLine);
        //    GuideLine_Clone.transform.parent = transform;
        //}

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
        ActivateTouchEffect(true);
        SetTouchEffectPosition(0, TouchedPosition);
        SetTouchEffectPosition(1, TouchedPosition);

        ActivateGuideLine(true);
    }

    public void OnTouchMove(Vector2 TouchedPosition)
    {
        SetTouchEffectPosition(1, TouchedPosition);

        UpdateGuildLine();
    }

    public void OnTouchEnd(Vector2 TouchedPosition)
    {
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
        //if (GuideLine_Clone == null)
        //    return;

        //float Power = 20000;
        //Vector2 Direction = (StartPosition - EndPosition).normalized;

        //float Angle = Vector2.SignedAngle(Vector2.up, StartPosition - EndPosition);
        //GuideLine_Clone.transform.localScale = new Vector3(1, Power, 1);
        //GuideLine_Clone.transform.eulerAngles = new Vector3(0.0f, 0.0f, Angle);

        //CircleCollider2D Collider = GetComponent<CircleCollider2D>();

        //SimulatePath(transform.position, Collider.bounds.size.x / 2, Direction, Power );
    }

    public void SimulatePath(List<Vector2> Segments)
    {
        Color startColor = Color.red;
        Color endColor = Color.blue;
        startColor.a = 1;
        endColor.a = 1;
        SightLine.startColor = startColor;
        SightLine.endColor = endColor;
        SightLine.positionCount = Segments.Count;

        for (int i = 0; i < Segments.Count; ++i)
        {
            SightLine.SetPosition(i, Segments[i]);
        }
    }

    //protected void SimulatePath3(Vector2 StartPos, float Radius, Vector2 Direction, float Power)
    //{
    //    float SimulateDistance = 1000.0f;

    //    List<Vector2> Segments = new List<Vector2>();
    //    Segments.Add(StartPos);

    //    Rigidbody2D RB = GetComponent<Rigidbody2D>();
    //    PhysicsMaterial2D PM = RB.sharedMaterial;

    //    int dbgCount = 0;
    //    while (SimulateDistance > 0.0f)
    //    {
    //        RaycastHit2D hit = Physics2D.CircleCast(StartPos, Radius, Direction, SimulateDistance);
    //        if (hit)
    //        {
    //             Segments.Add(hit.centroid);

    //            Direction = Vector2.Reflect(Direction * PM.bounciness, hit.normal);

    //            SimulateDistance -= hit.distance;
    //            StartPos = hit.centroid + Direction;
    //        }
    //        else
    //        {
    //            Segments.Add(StartPos + (Direction * SimulateDistance));

    //            SimulateDistance = 0.0f;
    //        }

    //        //    Debug.Log("[" + dbgCount + "] " + Segments[dbgCount] + " / " + SimulateDistance);

    //        if (++dbgCount > 10)
    //        {
    //            Debug.Log("Break force");
    //            break;
    //        }
    //    }

    //        Color startColor = Color.red;
    //    Color endColor = Color.blue;
    //    startColor.a = 1;
    //    endColor.a = 1;
    //    SightLine.startColor = startColor;
    //    SightLine.endColor = endColor;
    //    SightLine.positionCount = Segments.Count;

    //    for (int i = 0; i < Segments.Count; ++i)
    //    {
    //        SightLine.SetPosition(i, Segments[i]);
    //    }
    //}

    //protected void SimulatePath2(Vector2 StartPos, float Radius, Vector2 Direction, float Power)
    //{
    //    Vector2[] Segments = new Vector2[SegmentCount];

    //    // The first line point is wherever the player's cannon, etc is
    //    Segments[0] = StartPos;

    //    // 0.1초 단위로 세그먼트를 찍는다?
    //    Vector2 SegVelocity = Direction * Power * 0.1f;

    //    // 세그먼트 갯수만큼 루프
    //    for (int i = 1; i < SegmentCount; ++i)
    //    {
    //        // Time it takes to traverse one segment of length segScale (careful if velocity is zero)
    //        float SegTime = (SegVelocity.sqrMagnitude != 0) ? SegmentScale / SegVelocity.magnitude : 0;

    //        // Add velocity from gravity for this segment's timestep
    //        //SegVelocity = SegVelocity + SegTime;

    //        // Check to see if we're going to hit a physics object
    //        RaycastHit2D hit = Physics2D.Raycast(Segments[i - 1], SegVelocity, SegmentScale);
    //        //RaycastHit2D hit = Physics2D.CircleCast(Segments[i - 1], Radius, SegVelocity, SegmentScale);
    //        if (hit)
    //        {
    //            // remember who we hit
    //            Collider2D _hitObject = hit.collider;

    //            Debug.Log("[" + i + "]" + _hitObject + " : " + hit.normal );

    //            // set next position to the position where we hit the physics object
    //            Segments[i] = Segments[i - 1] + SegVelocity.normalized * hit.distance;
    //            // correct ending velocity, since we didn't actually travel an entire segment
    //            //SegVelocity = SegVelocity - Physics.gravity * (Segmentscale - hit.distance) / SegVelocity.magnitude;
    //            // flip the velocity to simulate a bounce
    //            SegVelocity = Vector2.Reflect(SegVelocity, hit.normal);

    //            /*
    // * Here you could check if the object hit by the Raycast had some property - was 
    // * sticky, would cause the ball to explode, or was another ball in the air for 
    // * instance. You could then end the simulation by setting all further points to 
    // * this last point and then breaking this for loop.
    // */
    //        }
    //        // If our raycast hit no objects, then set the next position to the last one plus v*t
    //        else
    //        {
    //            Segments[i] = Segments[i - 1] + SegVelocity * SegTime;
    //        }
    //    }

    //    // At the end, apply our simulations to the LineRenderer

    //    // Set the colour of our path to the colour of the next ball
    //    Color startColor = Color.red;
    //    Color endColor = startColor;
    //    startColor.a = 1;
    //    endColor.a = 0;
    //    SightLine.startColor = startColor;
    //    SightLine.endColor = endColor;
    //    SightLine.positionCount = SegmentCount;

    //    for (int i = 0; i < SegmentCount; i++)
    //        SightLine.SetPosition(i, Segments[i]);
    //}
}
