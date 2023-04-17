using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DDGame;


public class Tile_Finish : Tile
{
    private GetTileHandler TileHandler;

    protected override void Awake()
    {
        base.Awake();

        TileHandler = new GetTileHandler(GetTile);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        GameManager.GetFinishTileEvent += TileHandler;
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

    protected override void OnDisable()
    {
        GameManager.GetFinishTileEvent -= TileHandler;

        base.OnDisable();
    }

    public override bool CanEnter(Unit unit)
    {
        return true;
    }
    
    public override void Enter(Unit unit)
    {
        base.Enter(unit);

        // gameManager.UnitReachedFinish(unit);
    }

    public Tile GetTile()
    {
        return this;
    }
}
