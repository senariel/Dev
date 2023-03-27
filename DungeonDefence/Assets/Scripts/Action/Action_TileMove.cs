using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_TileMove : Action
{
    protected UnityEngine.AI.NavMeshAgent navAgent = null;
    protected TileManager tileManager = null;


    protected override void Awake()
    {
        base.Awake();

        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        tileManager = gm?.TileManager;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!isPlaying) return;

        // 도착 여부
        if ((navAgent.remainingDistance <= navAgent.stoppingDistance) && (navAgent.velocity.sqrMagnitude == 0.00f))
        {
            OnMoveFinished();
        }
    }

    public override bool CanPlay(Unit unit)
    {
        if (base.CanPlay(unit) && tileManager && navAgent && (unit.TileIndex > -1))
        {
            return true;
        }

        return false;
    }

    protected override void OnBeginPlay()
    {
        base.OnBeginPlay();

        MoveToNextTile();
    }

    // 다음 타일로 이동
    void MoveToNextTile()
    {
        // 다음 타일 인덱스 찾기
        // step #1. 오른쪽 확인
        Vector3 dir = Quaternion.AngleAxis(90.0f, Vector3.up) * owner.Direction;
        int nextTileIndex = tileManager.CheckMovableTile(owner.TileIndex, dir, owner);
        if (nextTileIndex == -1)
        {
            // step #2. 정면 확인
            dir = owner.Direction;
            nextTileIndex = tileManager.CheckMovableTile(owner.TileIndex, dir, owner);
            if (nextTileIndex == -1)
            {
                // step #3. 왼쪽 확인
                dir = Quaternion.AngleAxis(-90.0f, Vector3.up) * owner.Direction;
                nextTileIndex = tileManager.CheckMovableTile(owner.TileIndex, dir, owner);
                if (nextTileIndex == -1)
                {
                    Debug.Log("Failed to find next tile!");

                    return;
                }
            }
        }

        owner.Direction = dir;
        owner.TileIndex = nextTileIndex;

        Vector3 nextPosition = tileManager.GetTilePosition(nextTileIndex, true);
        nextPosition.y += (owner.GetComponent<CapsuleCollider>().height * 0.5f);

        if (navAgent.SetDestination(nextPosition) == false)
        {
            Debug.Log("Failed to SetDestination" + nextPosition);
        }
    }

    // 이동 완료 처리
    protected void OnMoveFinished()
    {
        navAgent.ResetPath();

        Finish();

        Tile tile = tileManager.GetTile(owner.TileIndex);
        if (tile)
        {
            tile.Enter(owner);
        }
    }
}
