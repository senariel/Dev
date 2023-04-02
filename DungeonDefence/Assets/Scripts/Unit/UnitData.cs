using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DDGame;

[CreateAssetMenu(fileName = "UnitData", menuName = "ScriptableObject/UnitData", order = 1)]
public class UnitData : ScriptableObject
{
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
}
