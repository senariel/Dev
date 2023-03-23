using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    protected UnityEngine.UI.ScrollRect rect;
    protected GameManager gameManager = null;

    void Awake()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (!gm)
            return;

        gameManager = gm.GetComponent<GameManager>();
        if (gameManager)
        {
            gameManager.Inventory = this;
        }

        rect = GetComponent<UnityEngine.UI.ScrollRect>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //rect.content
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
