using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Action_Fight : Action
{
    protected override void Awake()
    {
        base.Awake();
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
    }

    public override bool CanPlay(Unit unit)
    {
        if (base.CanPlay(unit) == false)
            return false;

        return FindEnemyOnTile(unit.TileIndex, unit);
    }

    protected override void OnBeginPlay()
    {
        base.OnBeginPlay();

        // 사생결단
        Unit enemy = FindEnemyOnTile(owner.TileIndex, owner);
        if (enemy)
        {
            StartCoroutine(AttackUnit(enemy));
        }
        else
        {
            Finish();
        }
    }

    protected override void OnEndPlay()
    {
        base.OnEndPlay();
    }

    // 특정 타일 위에 적 탐색
    protected Unit FindEnemyOnTile(int tileIndex, Unit unit)
    {
        Unit enemy = null;
        List<Unit> units = GameManager.GetUnitsOnTile(tileIndex);
        foreach (Unit other in units)
        {
            if (other && other.IsAlive() && other.IsEnemy(unit))
            {
                enemy = other;
                break;
            }
        }

        return enemy;
    }

    protected bool IsEnemy(GameObject target)
    {
        // 타겟이 없다면 아님
        if (!owner || !target) return false;

        return owner.IsEnemy(target.GetComponent<Unit>());
    }

    protected virtual IEnumerator AttackUnit(Unit target)
    {
        while (IsPlaying)
        {
            // 애니메이션 속도 조절도 필요할 것으로 예상 됨.
            if (animator)
            {
                // animator.SetFloat("Attack_Speed", 1.0f / owner.AttackSpeed);
                animator.SetTrigger("Attack");
            }

            // 공격 딜레이를 버프 등의 이유로 실시간 조절하려면 달라져야 할 수도.
            yield return new WaitForSeconds(1.0f / owner.UnitData.AttackSpeed);

            DamageData damage = new DamageData(owner);
            damage.physical = owner.UnitData.Attack;

            GameManager.NotifyHit(target, damage);

            // 공격 후 둘 중 누군가 죽었다면 종료
            if (owner.IsAlive() == false || target.IsAlive() == false)
            {
                break;
            }
        }

        // yield break;
        Finish();
    }
}
