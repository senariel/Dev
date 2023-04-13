using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using DDGame;


public class UI_GameState : MonoBehaviour
{
    protected TextMeshProUGUI textMesh;

    protected GameManager GameManager;

    protected DDGame.StageStateChangeEventHandler StageStateChangedHandler;

    void Awake()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StageStateChangedHandler = new DDGame.StageStateChangeEventHandler(OnStageStateChanged);
        GameManager.StateChangeEvent += StageStateChangedHandler;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        GameManager.StateChangeEvent -= StageStateChangedHandler;
    }

    void OnStageStateChanged(EStageState newState)
    {
        textMesh.text = "StageState : " + newState.ToString();
    }
}
