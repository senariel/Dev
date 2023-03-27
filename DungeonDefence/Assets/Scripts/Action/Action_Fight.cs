using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Fight : Action
{
    protected Animator animator;
    protected GameManager gameManager;

    protected override void Awake()
    {
        base.Awake();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (owner)
        {
            animator = owner.GetComponentInChildren<Animator>();
        }
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
        if (base.CanPlay(unit) && IsEnemy(owner.ActionTarget))
        {
            return true;
        }

        return false;
    }

    protected override void OnBeginPlay()
    {
        base.OnBeginPlay();

        // 사생결단
        StartCoroutine(AttackUnit(owner.ActionTarget.GetComponent<Unit>()));
    }

    protected override void OnEndPlay()
    {
        base.OnEndPlay();
    }

    protected bool IsEnemy(GameObject target)
    {
        // 타겟이 없다면 아님
        if (!owner || !target) return false;

        return owner.IsEnemy(target.GetComponent<Unit>());
    }

    protected virtual IEnumerator AttackUnit(Unit target)
    {
        while (isPlaying)
        {
            // 애니메이션 속도 조절도 필요할 것으로 예상 됨.
            if (animator)
            {
                // animator.SetFloat("Attack_Speed", 1.0f / owner.AttackSpeed);
                animator.SetTrigger("Attack");
            }

            // 공격 딜레이를 버프 등의 이유로 실시간 조절하려면 달라져야 할 수도.
            yield return new WaitForSeconds(1.0f / owner.AttackSpeed);

            gameManager.NotifyHit(owner, target);

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
