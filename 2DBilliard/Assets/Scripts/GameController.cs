using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 턴을 관리해야 한다.
// 플레이어의 행동 후 모든 공의 움직임이 멈추면 턴을 종료한다.
// 

public class GameController : MonoBehaviour
{
    public enum ETurn
    {
        None,
        Action,
        Wait
    }
    public GameObject CueBall;
    public List<GameObject> ObjectBalls;
    public GameObject GuideLine, TouchPoint_Begin, TouchPoint_End;
    public int MaxScore, MaxLife;
    public float DistanceToPower, MaxPower;

    protected int Score, Life;

    private ETurn Turn;     // 플레이어 행동 중 여부
    private Vector2 StartPosition, EndPosition;

    private Rigidbody2D CueBallRB;

	// Use this for initialization
	void Start ()
    {
        // 점수 초기화
        Score = 0;
        Life = MaxLife;

        // 큐 볼의 RB 를 캐싱한다.
        if (CueBall != null)
        {
            CueBallRB = CueBall.GetComponent<Rigidbody2D>();
        }

        ResetEffects();

        // 턴 시작
        StartTurn();
    }
	
	// Update is called once per frame
	void Update ()
    {
        // 턴 확인
        if (Turn == ETurn.Action)
        {
            if (CheckTouch() == false)
                CheckMouse();
        }
        else if (Turn == ETurn.Wait && CheckBallStop() == true)
        {
            EndTurn();
        }

        // 임시 비상 정지
        if (Input.GetMouseButtonDown(1) == true)
        {
            Debug.Log("Stop!");

            CueBallRB.velocity = Vector2.zero;

            for (int i = 0; i < ObjectBalls.Count; ++i)
            {
                Rigidbody2D RB = ObjectBalls[i].GetComponent<Rigidbody2D>();
                if (RB != null)
                {
                    RB.velocity = Vector2.zero;
                }
            }
        }
    }

    private void ResetEffects()
    {
        // 일단 가이드라인은 숨김
        GuideLine.SetActive(false);

        // 터치 포인트도 숨김
        TouchPoint_Begin.SetActive(false);
        TouchPoint_End.SetActive(false);
    }

    bool CheckTouch()
    {
        if (Input.touchCount < 1)
            return false;

        // 터치 판단
        Touch Shot = Input.GetTouch(0);
        switch (Shot.phase)
        {
            case TouchPhase.Began:
                OnTouchStart(Shot.position);
                break;

            case TouchPhase.Moved:
                OnTouchMove(Shot.position);
                break;

            case TouchPhase.Ended:
                OnTouchEnd(Shot.position);
                break;

            case TouchPhase.Canceled:
                OnTouchEnd(Shot.position);
                break;
        }

        return true;
    }

    bool CheckMouse()
    {   
        if (Input.GetMouseButtonDown(0) == true)
        {
            OnTouchStart((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            return true;
        }
        else if (Input.GetMouseButtonUp(0) == true)
        {
            OnTouchEnd((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            return true;
        }
        else if (Input.GetMouseButton(0) == true)
        {
            OnTouchMove((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            return true;
        }

        return false;
    }

    void OnTouchStart( Vector2 TouchedPosition )
    {
        StartPosition = TouchedPosition;

        TouchPoint_Begin.transform.position = (Vector3)TouchedPosition;
        TouchPoint_Begin.SetActive(true);

        TouchPoint_End.transform.position = (Vector3)TouchedPosition;
        TouchPoint_End.SetActive(true);

        GuideLine.SetActive(true);
    }

    void OnTouchMove(Vector2 TouchedPosition)
    {
        EndPosition = TouchedPosition;

        TouchPoint_End.transform.position = (Vector3)TouchedPosition;

        UpdateGuildLine();
    }

    void OnTouchEnd(Vector2 TouchedPosition)
    {
        EndPosition = TouchedPosition;

        GuideLine.SetActive(false);
        
        // 터치 포인트도 숨김
        TouchPoint_Begin.SetActive(false);
        TouchPoint_End.SetActive(false);

        StartShot();
    }

    void UpdateGuildLine()
    {
        if (GuideLine == null)
            return;
        
        float Power = Mathf.Min(Vector2.Distance(StartPosition, EndPosition) * DistanceToPower, MaxPower) / DistanceToPower;
        float Angle = Vector2.SignedAngle(Vector2.up, StartPosition - EndPosition);

        Transform t = GuideLine.GetComponent<Transform>();
        if (t == null)
            return;

        Vector3 NewScale = new Vector3(1, Power, 1);

        t.localScale = NewScale;
        t.rotation = Quaternion.Euler(0.0f, 0.0f, Angle);
    }

    // 샷 가즈아!
    void StartShot()
    {
        float PowerScalar = Mathf.Min(Vector2.Distance(StartPosition, EndPosition) * DistanceToPower, MaxPower);

        Vector2 Power = (StartPosition - EndPosition).normalized * PowerScalar;

        if (CueBallRB == null)
            return;

        // 플레이어 턴 종료
        Turn = ETurn.Wait;
        
        CueBallRB.AddForce(Power);
    }

    // 미세하게 움직이는 공은 세운다.
    bool CheckBallStop()
    {
        if (CueBallRB.velocity != Vector2.zero)
            return false;

        for (int i = 0; i < ObjectBalls.Count; ++i)
        {
            Rigidbody2D RB = ObjectBalls[i].GetComponent<Rigidbody2D>();
            if (RB != null && RB.velocity != Vector2.zero)
                return false;
        }

        return true;
    }

    void StartTurn()
    {
        Turn = ETurn.Action;
    }

    void EndTurn()
    {
        // 스코어 체크

        // 게임 종료 체크

        ResetEffects();

        // 다음 턴 시작
        StartTurn();
    }
}
