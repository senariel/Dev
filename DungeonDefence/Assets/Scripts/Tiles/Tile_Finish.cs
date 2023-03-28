using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile_Finish : Tile
{
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

    public override void Enter(Unit unit)
    {
        base.Enter(unit);

        // gameManager.UnitReachedFinish(unit);
    }
}
