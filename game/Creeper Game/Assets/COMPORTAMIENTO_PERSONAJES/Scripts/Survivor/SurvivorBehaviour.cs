using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;
using comportamiento_personajes;

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
    public List<GameObject> allWaypointList;

    private List<Transform> waypoints;
    private int actualWayPoint = 0;

    [HideInInspector]
    public List<string> zombieTags;
    [HideInInspector]
    public List<string> survivorTags;

    //En esta lista se meterán los humanos que se detecten para no volver a detectarlos en un tiempo después de realizar la acción
    [HideInInspector]
    public List<GameObject> detectedHumans;

    public Transform head;

    /*VARIABLES PARA CAMBIAR DE ESTADO*/
    private bool idleStarted = false;
    private AIStates currentState = AIStates.Patrol;

    /* VARIABLES PARA PERSEGUIR */
    [HideInInspector]
    public GameObject actualTarget;

    public LayerMask mask;

    [HideInInspector]
    public float distance = StandardConstants.SURVIVOR_DETECT_DIST;

    private bool canAttack = false;
    private bool canDoAction = false;
    [HideInInspector]
    public bool canDetect = true;

    private bool healing = false;
    private bool givingAmmo = false;
    private bool isAiming = false;
    private bool finishedAiming = false;
    bool isShooting = false;

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
        if (!startIdle)
        {
            setState(AIStates.Patrol);
        }
        //setState(AIStates.Rest);
    }
	
	// Update is called once per frame
	void Update () {
        if (getIsDead())
        {
            StopAllCoroutines();
            return;
        }

        CheckStateBehaviour();
	}

    private void CheckStateBehaviour()
    {
        print(name + " State: " + currentState + " target: " + actualTarget);
        //print("Target: " + actualTarget.name);
        switch (currentState)
        {
            case AIStates.Patrol:
                Patrol();
                break;
            case AIStates.Attack:                
                if (CheckRayTarget())
                {
                    agent.speed = 0;
                    anim.SetBool("AimWalking", false);
                    Vector3 realObjectPos = new Vector3(actualTarget.transform.position.x, transform.position.y, actualTarget.transform.position.z);
                    Vector3 relPos = realObjectPos - transform.position;
                    Quaternion rot = Quaternion.LookRotation(relPos, Vector3.up);
                    transform.rotation = rot;
                    //print("Finished Aiming " + finishedAiming);
                    if (!isAiming)
                    {
                        isAiming = true;
                        anim.SetBool("Aim", true);
                        agent.speed = 0;
                    }
                    if (finishedAiming && !isShooting)
                    {
                        isShooting = true;
                        print("Starting shooting corroutine");
                        StartCoroutine("Shoot");
                    }
                }
                else if(finishedAiming)
                {
                    if (actualTarget.GetComponent<Zombie>().getIsDead())
                    {
                        print("Ha muerto");
                        distance = StandardConstants.SURVIVOR_DETECT_DIST;
                        setState(AIStates.Patrol);
                    }
                    //finishedAiming = false;
                    anim.SetBool("AimWalking", true);
                    agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
                    Chase();
                    isShooting = false;
                    StopAllCoroutines();
                }
                else
                {
                    print(finishedAiming);
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
                    StartCoroutine("StopRunaway");
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
            case AIStates.Steal:
                print("CanDoAction: " + canDoAction);
                if (!canDoAction)
                {
                    Chase();
                }
                else
                {
                    print("Not chasing: " + givingAmmo);
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !givingAmmo)
                    {
                        agent.speed = StandardConstants.SURVIVOR_IDLE_SPEED;
                        ResetAnimator();
                        anim.SetBool("Touch", true);
                        givingAmmo = true;
                        StartCoroutine("StealAmmo");
                    }
                }
                break;
            case AIStates.Rest:
                print("Descansando");
                break;

        }
    }

    private void setState(AIStates state)
    {
        if (currentState != state)
        {
            anim.SetBool("Idle", false);
            canDoAction = false;
            switch (state)
            {
                case AIStates.Patrol:
                    agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                    {
                        anim.SetBool("Aim", false);
                        anim.SetBool("AimWalking", false);
                        ResetAnimator();
                        anim.SetTrigger("Walk");
                    }
                    idleStarted = false;
                    canDoAction = false;
                    StartCoroutine("checkIdle");
                    break;
                case AIStates.RunAway:
                    agent.speed = StandardConstants.SURVIVOR_RUNNING_SPEED;
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                    {
                        ResetAnimator();
                        anim.SetTrigger("Walk");
                    }
                    StopAllCoroutines();
                    break;
                case AIStates.Rest:
                    canDoAction = false;
                    agent.speed = 0;
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        anim.SetBool("Idle", true);
                    break;
                case AIStates.Give:
                    givingAmmo = false;
                    StopAllCoroutines();
                    agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                    {
                        ResetAnimator();
                        anim.SetTrigger("Walk");
                    }
                    break;
                case AIStates.Steal:
                    ResetAnimator();
                    print("Stealing to: " + actualTarget.name);
                    givingAmmo = false;
                    StopAllCoroutines();
                    if (actualTarget.GetComponent<Human>().getAmmo() <= 0)
                    {
                        setState(AIStates.RunAway);
                    }
                    else
                    {
                        agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                        {
                            ResetAnimator();
                            anim.SetTrigger("Walk");
                        }
                    }
                    break;
                case AIStates.Attack:
                    if (currentState != AIStates.Attack)
                    {
                        isAiming = false;
                        finishedAiming = false;
                        agent.speed = 0;
                    }
                    break;
                default:
                    StopAllCoroutines();
                    break;
            }
        }
        currentState = state;
    }

    private void Patrol()
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(waypoints[actualWayPoint].position, path);
        agent.SetPath(path);
        //agent.SetDestination(waypoints[actualWayPoint].position);


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

    private bool CheckRayTarget()
    {
        bool isInLine = false;

        RaycastHit hit;

        RaycastHit[] hits;

        Vector3 direction = actualTarget.transform.position - head.position;

        //print("ActualTargetRay: " + actualTarget);
        Debug.DrawRay(head.position, direction, Color.red, -1);
        bool wall = false;
        hits = Physics.RaycastAll(head.position, direction, Vector3.Distance(actualTarget.transform.position, head.position));
        for(int i = 0; i<hits.Length; i++)
        {
            if(hits[i].transform.tag == Tags.WALL)
            {
                wall = true;
            }
            if(!wall && hits[i].transform.gameObject == actualTarget)
            {
                isInLine = true;
            }
        }
        if (Vector3.Distance(actualTarget.transform.position, transform.position)<=1)
        {
            isInLine = true;
        }

        return isInLine;
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


    public void DetectZombi()
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

    public void DetectHuman(GameObject human)
    {

        SurvivorBehaviour surv = human.GetComponent<SurvivorBehaviour>();
        if (surv != null)
        {
            human.GetComponent<SurvivorBehaviour>().ReceiveContact(this.gameObject);
        }
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
                setState(AIStates.Attack);
                print(name + " attacking " + actualTarget.name);
            }
            else
            {
                setState(AIStates.Steal);
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

    public void FinishAiming()
    {
        this.finishedAiming = true;
        print("finishedAiming");
    }

    private void ResetAnimator()
    {
        anim.SetBool("Touch", false);
        anim.SetBool("Idle", false);
        anim.ResetTrigger("LookAround");
        anim.ResetTrigger("Walk");
        anim.ResetTrigger("Shoot");
        anim.ResetTrigger("Die");
        anim.ResetTrigger("GetShot");
    }

    public void ReceiveContact(GameObject human)
    {
        StopAllCoroutines();
        IEnumerator specificReset = resetDetection(30.0f, human);
        StartCoroutine(specificReset);
        setState(AIStates.Rest);
        StartCoroutine("StopIdle");
    }

    IEnumerator StopIdle()
    {
        yield return new WaitForSeconds(4.0f);
        setState(AIStates.Patrol);
    }

    IEnumerator waitIdle()
    {
        agent.speed = StandardConstants.SURVIVOR_IDLE_SPEED;
        ResetAnimator();
        anim.SetTrigger("LookAround");
        yield return new WaitForSeconds(8.45f);
        ResetAnimator();
        anim.SetTrigger("Walk");
        idleStarted = false;
        agent.speed = StandardConstants.SURVIVOR_WALKING_SPEED;
    }
    IEnumerator checkIdle()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.0f);
            float random = Random.Range(0.0f, 1.0f);
            if (!idleStarted && random > 0.5f)
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
        IEnumerator specificReset = resetDetection(30.0f, actualTarget);
        StartCoroutine(specificReset);
        yield return new WaitForSeconds(2.0f);
        actualTarget.GetComponent<Human>().GiveAmmo(this, StandardConstants.SURVIVOR_AMMO_TO_GIVE);
        distance = StandardConstants.SURVIVOR_DETECT_DIST;
        anim.SetBool("Touch", false);
        setState(AIStates.RunAway);
        canDetect = true;
        givingAmmo = false;        
    }



    IEnumerator StealAmmo()
    {
        canDetect = false;
        IEnumerator specificReset = resetDetection(30.0f, actualTarget);
        StartCoroutine(specificReset);
        yield return new WaitForSeconds(2.0f);
        GetComponent<Human>().GiveAmmo(actualTarget.GetComponent<Human>(), StandardConstants.SURVIVOR_AMMO_TO_GIVE);
        distance = StandardConstants.SURVIVOR_DETECT_DIST;
        anim.SetBool("Touch", false);
        setState(AIStates.RunAway);
        canDetect = true;
        givingAmmo = false;
    }

    IEnumerator resetDetection(float time, GameObject target)
    {
        if (!detectedHumans.Contains(target))
        {
            detectedHumans.Add(target);
        }
        yield return new WaitForSeconds(time);
        print("Removed: " + target.name);
        detectedHumans.Remove(target);
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        print(name + ": PUM");
        ResetAnimator();
        anim.SetTrigger("Shoot");
        actualTarget.GetComponent<Zombie>().TakeDamage(25);
        yield return new WaitForSeconds(2.0f);
        if (actualTarget.GetComponent<Zombie>().getIsDead())
        {
            print("Ha muerto");
            setState(AIStates.Patrol);
        }
        else if (CheckRayTarget())
        {
            StartCoroutine("Shoot");
            print("Restarting");
        }
        else
        {
            print("no ha muerto");
        }
        yield return null;
    }

    IEnumerator StopRunaway()
    {
        bool found = false;
        while (!found)
        {
            int index = Random.Range(0, allWaypointList.Count - 1);
            GameObject points = allWaypointList[index];
            if (points != WayPointList)
            {
                WayPointList = points;
                actualWayPoint = 0;
                found = true;
                waypoints = new List<Transform>();
                foreach(Transform t in WayPointList.transform)
                {
                    waypoints.Add(t);
                }

            }
        }
        yield return new WaitForSeconds(5.0f);
        setState(AIStates.Patrol);
        yield return null;
    }
}
