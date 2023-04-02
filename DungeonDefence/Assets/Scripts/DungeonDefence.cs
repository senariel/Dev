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
        Block,
        Start,
        Finish,
    }

    public delegate void StageStateChangeEventHandler(EStageState newState);
}