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
        Wait,
        Calc
    }
    // 수구
    public GameObject CueBall;
    // 적구
    public List<GameObject> ObjectBalls;
    // 가이드라인, 터치 이펙트,
    public GameObject GuideLine, TouchPoint_Begin, TouchPoint_End;
    // 기본 턴 수, 최대 턴 수
    public float DefaultTurn, MaxTurn;
    // 타격 점수
    public float Score_Miss, Score_Save, Score_Success, Score_3Cushion, Score_BankShot;
    // 힘 계산용 값
    public float DistanceToPower, MaxPower;

    // 플레이어에게 남은 턴 수
    protected float Turn = 0.0f;
    // 현재 턴 진행 상태
    private ETurn TurnState = ETurn.None;
    // 수구의 타격 계산용
    private Vector2 StartPosition, EndPosition;

	// Use this for initialization
	void Start ()
    {        
        // 턴 수 충전
        Turn = DefaultTurn;

        // 턴 시작
        SetTurnState(ETurn.Action);
    }
	
	// Update is called once per frame
	void Update ()
    {
        // 턴 확인
        if (TurnState == ETurn.Action)
        {
            // 입력 확인
            if (CheckTouch() == false)
                CheckMouse();
        }
        else if (TurnState == ETurn.Wait && CheckBallStop() == true)
        {
            SetTurnState(ETurn.Calc);
        }

        // 임시 비상 정지
        if (Input.GetMouseButtonDown(1) == true)
        {
            EmergencyStop();
        }
    }

    // 비상 정지 : 디버깅용
    void EmergencyStop()
    {
        Debug.Log("Stop!");

        // 모든 공에 알림
        GameObject[] Balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject Ball in Balls)
        {
            if (Ball == null)
                continue;

            Rigidbody2D RB = Ball.GetComponent<Rigidbody2D>();
            if (RB == null)
                continue;

            RB.velocity = Vector2.zero;
        }
    }

    // 턴 관리
    void SetTurnState( ETurn NewTurnState )
    {
        if (TurnState == NewTurnState)
            return;

        Debug.Log("SetTurnState : " + TurnState + " -> " + NewTurnState);

        TurnState = NewTurnState;

        switch (NewTurnState)
        {
            case ETurn.Action:
                StartPlayerAction();
                break;

            case ETurn.Wait:
                EndPlayerAction();
                break;

            case ETurn.Calc:
                CalcResult();
                break;
        }

        // 모든 공에 알림
        GameObject[] Balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject Ball in Balls)
        {
            if (Ball == null)
                continue;

            BallController BC = Ball.GetComponent<BallController>();
            if (BC == null)
                continue;

            BC.OnTurnStateChanged(NewTurnState);
        }
    }

    // 플레이어 액션 시작
    void StartPlayerAction()
    {
        // 연출 초기화
        ResetEffects();

        // to do. 액션 가능 연출

    }

    // 플레이어 액션 종료
    void EndPlayerAction()
    {
        // to do. 관전 연출?
    }

    // 점수 계산
    void CalcResult()
    {
        // 맞은 적구 판단
        // 수구는 최초에 캐싱된 하나만 인정한다.
        PlayerBallController PC = CueBall.GetComponent<PlayerBallController>();
        if (PC != null)
        {
            List<GameObject> TouchedObjects = PC.GetTouchedObjectsList();

            int TouchedBalls = 0;
            int TouchedWalls = 0;
            bool bBankShot = false;
            float Score = Score_Miss;
            foreach (GameObject TouchedObject in TouchedObjects)
            {
                if (TouchedObject == null)
                    continue;

                // 적구 타격 먼저 판단
                if (TouchedObject.CompareTag( "Ball" ))
                {
                    TouchedBalls++;

                    // 첫 적구 터치 시 가락 확인
                    if (TouchedBalls == 1 && TouchedWalls >= 3 && bBankShot == false)
                    {
                        bBankShot = true;
                    }

                    // 적구 타격 완료 시 계산 완료
                    if (TouchedBalls >= ObjectBalls.Count)
                    {
                        // 성공 시 턴 증감 계산
                        if (bBankShot)
                        {
                            Score = Score_BankShot;
                        }
                        else if (TouchedWalls >= 3)
                        {
                            Score = Score_3Cushion;
                        }
                        else
                        {
                            Score = Score_Success;
                        }

                        break;
                    }
                }

                // 쿠션 판단
                if (TouchedObject.CompareTag("Wall"))
                {
                    TouchedWalls++;
                }
            }

            // 실패에 대한 턴 증감 계산
            if (Score == Score_Miss && TouchedBalls > 0)
            {
                Score = Score_Save;
            }

            // 잔여 턴 계산
            Turn = Mathf.Min( Turn + Score, MaxTurn );

            Debug.Log("Result ----");
            Debug.Log("Score : " + Score);
            Debug.Log("Touched Balls : " + TouchedBalls);
            Debug.Log("Touched Walls : " + TouchedWalls);
            Debug.Log("Bank Shot : " + bBankShot );
            Debug.Log("Remain Turn : " + Turn);
        }

        if (Turn > 0.0f)
            SetTurnState(ETurn.Action);
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

        // 샷 시작
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

        Rigidbody2D RB = CueBall.GetComponent<Rigidbody2D>();
        if (RB == null)
            return;

        RB.AddForce(Power);

        // 턴 변경
        SetTurnState(ETurn.Wait);
    }

    // 공이 움직이는지 여부 확인
    bool CheckBallStop()
    {
        // 모든 공에 알림
        GameObject[] Balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject Ball in Balls)
        {
            if (Ball == null)
                continue;

            Rigidbody2D RB = Ball.GetComponent<Rigidbody2D>();
            if (RB == null || RB.velocity != Vector2.zero)
                return false;
        }

        return true;
    }     
}
