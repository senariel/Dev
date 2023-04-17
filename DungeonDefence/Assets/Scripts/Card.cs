using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public GameObject ImageObject;

    // 카드 드로우로 생성할 유닛의 프리팹
    public UnitData UnitData { get; private set; }

    protected RectTransform rectTransform;

    // 생성된 유닛. 임시
    protected Unit unitInstance = null;
    protected GameManager gameManager = null;
    protected Image image;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        image = ImageObject?.GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetUnitData(UnitData unitData)
    {
        UnitData = unitData;

        image.sprite = UnitData.CardImage;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnClick : " + gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log("OnBeginDrag " + UnitData);

        // 연출
        rectTransform.anchoredPosition += new Vector2(0.0f, 50.0f);

        unitInstance = gameManager.SpawnUnitOnTile(UnitData, -1, DDGame.ETeamID.Defence);
        if (!unitInstance)
        {
            Debug.Log("\tFailed to instantiate unit.");
        }

        UpdateUnitPosition(unitInstance, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 연출
        rectTransform.anchoredPosition -= new Vector2(0.0f, 50.0f);

        Tile tile = GetTileAtPoint(eventData.position);
        if (tile)
        {
            gameManager.PlaceUnitOnTile(unitInstance, tile.TileIndex);

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

    protected bool UpdateUnitPosition(Unit unit, Vector2 screenPoint)
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
