using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    // 카드 드로우로 생성할 유닛의 프리팹
    public GameObject unitPrefab;

    protected RectTransform rectTransform;

    // 생성된 유닛. 임시
    protected GameObject unitInstance = null;
    protected GameManager gameManager = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnClick : " + gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 연출
        rectTransform.anchoredPosition += new Vector2(0.0f, 50.0f);

        unitInstance = Instantiate(unitPrefab);
        unitInstance.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        unitInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        
        UpdateUnitPosition(unitInstance, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 연출
        rectTransform.anchoredPosition -= new Vector2(0.0f, 50.0f);

        Tile tile = GetTileAtPoint(eventData.position);
        if (tile)
        {
            unitInstance.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
            unitInstance.GetComponent<Rigidbody>().constraints = unitPrefab.GetComponent<Rigidbody>().constraints;

            gameManager.SpawnUnitOnTile(unitPrefab, tile.TileIndex, unitInstance);

            unitInstance = null;
            Destroy(gameObject);
        }
        else
        {
            Destroy(unitInstance);
            unitInstance = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateUnitPosition(unitInstance, eventData.position);
    }

    protected bool UpdateUnitPosition(GameObject unit, Vector2 screenPoint)
    {
        if (!unit) return false;
        
        // 유닛은 통과해서 레이캐스트
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Unit"))))
        {
            // 바닥 타일에 닿았을 때만 유효
            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Tile_Bottom"))
            {
                Vector3 position = hitInfo.point;
                position.y += unit.GetComponent<CapsuleCollider>().height * 0.5f;

                unit.transform.position = position;

                return true;
            }
        }
        
        unit.transform.position = ray.origin + (ray.direction * 10.0f);

        return false;
    }

    protected Tile GetTileAtPoint(Vector2 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Unit"))))
        {
            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Tile_Bottom"))
            {
                return hitInfo.collider.GetComponent<Tile>();
            }
        }

        return null;
    }
}
