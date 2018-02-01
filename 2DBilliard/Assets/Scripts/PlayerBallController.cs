using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBallController : BallController
{
    private List<GameObject> TouchedObjectList;

    // Use this for initialization
    void Start()
    {
        TouchedObjectList = new List<GameObject>();
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
}
