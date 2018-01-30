using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 턴을 관리해야 한다.
// 플레이어의 행동 후 모든 공의 움직임이 멈추면 턴을 종료한다.
// 

public class GameController : MonoBehaviour
{
    public int MaxScore, MaxLife;
    protected int Score, Life;
    private bool bShot;

    protected ArrayList Balls;

	// Use this for initialization
	void Start ()
    {
        // 점수 초기화
        Score = 0;
        Life = MaxLife;

        

    }
	
	// Update is called once per frame
	void Update ()
    {
		if (bShot && CheckBallStop())
        {
            OnTurnEnd();
        }
	}

    bool CheckBallStop()
    {
        return false;
    }

    // 턴 시작 됨
    void OnTurnStart()
    {
        // 플레이어 볼 활성화
    }

    // 턴 종료 됨
    void OnTurnEnd()
    {
        // 샷 초기화
        bShot = false;

        // 모든 공의 타격 판단


        // 점수 체크
     //   if (Score >= MaxScore)


        // 라이프 체크

        // 클리어/종료 판단
    }
}
