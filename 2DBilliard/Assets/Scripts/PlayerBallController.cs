using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBallController : BallController
{
    public GameObject TouchEffect_Begin, TouchEffect_End;

    private List<GameObject> TouchedObjectList;
    private GameObject TouchEffect_Begin_Clone, TouchEffect_End_Clone;

    // Use this for initialization
    void Start()
    {
        TouchedObjectList = new List<GameObject>();

        if (TouchEffect_Begin != null)
        {
            TouchEffect_Begin_Clone = MonoBehaviour.Instantiate<GameObject>(TouchEffect_Begin);
            if (TouchEffect_Begin_Clone != null)
            {
                TouchEffect_Begin_Clone.SetActive(false);
            }
        }
        if (TouchEffect_End != null)
        {
            TouchEffect_End_Clone = MonoBehaviour.Instantiate<GameObject>(TouchEffect_End);
            if (TouchEffect_End_Clone != null)
            {
                TouchEffect_End_Clone.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<GameObject> GetTouchedObjectsList()
    {
        return TouchedObjectList;
    }

    override public void OnTurnStateChanged(GameController.ETurn NewTurnState)
    {
        if (NewTurnState == GameController.ETurn.Action)
            TouchedObjectList.Clear();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TouchedObjectList.Add(collision.gameObject);
    }

    public void OnTouchStart(Vector2 TouchedPosition)
    {
        if (TouchEffect_Begin_Clone == null || TouchEffect_End_Clone == null)
            return;

        TouchEffect_Begin_Clone.transform.position = (Vector3)TouchedPosition;
        TouchEffect_Begin_Clone.SetActive(true);

        TouchEffect_End_Clone.transform.position = (Vector3)TouchedPosition;
        TouchEffect_End_Clone.SetActive(true);
    }

    public void OnTouchMove(Vector2 TouchedPosition)
    {
        if (TouchEffect_End_Clone == null)
            return;

        TouchEffect_End_Clone.transform.position = (Vector3)TouchedPosition;
    }

    public void OnTouchEnd(Vector2 TouchedPosition)
    {
        if (TouchEffect_Begin_Clone == null || TouchEffect_End_Clone == null)
            return;

        TouchEffect_Begin_Clone.SetActive(false);
        TouchEffect_End_Clone.SetActive(false);
    }
}
