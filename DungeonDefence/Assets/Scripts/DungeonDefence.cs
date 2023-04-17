using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DDGame
{
    public enum EStageState
    {
        None,
        Fortify,
        Invade,
        End
    }

    public enum ETeamID
    {
        Defence,
        Offence,
        NoTeam,
    }

    public enum ETileType
    {
        Block,      // 파괴가능한 블럭
        Start,      // 시작지점
        Finish,     // 종료지점
    }

    public delegate void StageStateChangeEventHandler(EStageState newState);

    public delegate Tile GetTileHandler();
}