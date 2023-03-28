using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DamageData
{
    public GameObject instigator;

    public int physical;
    public int magical;


    public DamageData(GameObject newInstigator)
    {
        instigator = newInstigator;

        physical = 0;
        magical = 0;
    }

    public DamageData(Unit newInstigator)
    {
        instigator = newInstigator.gameObject;

        physical = 0;
        magical = 0;
    }

    public int ApplyTo(Unit target)
    {
        // 계산식은 계속 복잡해질 예정
        return Mathf.Max(physical - target.Armor, 0);
    }
}
