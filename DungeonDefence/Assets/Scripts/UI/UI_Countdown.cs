using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Countdown : MonoBehaviour
{
    protected TextMeshProUGUI textMesh;

    protected GameManager GameManager;

    protected DDGame.StageStateChangeEventHandler StageStateChangedHandler;

    void Awake()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        textMesh = GetComponent<TextMeshProUGUI>();
        textMesh.enabled = false;
        
        StageStateChangedHandler = new DDGame.StageStateChangeEventHandler(OnStageStateChanged);
        GameManager.StateChangeEvent += StageStateChangedHandler;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        GameManager.StateChangeEvent -= StageStateChangedHandler;
    }

    public void OnStageStateChanged(DDGame.EStageState newState)
    {
        if (newState == DDGame.EStageState.Fortify)
        {
            textMesh.enabled = true;
            StartCoroutine(Countdown((int)GameManager.FortifyTime));
        }
        else
        {
            textMesh.enabled = false;
        }
    }

    // 카운트다운
    public virtual IEnumerator Countdown(int amount)
    {
        for (int i = amount; i > 0; --i)
        {
            textMesh.text = i.ToString();
            yield return new WaitForSeconds(1.0f);
        }
    }
}
