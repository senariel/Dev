using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DDGame;

[CreateAssetMenu(fileName = "UnitData", menuName = "ScriptableObject/UnitData", order = 1)]
public class UnitData : ScriptableObject
{
    // 유닛 스크립트
    public Object Script;
    // 유닛 프리팹
    public GameObject Prefab;
    // 유닛 이름
    public string UnitName;
    // 체력
    public int HP;
    // 공격력
    public int Attack;
    // 방어력
    public int Defence;
    // 공격 속도
    public float AttackSpeed;
    // 공격 범위
    public int AttackRange;
    // 팀 아이디
    public ETeamID TeamID;
    // 보유 액션
    public List<Object> Actions;
    // 카드 이미지
    public Sprite CardImage; 
    // 선호 이동 방향
    public EPreferDirection PreferDirection;


    public Unit CreateUnit()
    {
        if (!Prefab)
            return null;

        GameObject unitInstance = GameObject.Instantiate<GameObject>(Prefab);
        if (!unitInstance) 
            return null;

        Unit unit = unitInstance.AddComponent(System.Type.GetType(Script.name)) as Unit;
        unit.UnitData = this;

        // 액션 스크립트 생성/등록
        foreach (Object action in Actions)
        {
            if (!action) continue;

            // 액션 생성.
            unitInstance.AddComponent(System.Type.GetType(action.name));
        }

        // 필수 컴포넌트 추가
        CapsuleCollider capsule = unitInstance.GetComponent<CapsuleCollider>();
        if (capsule)
        {
            NavMeshAgent navAgent = unitInstance.GetComponent<NavMeshAgent>();
            if (navAgent)
            {
                navAgent.radius = capsule.radius;
                navAgent.height = capsule.height;
            }
        }

        Rigidbody rb = unitInstance.GetComponent<Rigidbody>();
        if (rb)
        {
            //rb.constraints = (RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ);
        }

        return unit;
    }
}
