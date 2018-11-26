using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

public class SurvivorBehaviour : MonoBehaviour {

    /*DEBUG*/
    public bool debug = false;
    Vector3 dirdir;
    Vector3 hitpoint;
    float lastDirection;

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
    private AIStates currentState = AIStates.Patrol;

    /* VARIABLES PARA PERSEGUIR */
    GameObject actualTarget;
    float distance = 100;
    float ammo = 0;

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
        setState(AIStates.Patrol);
    }
	
	// Update is called once per frame
	void Update () {             

        CheckStateBehaviour();
	}

    private void CheckStateBehaviour()
    {
        print("State: " + currentState);
        //print("Target: " + actualTarget.name);
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
                    setState(AIStates.Patrol);
                }
                break;
            case AIStates.RunAway:
                float distanceToTarget = Vector3.Distance(transform.position, actualTarget.transform.position);
                distance = distanceToTarget;
                if(distanceToTarget < StandardConstants.DISTANCE_TO_RUNAWAY_SURVIVOR)
                {
                    RunAway();
                }
                else
                {
                    print("He terminado de huir");
                    setState(AIStates.Rest);
                }

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
            case AIStates.Rest:
                break;

        }
    }

    private void setState(AIStates state)
    {
        switch (state)
        {
            case AIStates.Patrol:
                agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                    anim.SetTrigger("Walk");
                idleStarted = false;
                StartCoroutine("checkIdle");
                break;
            case AIStates.RunAway:
                agent.speed = StandardConstants.SURVIVOR_RUNNING_SPEED;
                //anim.SetTrigger("Walk");
                StopAllCoroutines();
                break;
            case AIStates.Rest:
                if(!anim.GetCurrentAnimatorStateInfo(0).IsName("LookAround"))
                    anim.SetTrigger("LookAround");
                break;
            default:
                StopAllCoroutines();
                break;
        }
        currentState = state;
    }

    private void Patrol()
    {
        if(agent.destination != waypoints[actualWayPoint].position)
        {
            agent.SetDestination(waypoints[actualWayPoint].position);
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

    
    private void RunAway()
    {
        float distanceUnits = Vector3.Distance(transform.position, actualTarget.transform.position);
        if (Mathf.Abs(lastDirection-distanceUnits)>3.0f || agent.remainingDistance<0.3)
        {
            lastDirection = distanceUnits;
            Vector3 targetDist = transform.position - actualTarget.transform.position;
            float randAngle = Random.Range(0f, 45f);
            targetDist = Quaternion.AngleAxis(randAngle, Vector3.up) * targetDist;
            Vector3 newDir = transform.position  +(targetDist.normalized * 2);
            NavMeshHit hit;
            NavMesh.SamplePosition(newDir, out hit, 5.0f, NavMesh.AllAreas);
            dirdir = newDir;
            hitpoint = hit.position;
            agent.SetDestination(hit.position);
        }
        //Gizmos.DrawSphere(hit.position, 0.5f);
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(agent.destination, 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(dirdir, 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hitpoint, 0.5f);
        }
        
    }

    private void DetectZombi()
    {
        if (ammo > 0)
        {
            setState(AIStates.Attack);
        }
        else
        {
            setState(AIStates.RunAway);
        }
    }

    private void DetectHuman(GameObject human)
    {
        if (goodness > 50)
        {
            if (true)
            {
                //Comprobamos si el jugador tiene menos del 100% de vida
                setState(AIStates.Heal);
            }
            else
            {
                setState(AIStates.Give);
            }
        }
        else
        {
            if (goodness < 30)
            {
                setState(AIStates.Attack);
            }
            else
            {
                setState(AIStates.Steal);
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
                            StopAllCoroutines();
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
        agent.speed = StandardConstants.SURVIVOR_IDLE_SPEED;
        anim.SetTrigger("LookAround");
        yield return new WaitForSeconds(8.45f);
        anim.SetTrigger("Walk");
        print("Ending idle");
        idleStarted = false;
        agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
    }   
    IEnumerator checkIdle()
    {
        while (true)
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
                StartCoroutine("waitIdle");
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
