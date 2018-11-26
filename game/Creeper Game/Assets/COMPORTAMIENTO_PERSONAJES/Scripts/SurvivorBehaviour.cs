using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

public class SurvivorBehaviour : MonoBehaviour {

    public float goodness = 100;
    private NavMeshAgent agent;
    private Animator anim;
    /* VARIABLES PARA EL NAVMESH*/
    public GameObject WayPointList;
    private List<Transform> waypoints;
    private int actualWayPoint = 0;

    private List<string> zombieTags;
    private List<string> survivorTags;

    /*VARIABLES PARA CAMBIAR DE ESTADO*/
    private bool idleStarted = false;
    bool checkIdleBool = true;
    private AIStates currentState = AIStates.Patrol;

    /* VARIABLES PARA PERSEGUIR */
    GameObject actualTarget;
    float distance = 100;
    float ammo = 10;

    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        waypoints = new List<Transform>();
        zombieTags = new List<string>(new string[] { Tags.NORMAL_ZOMBIE, Tags.FAST_ZOMBIE });
        survivorTags = new List<string>(new string[] { Tags.SURVIVOR, Tags.PLAYER });

        foreach(Transform t in WayPointList.transform)
        {
            waypoints.Add(t);
        }

        print("Creado superviviente con " + waypoints.Count + " puntos de control");

        agent.SetDestination(waypoints[0].position);
        StartCoroutine("checkIdle");
    }
	
	// Update is called once per frame
	void Update () {             

        CheckStateBehaviour();
	}

    private void CheckStateBehaviour()
    {
        switch (currentState)
        {
            case AIStates.Patrol:
                Patrol();
                break;
            case AIStates.Attack:
                if (Chase())
                {
                    print("He llegado");
                    actualTarget = null;
                    distance = 100;
                    anim.SetTrigger("LookAround");
                    currentState = AIStates.Patrol;
                }
                break;
            case AIStates.RunAway:
                break;
            case AIStates.Steal:
                if (Chase())
                {

                }
                break;
            case AIStates.Heal:
                if (Chase())
                {

                }
                break;
            case AIStates.Give:
                if (Chase())
                {

                }
                break;

        }
    }

    private void Patrol()
    {
        if(agent.destination != waypoints[actualWayPoint].position)
        {
            agent.destination = waypoints[actualWayPoint].position;
        }

        if (CheckPoint())
        {
            actualWayPoint++;
            if (actualWayPoint == waypoints.Count)
            {
                actualWayPoint = 0;
            }
            agent.SetDestination(waypoints[actualWayPoint].position);
        }
    }

    private bool CheckPoint()
    {
        bool inCheckpoint = false;
        if(agent.remainingDistance < 0.3)
        {
            inCheckpoint = true;
        }
        return inCheckpoint;
    }

    private bool Chase()
    {
        checkIdleBool = false;
        bool isClose = false;
        if (agent.destination != actualTarget.transform.position)
        {
            agent.SetDestination(actualTarget.transform.position);
        }
        if (agent.remainingDistance<1)
        {
            isClose = true;
        }
        return isClose;
    }

    private void DetectZombi()
    {
        if (ammo > 0)
        {
            currentState = AIStates.Attack;
        }
        else
        {
            currentState = AIStates.RunAway;
        }
    }

    private void DetectHuman(GameObject human)
    {
        if (goodness > 50)
        {
            if (true)
            {
                //Comprobamos si el jugador tiene menos del 100% de vida
                currentState = AIStates.Heal;
            }
            else
            {
                currentState = AIStates.Give;
            }
        }
        else
        {
            if (goodness < 30)
            {
                currentState = AIStates.Attack;
            }
            else
            {
                currentState = AIStates.Steal;
            }
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (zombieTags.Contains(collision.gameObject.tag) || survivorTags.Contains(collision.gameObject.tag))
        {
            Vector3 direction = collision.gameObject.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);
            if (angle < 30.0f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction, out hit, Vector3.Distance(collision.gameObject.transform.position, transform.position)))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                    if (zombieTags.Contains(hit.transform.tag) || survivorTags.Contains(hit.transform.tag))
                    {
                        float d = Vector3.Distance(collision.gameObject.transform.position, transform.position);
                        if (d < distance)
                        {
                            actualTarget = hit.transform.gameObject;
                            if (zombieTags.Contains(hit.transform.tag))
                            {
                                //Hemos detectado un zombie
                                DetectZombi();

                            }
                            else
                            {
                                //Hemos detectado un humano
                                DetectHuman(hit.transform.gameObject);
                            }
                            print("Detectado " + collision.gameObject.tag);

                        }
                    }
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                    Debug.Log("Did not Hit");
                }
            }

        }
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
