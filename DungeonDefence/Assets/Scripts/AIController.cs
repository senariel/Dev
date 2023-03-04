using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private GameObject Destination;
    private UnityEngine.AI.NavMeshAgent NavAgent;

    // Start is called before the first frame update
    void Start()
    {
        Destination = GameObject.FindGameObjectWithTag("Finish");
        if (Destination != null)
        {
            Debug.Log("I found Dest.");

            NavAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (NavAgent != null)
            {
                Debug.Log("I'm going to dest");
                NavAgent.SetDestination(Destination.transform.position);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
