using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

public class SurvivorBehaviour : Human {

    /*DEBUG*/
    public bool debug = false;
    Vector3 dirdir;
    Vector3 hitpoint;
    float lastDirection;

    public float goodness = 100;
    private NavMeshAgent agent;
    /* VARIABLES PARA EL NAVMESH*/
    public GameObject WayPointList;
    private List<Transform> waypoints;
    private int actualWayPoint = 0;

    private List<string> zombieTags;
    private List<string> survivorTags;

    //En esta lista se meterán los humanos que se detecten para no volver a detectarlos en un tiempo después de realizar la acción
    private List<GameObject> detectedHumans;

    public Transform rightHand;

    /*VARIABLES PARA CAMBIAR DE ESTADO*/
    private bool idleStarted = false;
    private AIStates currentState = AIStates.Patrol;

    /* VARIABLES PARA PERSEGUIR */
    GameObject actualTarget;
    float distance = StandardConstants.SURVIVOR_DETECT_DIST;
    private bool canAttack = false;
    private bool canDoAction = false;
    private bool canDetect = true;
    private bool healing = false;
    private bool givingAmmo = false;
    private bool isAiming = false;

    // Use this for initialization
    void Start () {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        waypoints = new List<Transform>();
        zombieTags = new List<string>(new string[] { Tags.NORMAL_ZOMBIE, Tags.FAST_ZOMBIE });
        survivorTags = new List<string>(new string[] { Tags.SURVIVOR, Tags.PLAYER });
        detectedHumans = new List<GameObject>();

        foreach(Transform t in WayPointList.transform)
        {
            waypoints.Add(t);
        }

        print("Creado superviviente con " + waypoints.Count + " puntos de control");
        setState(AIStates.Patrol);
        //setState(AIStates.Rest);
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
                print("Attacking at a distance: " + distance);
                RaycastHit hit;
                
                Debug.DrawRay(rightHand.position, transform.forward*100, Color.red, -1);
                if (Physics.Raycast(rightHand.position, transform.forward, out hit, StandardConstants.DISTANCE_TO_RUNAWAY_SURVIVOR))
                {
                    if (hit.transform.gameObject == actualTarget)
                    {
                        agent.speed = 0;
                        anim.SetBool("AimWalking", false);
                        Vector3 realObjectPos = new Vector3(actualTarget.transform.position.x, transform.position.y, actualTarget.transform.position.z);
                        Vector3 relPos = realObjectPos - transform.position;
                        Quaternion rot = Quaternion.LookRotation(relPos, Vector3.up);
                        transform.rotation = rot;
                        if (!isAiming)
                        {
                            anim.SetBool("Aim", true);
                        }
                    }
                    else
                    {
                        print("No le veo");
                        anim.SetBool("AimWalking", true);
                        agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
                        Chase();
                    }
                }
                else
                {
                    agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
                    Chase();
                    anim.SetBool("AimWalking", true);
                    print("Falla todito el raycast");
                }
                //Si la distancia es menor a 100 ataca
                    //Si no está mirando al usuario rota hasta mirar
                    //Si le está mirando le dispara PUM PUM CHAS
                //Si es mayor a 100 hace un buen Chase de puta madre

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
                if (!canDoAction)
                {
                    Chase();
                }
                else
                {
                    print("Robando");
                }
                break;
            case AIStates.Heal:
                if (!canDoAction)
                {
                    Chase();
                }
                else
                {
                    agent.speed = StandardConstants.SURVIVOR_IDLE_SPEED;
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !healing)
                    {
                        anim.SetBool("Touch", true);
                        healing = true;
                        StartCoroutine("Heal");
                    }

                    print("Curando");
                }
                break;
            case AIStates.Give:
                if (ammo > 0)
                {
                    if (!canDoAction)
                    {
                        Chase();
                    }

                    else
                    {
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !givingAmmo)
                        {
                            agent.speed = StandardConstants.SURVIVOR_IDLE_SPEED;
                            anim.SetBool("Touch", true);
                            givingAmmo = true;
                            StartCoroutine("GiveAmmo");
                        }
                    }
                }
                else
                {
                    print(this.name + ": No tengo munición");
                    setState(AIStates.Patrol);
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
                {
                    print("Anim state: " + anim.GetCurrentAnimatorStateInfo(1).IsName("Walking"));
                    anim.SetTrigger("Walk");
                    anim.ResetTrigger("Walk");
                }
                idleStarted = false;
                canDoAction = false;
                StartCoroutine("checkIdle");
                break;
            case AIStates.RunAway:
                agent.speed = StandardConstants.SURVIVOR_RUNNING_SPEED;
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                {
                    anim.SetTrigger("Walk");
                    anim.ResetTrigger("Walk");
                }
                StopAllCoroutines();
                break;
            case AIStates.Rest:
                canDoAction = false;
                if(!anim.GetCurrentAnimatorStateInfo(0).IsName("LookAround"))
                    anim.SetTrigger("LookAround");
                anim.ResetTrigger("LookAround");
                break;
            case AIStates.Give:
                StopAllCoroutines();
                agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                {
                    anim.SetTrigger("Walk");
                    anim.ResetTrigger("Walk");
                }
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
            if (human.GetComponent<Human>().isWounded())
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
                //setState(AIStates.Attack);
            }
            else
            {
                setState(AIStates.Steal);
            }
        }
    }

    void OnTriggerStay(Collider collision)
    {
        print("CanDetect: " + canDetect);
        if (canDetect && !detectedHumans.Contains(collision.gameObject))
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
                            print("Distace: " + distance);
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
                            else
                            {
                                print("Distance: " + distance);
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

    public void setCanAttack(bool value)
    {
        canAttack = value;
    }

    public void setCanDoAction(bool value)
    {
        canDoAction = value;
    }

    public GameObject getTarget()
    {
        return actualTarget;
    }

    public AIStates getState()
    {
        return currentState;
    }


    IEnumerator waitIdle()
    {
        agent.speed = StandardConstants.SURVIVOR_IDLE_SPEED;
        anim.SetTrigger("LookAround");
        anim.ResetTrigger("LookAround");
        yield return new WaitForSeconds(8.45f);
        anim.SetTrigger("Walk");
        anim.ResetTrigger("Walk");
        idleStarted = false;
        agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
    }
    IEnumerator checkIdle()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.0f);
            float random = Random.Range(0.0f, 1.0f);
            if (!idleStarted && random > 0.8f)
            {
                print("Going to idle");
                idleStarted = true;
                StartCoroutine("waitIdle");
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator Heal()
    {
        canDetect = false;
        detectedHumans.Add(actualTarget);
        IEnumerator specificReset = resetDetection(30.0f, actualTarget);
        StartCoroutine(specificReset);
        yield return new WaitForSeconds(2.0f);
        //CURAR
        distance = StandardConstants.SURVIVOR_DETECT_DIST;
        anim.SetBool("Touch", false);
        setState(AIStates.Patrol);
        canDetect = true;
        healing = false;
    }

    IEnumerator GiveAmmo()
    {
        canDetect = false;
        detectedHumans.Add(actualTarget);
        IEnumerator specificReset = resetDetection(30.0f, actualTarget);
        StartCoroutine(specificReset);
        yield return new WaitForSeconds(2.0f);
        actualTarget.GetComponent<Human>().GiveAmmo(this, StandardConstants.SURVIVOR_AMMO_TO_GIVE);
        distance = StandardConstants.SURVIVOR_DETECT_DIST;
        anim.SetBool("Touch", false);
        setState(AIStates.Patrol);
        canDetect = true;
        givingAmmo = false;
        
    }

    IEnumerator resetDetection(float time, GameObject target)
    {
        yield return new WaitForSeconds(time);
        print("Removed: " + target.name);
        detectedHumans.Remove(target);
    }
}
