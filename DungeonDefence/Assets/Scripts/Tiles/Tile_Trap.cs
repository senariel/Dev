using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile_Trap : Tile
{
    // 함정 내구도
    public int Duration;
    // 타격량
    public int Damage;
    // 수리 시간
    public float RepairTime;

    protected int currentDuration;

    protected override void Awake()
    {
        base.Awake();

        currentDuration = Duration;
    }

    public override void Enter(Unit unit)
    {
        base.Enter(unit);

        if (currentDuration > 0)
        {
            DamageData damage = new DamageData(gameObject);
            damage.physical = Damage;

            // 타격
            GameManager.NotifyHit(unit, damage);

            StartCoroutine(ReduceDuration(1));
        }
    }

    // 내구도 감소
    protected IEnumerator ReduceDuration(int amount)
    {
        currentDuration -= amount;
        if (Duration <= 0)
        {
            yield return new WaitForSeconds(RepairTime);

            currentDuration = Duration;
        }

        yield break;
    }
}
