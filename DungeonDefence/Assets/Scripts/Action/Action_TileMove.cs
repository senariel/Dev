using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DDGame;

[RequireComponent(typeof(NavMeshAgent)), DisallowMultipleComponent]
public class Action_TileMove : Action
{
    public EPreferDirection PreferDirection { get; set; }
    protected NavMeshAgent navAgent = null;
    protected TileManager tileManager = null;
    protected int targetTileIndex = -1;


    protected override void Awake()
    {
        base.Awake();

        navAgent = GetComponentInChildren<NavMeshAgent>();
        navAgent.autoBraking = false;
        navAgent.stoppingDistance = 0.1f;

        tileManager = GameManager?.TileManager;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        PreferDirection = (EPreferDirection)Random.Range(0, 2);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        //Debug.Log("[Update #1] " + IsPlaying + " / " + navAgent.pathPending + " / " + navAgent.hasPath);

        if (!IsPlaying) return;

        // 현재 이동 계산 중이 아니라면
        if (!navAgent.pathPending)
        {
            // 이동 중이라면
            if (navAgent.hasPath)
            {
                // 애니메이션 연출
                if (animator)
                {
                    animator.SetFloat("Speed", navAgent.velocity.magnitude);
                }
            }
            else
            {
                navAgent.velocity = Vector3.zero;

                // 도착 여부
                // if ((navAgent.remainingDistance <= navAgent.stoppingDistance) && (navAgent.velocity.sqrMagnitude == 0.00f))
                {
                    OnMoveFinished();
                }
            }

        }
    }

    // 작동 가능 여부
    public override bool CanPlay(Unit unit)
    {
        // 기본 체크
        if (base.CanPlay(unit) == false || !tileManager || !navAgent)
            return false;

        // 타일 위치 확인
        int tileIndex = FindNextTile(unit.TileIndex, unit.Direction);
        if (tileIndex == -1)
            return false;

        // 유효 타일 확인
        Tile tile = tileManager.GetTile(tileIndex);
        if (tile && tile.CanEnter(unit) == false)
            return false;

        return true;
    }

    protected override void OnBeginPlay()
    {
        base.OnBeginPlay();

        // 다음 목표 타일 갱신
        targetTileIndex = FindNextTile(owner.TileIndex, owner.Direction);
        if (targetTileIndex != -1)
        {
            // 이동
            MoveToTile(targetTileIndex);
        }
        else
        {
            Finish();
        }
    }

    public override void OnUnitActiveChanged(bool isActivated)
    {
        navAgent.enabled = isActivated;
    }

    // 다음 이동 타일 검색
    // tileIndex : 현재 타일 위치
    // direction : 현재 진행 방향
    // return : Tile Index
    protected int FindNextTile(int tileIndex, Vector3 direction)
    {
        // step #1. 선호 방향 확인
        float angle = PreferDirection == EPreferDirection.Right ? 90.0f : -90.0f;
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * direction;
        int index = FindMovableTileAround(tileIndex, dir, owner);
        if (index == -1)
        {
            // step #2. 정면 확인
            dir = direction;
            index = FindMovableTileAround(tileIndex, dir, owner);
            if (index == -1)
            {
                // step #3. 왼쪽 확인
                dir = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                index = FindMovableTileAround(tileIndex, dir, owner);
            }
        }

        return index;
    }

    // 이동 가능한 주변 타일 검색
    protected int FindMovableTileAround(int index, Vector3 dir, Unit unit)
    {
        int found = tileManager.GetTileIndexAround(index, dir);
        Tile tile = tileManager.GetTile(found);

        if (tile && tile.CanEnter(unit) == false)
        {
            return -1;
        }

        return found;
    }

    // 다음 타일로 이동
    void MoveToTile(int tileIndex)
    {
        Vector3 curPosition = tileManager.GetTilePosition(owner.TileIndex, true);
        Vector3 nextPosition = tileManager.GetTilePosition(tileIndex, true);
        // nextPosition.y += (owner.GetComponent<CapsuleCollider>().height * 0.5f);

        // 위치/방향 갱신
        owner.TileIndex = tileIndex;
        owner.Direction = (nextPosition - curPosition).normalized;

        // Debug.Log("[MoveToTile] " + owner.TileIndex + " / " + owner.Direction);

        if (navAgent.SetDestination(nextPosition) == false)
        {
            Debug.Log("Failed to SetDestination" + nextPosition);
        }
    }

    // 이동 완료 처리
    protected void OnMoveFinished()
    {
        // Debug.Log("OnMoveFinished");

        navAgent.ResetPath();

        // 애니메이션 멈춤
        animator.SetFloat("Speed", 0.0f);

        owner.TileIndex = targetTileIndex;

        Tile tile = tileManager.GetTile(owner.TileIndex);

        tile?.Enter(owner);

        Finish();
    }
}
