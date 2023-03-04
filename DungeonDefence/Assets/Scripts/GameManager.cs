using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject StartArea, FinishArea = null;

    public List<GameObject> EnemyList = new List<GameObject>();

    private bool bPlayable = false;

    private List<GameObject> EnemyInstanceList = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        if (StartArea == null)
        {
            StartArea = GameObject.FindGameObjectWithTag("AREA_START");
        }

        if (FinishArea == null)
        {
            FinishArea = GameObject.FindGameObjectWithTag("AREA_FINISH");
        }

        bPlayable = (StartArea != null && FinishArea != null && EnemyList.Count > 0);

        Debug.Log("[Awake] " + bPlayable);
    }

    void Start()
    {
        if (!bPlayable)
            return;

        Debug.Log("[Start]");

        // 적 생성
        SpawnEnemyWave();

        // 웨이브 시작
        StartWave();
    }

    // Update is called once per frame
    void Update()
    {

    }

    GameObject GetNextEnemyGameObject()
    {
        GameObject Ret = null;

        if (EnemyList.Count > 0)
        {
            Ret = EnemyList[0];
        }

        Debug.Log("[GetNextEnemyGameObject] " + Ret);

        return Ret;
    }

    bool SpawnEnemyWave()
    {
        Debug.Log("[SpawnEnemyWave]");

        // 적당한 공간 찾기는 나중에 하자.

        foreach (GameObject Enemy in EnemyList)
        {
            Debug.Log("\tInstantiate [" + Enemy + "] " + Enemy.transform.forward + " / " + StartArea.transform.forward + " / " + Vector3.forward);

            if (Enemy != null)
            {
                GameObject EnemyInstance = GameObject.Instantiate<GameObject>(
                    Enemy, 
                    StartArea.transform.position, 
                    StartArea.transform.rotation);

                if (EnemyInstance != null)
                {
                    EnemyInstanceList.Add(EnemyInstance);
                }
            }
        }

        return (EnemyInstanceList.Count > 0);
    }

    void StartWave()
    {
        Debug.Log("[StartWave]");

        foreach (GameObject Enemy in EnemyInstanceList)
        {
            if (Enemy == null)
                continue;

            UnityEngine.AI.NavMeshAgent NavAgent = Enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();

            Debug.Log("\t" + Enemy + " / " + NavAgent);

            if (NavAgent == null)
                continue;

            NavAgent.SetDestination(FinishArea.transform.position);
        }
    }
}
