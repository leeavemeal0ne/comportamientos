using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SurvivorBehaviour : MonoBehaviour {

    public float goodness = 100;
    private NavMeshAgent agent;
    private Animator anim;
    /* VARIABLES PARA EL NAVMESH*/
    public List<Transform> waypoints;
    private int actualWayPoint = 0;

    /*VARIABLES PARA CAMBIAR DE ESTADO*/
    private bool idleStarted = false;
    bool checkIdleBool = true;

    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.SetDestination(waypoints[0].position);
        StartCoroutine("checkIdle");
    }
	
	// Update is called once per frame
	void Update () {
        if (checkPoint())
        {
            actualWayPoint++;
            if(actualWayPoint == waypoints.Count)
            {
                actualWayPoint = 0;
            }
            agent.SetDestination(waypoints[actualWayPoint].position);
        }
	}

    private bool checkPoint()
    {
        bool inCheckpoint = false;
        if(agent.remainingDistance < 0.3)
        {
            inCheckpoint = true;
        }
        return inCheckpoint;
    }

    IEnumerator waitIdle()
    {
        print("Starting idle");
        anim.SetTrigger("LookAround");
        yield return new WaitForSeconds(8.45f);
        print("Ending idle");
        idleStarted = false;
        agent.speed = 1;
    }   
    IEnumerator checkIdle()
    {
        while (checkIdleBool)
        {
            print("Voy a esperar");
            yield return new WaitForSeconds(5.0f);
            print("He esperado");
            float random = Random.Range(0.0f, 1.0f);
            print(random);
            if (!idleStarted && random > 0.8f)
            {
                print("Going to idle");
                idleStarted = true;
                agent.speed = 0;
                StartCoroutine("waitIdle");
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
