using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DDGame;

[CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObject/TileData", order = 1)]
public class TileData : ScriptableObject
{
    public Object Script;
    public GameObject Prefab;
    // public string TileName;
    // public Vector3 Forward;
    // public ETileType Type;
    public bool IsBreakable;
    public GameObject BreakFX;
    public bool UseCustomLayer;
    [SerializeField, LayerAttribute] public int CustomLayer;


    public Tile CreateTile(Vector3 size)
    {
        GameObject obj = Prefab ? GameObject.Instantiate(Prefab) : new GameObject();

        Tile script = obj.AddComponent(System.Type.GetType(Script.name)) as Tile;
        if (!script)
        {
            DestroyImmediate(obj);
            script = null;
        }
        else
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            Bounds bounds = renderer ? renderer.localBounds : new Bounds(Vector3.zero, size);
            BoxCollider box = obj.GetComponent<BoxCollider>();
            box.center = bounds.center;
            box.size = bounds.size;

            script.TileData = this;
        }

        return script;
    }
}
