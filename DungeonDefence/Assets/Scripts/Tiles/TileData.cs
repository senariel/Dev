using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DDGame;

public class TileData : ScriptableObject
{
    public GameObject Prefab;
    public string TileName;
    public Vector3 Forward;
    public ETileType Type;
}
