using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // 파괴 가능 여부
    public bool breakable = false;
    public GameObject breakFX;

    private TileManager tileManager;
    private ParticleSystem fx;

    void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject obj = GameObject.Find("GameManager");
        GameManager gm = obj?.GetComponent<GameManager>();
        tileManager = gm?.GetTileManager();
    }

    // Update is called once per frame
    void Update()
    {
        if (fx)
        {
            Debug.Log("###");
            //Destroy(fx.gameObject);
            //Destroy(gameObject);
        }
    }

    private void OnMouseDown()
    {
        if (breakable)
        {
            Break();
        }
    }

    private void Break()
    {
        if (tileManager)
        {
            tileManager.BreakTile(this);
        }
    }
}
