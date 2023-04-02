using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Countdown : MonoBehaviour
{
    protected GameManager GameManager;

    protected DDGame.StageStateChangeEventHandler StageStateChangedHandler;

    void Awake()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
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

    public void OnStageStateChanged(DDGame.EStageState newState)
    {
        if (newState == DDGame.EStageState.Fortify)
        {
            Countdown(5);
        }
    }

     // 카운트다운
    public virtual IEnumerator Countdown(int amount)
    {
        for (int i = amount; i > 0; --i)
        {
            Debug.Log("Countdown : " + i);
            yield return new WaitForSeconds(1.0f);
        }

        yield break;
    }
}
