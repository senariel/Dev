using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private bool bTouchedByCueBall;

    // Use this for initialization
    void Start()
    {
        bTouchedByCueBall = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("CueBall"))
            bTouchedByCueBall = true;
    }

    public void OnTurnStart()
    {
        bTouchedByCueBall = false;
    }

    public void OnTurnEnd()
    {

    }

    public bool IsTouchedByCueBall()
    {
        return bTouchedByCueBall;
    }
}
