using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private GameObject Destination;
    private UnityEngine.AI.NavMeshAgent NavAgent;

    // 현재 목표 타일
    protected Tile targetTile;
    protected GameManager gameManager;
    protected TileManager tileManager;
    protected bool IsOnGoal = false;

    protected int currentTileIndex = -1;
    protected Vector3 moveDirection = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();
        tileManager = gameManager.TileManager;

        IsOnGoal = false;

        // 현재 타일 위치 갱신
        Tile startTile = tileManager?.GetStartTile();
        if (startTile)
        {
            currentTileIndex = startTile.tileIndex;
            moveDirection = startTile.transform.forward;
        }

        NavAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (NavAgent)
        {
            MoveToNextTile();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMoveFinished())
        {
            if (IsOnGoal)
            {
                // 목적지 도착 보고
                gameManager.UnitReachedFinish(this);
            }
            else if (currentTileIndex > -1)
            {
                // 다음 타일로 이동
                MoveToNextTile();
            }
        }
    }

    protected bool IsMoveFinished()
    {
        return (NavAgent.remainingDistance <= NavAgent.stoppingDistance) && (NavAgent.velocity.sqrMagnitude == 0.00f);
    }

    // 다음 타일로 이동
    void MoveToNextTile()
    {
        // 다음 타일 인덱스 찾기
        // step #1. 오른쪽 확인
        Vector3 dir = Quaternion.AngleAxis(90.0f, Vector3.up) * moveDirection;
        int nextTileIndex = tileManager.CheckMovableTile(currentTileIndex, dir, this);
        if (nextTileIndex == -1)
        {
            // step #2. 정면 확인
            dir = moveDirection;
            nextTileIndex = tileManager.CheckMovableTile(currentTileIndex, dir, this);
            if (nextTileIndex == -1)
            {
                // step #3. 왼쪽 확인
                dir = Quaternion.AngleAxis(-90.0f, Vector3.up) * moveDirection;
                nextTileIndex = tileManager.CheckMovableTile(currentTileIndex, dir, this);
                if (nextTileIndex == -1)
                {
                    Debug.Log("[Unit : MoveToNextTile] Can't find next tile");
                    currentTileIndex = nextTileIndex;
                    return;
                }
            }
        }

        moveDirection = dir;
        currentTileIndex = nextTileIndex;

        Vector3 nextPosition = tileManager.GetTilePosition(nextTileIndex, true);
        nextPosition.y += (GetComponent<CapsuleCollider>().height * 0.5f);

        NavAgent.SetDestination(nextPosition);
    }

    protected void OnTriggerEnter(Collider other)
    {
        // 최종 목적지에 도착하였다면 
        if (other.CompareTag("Tile_Finish"))
        {
            IsOnGoal = true;
        }
    }
}
