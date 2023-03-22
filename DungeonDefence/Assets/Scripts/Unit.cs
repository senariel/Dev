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

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();
        tileManager = gameManager?.GetTileManager();

        IsOnGoal = false;

        NavAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (NavAgent)
        {
            MoveToNextTile();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 목적지 도착
        if (targetTile)
        {
            if (NavAgent.remainingDistance <= NavAgent.stoppingDistance && NavAgent.velocity.sqrMagnitude == 0.00f)
            {
                if (IsOnGoal)
                {
                    Debug.Log("\t\tI'm on goal");
                    // 목적지 도착 보고
                    gameManager.UnitReachedFinish(this);
                }
                else
                {
                    // 다음 타일로 이동
                    MoveToNextTile();
                }
            }
        }
    }

    void MoveToNextTile()
    {
        targetTile = FindNextTile();
        if (!targetTile)
        {
            Debug.Log("[Unit : MoveToNextTile] Can't find next tile");
            return;
        }

        // 목적지 보정(캐릭터 높이)
        CapsuleCollider capsule = gameObject.GetComponent<CapsuleCollider>();
        Vector3 positionAdd = new Vector3(0.0f, capsule ? capsule.height : 0.0f, 0.0f);
        NavAgent.SetDestination(targetTile.transform.position + positionAdd);
    }

    // 목표 찾기
    protected Tile FindNextTile()
    {
        CapsuleCollider capsule = gameObject.GetComponent<CapsuleCollider>();

        // 레이 시작 위치. 높이 보정 포함
        Vector3 startPosition = transform.position;
        startPosition.y += capsule ? capsule.height / 2 : 1.0f;

        Debug.Log("[Unit : FindNextTile] " + startPosition);

        // 레이 도착 위치 
        // #1. 오른쪽부터 판단
        float maxDistance = CalcTileLength(transform.right);
        if (maxDistance <= 0.00f) return null;

        // 해당 위치로 트레이스
        RaycastHit hitInfo;
        if (Physics.Raycast(startPosition, transform.right, out hitInfo, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // 막혔으면 정면
            maxDistance = CalcTileLength(transform.forward);
            if (maxDistance <= 0.00f) return null;

            if (Physics.Raycast(startPosition, transform.forward, out hitInfo, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                maxDistance = CalcTileLength(transform.right * -1.00f);
                if (maxDistance <= 0.00f) return null;

                // 막혔으면 왼쪽으로
                if (Physics.Raycast(startPosition, transform.right * -1.00f, out hitInfo, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    // 여기까지 막혔다면 진출로가 없음.
                    Debug.Log("\tfailed to find movable direction");
                    return null;
                }
                else
                {
                    startPosition += transform.right * -1.00f * maxDistance;
                }
            }
            else
            {
                startPosition += transform.forward * maxDistance;
            }
        }
        else
        {
            startPosition += transform.right * maxDistance;
        }

        // 이동 가능한 블럭을 찾았음. 바닥 블럭을 찾아서 반환합니다.
        if (Physics.Raycast(startPosition, Vector3.down, out hitInfo, capsule.height) == false)
        {
            Debug.Log("\tfailed to find bottomblock : " + startPosition + " / " + capsule.height);
            return null;
        }

        return hitInfo.collider.gameObject.GetComponent<Tile>();
    }

    protected float CalcTileLength(Vector3 forwardDir)
    {
        if ((forwardDir - Vector3.forward).sqrMagnitude <= 0.01f || (forwardDir - Vector3.back).sqrMagnitude <= 0.01f)
        {
            return tileManager.tileSize.z;
        }
        else if ((forwardDir - Vector3.left).sqrMagnitude <= 0.01f || (forwardDir - Vector3.right).sqrMagnitude <= 0.01f)
        {
            return tileManager.tileSize.x;
        }

        Debug.Log("[Unit : FindNextTile] I'm on invalid rotation : " + forwardDir);

        return 0.00f;
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
