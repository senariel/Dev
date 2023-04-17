using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Tile : MonoBehaviour
{
    [HideInInspector] public TileData TileData;

    // 파괴 가능 여부
    public bool IsBreakable { get; private set; }

    // 배치된 타일의 인덱스. Bottom 과 Upper 의 인덱스가 중복되는 것에 주의.
    public int TileIndex { get; set; }

    public GameManager GameManager { get; private set; }
    public TileManager TileManager { get; private set; }


    protected virtual void Awake()
    {
        IsBreakable = (TileData.IsBreakable && gameObject.layer == LayerMask.NameToLayer("Tile_Upper"));

        GameManager = GameObject.Find("GameManager").GetComponentInChildren<GameManager>();
        TileManager = GameObject.Find("GameManager").GetComponentInChildren<TileManager>();
    }

    protected virtual void OnEnable()
    {

    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
    }

    // Update is called once per frame
    protected virtual void Update()
    {
    }

    protected virtual void OnDisable()
    {

    }

    // 터치 시
    private void OnMouseDown()
    {
        // 파괴
        if (IsBreakable && TileManager)
        {
            // 타일매니저에게 보고
            TileManager.NotifyBreakTile(this);
        }
    }

    // from TileManager
    // 파괴
    public void Break()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;

        StartCoroutine(OnBreak());
    }

    // 파괴 연출
    protected IEnumerator OnBreak()
    {
        GameObject FXObj = Instantiate(TileData.BreakFX, transform.position, TileData.BreakFX.transform.rotation);
        if (FXObj)
        {
            ParticleSystem fx = TileData.BreakFX.GetComponent<ParticleSystem>();
            if (fx)
            {
                fx.Play(true);

                yield return new WaitForSeconds(fx.main.duration);

                fx.Stop();
                Destroy(FXObj);
            }
        }

        Destroy(gameObject);
    }

    public virtual bool CanEnter(Unit unit) { return false; }
    public virtual void Enter(Unit unit) { }
}
