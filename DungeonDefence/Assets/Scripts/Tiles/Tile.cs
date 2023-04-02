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

    // 배치된 타일의 인덱스. Bottom 과 Upper 의 인덱스가 중복되는 것에 주의.
    public int TileIndex {get; set;}

    protected GameManager gameManager = null;


    protected virtual void Awake()
    {
        
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        GameObject obj = GameObject.Find("GameManager");
        gameManager = obj?.GetComponent<GameManager>();
        tileManager = gameManager?.TileManager;
    }

    // Update is called once per frame
    protected virtual void Update()
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

    public virtual bool CanEnter(Unit unit) { return true; }
    public virtual void Enter(Unit unit)  {}
}
