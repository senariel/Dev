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
        Debug.Log("OnBeginDrag : " + eventData.position);

        // 연출
        rectTransform.anchoredPosition += new Vector2(0.0f, 50.0f);

        unitInstance = Instantiate(unitPrefab);
        unitInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        UpdateUnitPosition(unitInstance, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrop : " + eventData.position);

        // 연출
        rectTransform.anchoredPosition -= new Vector2(0.0f, 50.0f);

        Tile tile = GetTileAtPoint(eventData.position);
        if (tile)
        {
            unitInstance.GetComponent<Rigidbody>().constraints = unitPrefab.GetComponent<Rigidbody>().constraints;

            gameManager.SpawnUnitOnTile(unitPrefab, tile.TileIndex, unitInstance);
        }
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateUnitPosition(unitInstance, eventData.position);
    }

    protected bool UpdateUnitPosition(GameObject unit, Vector2 screenPoint)
    {
        if (!unit) return false;
        
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            Vector3 position = hitInfo.point;
            position.y += unit.GetComponent<CapsuleCollider>().height * 0.5f;

            unit.transform.position = position;
        }
        else
        {
            unit.transform.position = ray.origin * 10.0f;
        }

        return true;
    }

    protected Tile GetTileAtPoint(Vector2 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            return hitInfo.collider.GetComponent<Tile>();
        }
        return null;
    }
}
