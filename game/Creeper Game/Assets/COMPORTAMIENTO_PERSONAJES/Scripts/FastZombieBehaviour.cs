using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using comportamiento_personajes;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

public class FastZombieBehaviour : MonoBehaviour {
    public Coroutine co = null, wa = null, blood = null;

    //Coroutine ended
    private bool coroutinePatrolEnded;

    private AIStates currentState = AIStates.Patrol;

    //Suffle de los waypoints así cada zombie tiene un path distinto
    private WayPoint_Manager wp = null;
    //path por el que va a moverse el zombie
    private List<Transform> wayPoints;
    //path por el que va a moverse el zombie
    public List<AnimationClip> animations = new List<AnimationClip>();
    private float animation_speed = 1.5f;

    //mouth position
    public Transform Mouth;

    //objetivo actual
    private int currentWayPoint;
    private bool nextPathCalculated = false;
    private bool waypointReached = false;
    public NavMeshAgent agent;
    private Animator animator;

    //wait time
    private float waitTime = 0.25f;

    //particle System
    public ParticleSystem bloodParticles;

    //zombie hungry
    public float hungry = 15;
    private float hungry_time = 0.005f;
    private float min_hungry = 0.1f;
    private bool feeding = false;
    private bool seeking_food = false;

    //zombie vision
    private GameObject target;
    private float distance = 100;
    private bool alerted = false;
    private bool lookingFor = false;
    private bool atacking = false;

    // Use this for initialization
    void Start()
    {
        StopAllCoroutines();

        coroutinePatrolEnded = true;
        co = null; wa = null; blood = null;

        currentWayPoint = -1;
        currentState = AIStates.Patrol;

        animator = GetComponent<Animator>();
        initAnimator();

        wayPoints = new List<Transform>(15);
        wp = WayPoint_Manager.getInstance();
        wayPoints = wp.getWayPointsPath();

        setNextRandomPoint();
    }

    #region init animator
    private void initAnimator()
    {
        animator.Rebind();
        setAnimatorParameters("Speed", 1);
        setAnimatorParameters("SeekingRight_bool", false);
        setAnimatorParameters("SeekingLeft_bool", false);
        setAnimatorParameters("Feeding_bool", false);
    }
    #endregion

    #region setAgentParameters
    private void setAgentParameters(float speed = 0, float angular_speed = 0)
    {
        agent.speed = speed;
        agent.angularSpeed = angular_speed;
    }
    #endregion

    #region setAnimatorParameters
    private void setAnimatorParameters(string name, float value)
    {
        animator.SetFloat(name, value);
    }
    private void setAnimatorParameters(string name, bool value)
    {
        animator.SetBool(name, value);
    }
    /*private void setAnimatorParameters(string name, int value)
    {
        animator.SetInteger(name, value);
    }*/
    private void setAnimatorTriggerParameters(string name)
    {
        animator.Rebind();

        Debug.Log("Llamo a animator cargo animacion: " + name);
        animator.SetTrigger(name);
    }
    #endregion

    #region GetNextPoint_region for food, attack, nextWaypoint
    /// <summary>
    /// Encuentra el siguiente punto donde tiene que ir en su ronda
    /// </summary>
    void setNextRandomPoint()
    {
        currentWayPoint++;

        if (currentWayPoint.Equals(wayPoints.Count))
        {
            currentWayPoint = 0;
        }
        agent.SetDestination(wayPoints[currentWayPoint].position);
        setAnimatorParameters("Speed", 1);
        setAgentParameters(2, 120);

    }

    /// <summary>
    /// Cuando tiene hambre recibe el punto  ,donde se encuentra la comida más cercana
    /// </summary>
    private void setFoodPoint()
    {
        seeking_food = true;
        ResetAllPatrolTasks();
        Debug.Log("setFoodPoint");
        //way point manager nos da el punto de comida más cercano
        agent.SetDestination(wp.get_closest_Zombie_food(this.transform).position);

        setAnimatorParameters("Speed", 1);
        setAgentParameters(2, 120);
    }
    #endregion

    private void setAlertedPoint()
    {
        ResetAllPatrolTasks();
        agent.SetDestination(target.transform.position);
        setAnimatorParameters("Speed", 1);
        setAgentParameters(2, 120);
        lookingFor = true;
    }
    private void setAtackPoint()
    {
        StopAllPatrolTasks();
        //agent.SetDestination(target.transform.position);
        setAnimatorParameters("Atack", true);
        
        //setAnimatorParameters("Speed", 0);
        //setAnimatorTriggerParameters("Attack_trigger");
        //setAgentParameters(2, 120);
        atacking = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentState.Equals(AIStates.Feeding) && !feeding && coroutinePatrolEnded)
        {
            hungry -= hungry_time;
            if (hungry < 0)
            {
                hungry = 0;
            }
            if (hungry < 0.1f && !currentState.Equals(AIStates.Alerted) && !currentState.Equals(AIStates.Attack))
            {
                currentState = AIStates.Feeding;
            }
        }
        else if (feeding && coroutinePatrolEnded)
        {
            hungry += hungry_time * 50;
            Debug.Log("Hungry Level = " + hungry);
            if (hungry >= 90f && !currentState.Equals(AIStates.Alerted) && !currentState.Equals(AIStates.Attack))
            {
                Debug.Log("ENTRO IF FEEDING UPDATE");
                setAnimatorParameters("Feeding_bool", false);
                hungry = 100;
                currentState = AIStates.Patrol;
                seeking_food = false;
                feeding = false;

                StartCoroutine(wait());
            }
        }
        else if(currentState.Equals(AIStates.Alerted))
        {
            float d = Vector3.Distance(target.transform.position, transform.position);
            if (d < 3)
            {
                currentState = AIStates.Attack;
                lookingFor = false;
            }
            else
            {
                if (d > GetComponent<SphereCollider>().radius)
                {
                    currentState = AIStates.Patrol;
                    lookingFor = false;
                }
                else
                {
                    agent.SetDestination(target.transform.position);
                }
            }
        }
        else if (currentState.Equals(AIStates.Attack))
        {
            float d = Vector3.Distance(target.transform.position, transform.position);
            if (d < 5)
            {
                //currentState = AIStates.Attack;
                //lookingFor = false;
            }
            else
            {

                currentState = AIStates.Alerted;
                atacking = false;
            }
        }

        checkStateBehaviour();
    }

    void OnTriggerStay(Collider collision)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.tag == "NormalZombie")
        {
            Vector3 direction = collision.gameObject.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);
            if (angle < 30.0f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction, out hit, Vector3.Distance(collision.gameObject.transform.position, transform.position)))
                {
                    if (hit.transform.tag == "NormalZombie")
                    {
                        float d = Vector3.Distance(collision.gameObject.transform.position, transform.position);
                        if (d < distance)
                        {
                            target = hit.transform.gameObject;
                            distance = d;
                            currentState = AIStates.Alerted;
                            
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

        //Check for a match with the specific tag on any GameObject that collides with your GameObject
        if (collision.gameObject.tag == "MyGameObjectTag")
        {
            //If the GameObject has the same tag as specified, output this message in the console
            Debug.Log("Do something else here");
        }
    }

    private void checkStateBehaviour()
    {
        switch (currentState)
        {
            case AIStates.Patrol:
                if (agent.remainingDistance < 0.3f && !nextPathCalculated)
                {
                    nextPathCalculated = true;
                    setAgentParameters(0, 0);
                    setAnimatorParameters("Speed", 0);
                    co = StartCoroutine(loadAnimationSeek());
                }
                break;
            case AIStates.Feeding:
                if (!seeking_food)
                {
                    setFoodPoint();
                }
                break;
            case AIStates.Alerted:
                if (!lookingFor)
                {
                    setAlertedPoint();
                }
                break;
            case AIStates.Attack:
                if (!atacking)
                {
                    setAtackPoint();
                }
                break;
        }
    }

    #region feeding_state_methods
    public void startToEat()
    {
        if (!feeding && AIStates.Feeding == currentState)
        {
            feeding = true;

            //ResetAllPatrolTasks();
            Debug.Log("START TO EAT DENTRO IF");

            setAnimatorParameters("Speed", 0);
            setAgentParameters(0);
            setAnimatorTriggerParameters("Feeding_trigger");
            setAnimatorParameters("Feeding_bool", true);
        }

    }

    #endregion

    #region Patrol_state_methods

    private IEnumerator wait()
    {
        wa = null;
        setNextRandomPoint();
        yield return new WaitForSeconds(waitTime);
        nextPathCalculated = false;
        coroutinePatrolEnded = true;

    }

    private IEnumerator loadAnimationSeek()
    {
        co = null;
        coroutinePatrolEnded = false;
        float idletime = animations[0].length;
        //float time = animations[1].length;

        yield return new WaitForSeconds((idletime) / animation_speed);

        wa = StartCoroutine(wait());
    }
    #endregion

    #region AppExit_resetCoroutines
    private void OnApplicationQuit()
    {
        StopAllPatrolTasks();
    }

    private void StopAllPatrolTasks()
    {
        if (co != null)
        {
            StopCoroutine(co);
        }
        co = null;

        if (wa != null)
        {
            StopCoroutine(wa);
        }
        wa = null;

        if (blood != null)
        {
            StopCoroutine(blood);
        }
        blood = null;
        initAnimator();
    }

    private void ResetAllPatrolTasks()
    {

        Debug.Log("Cierro las aplicaciones");
        if (co != null)
        {
            StopCoroutine(co);
        }

        if (wa != null)
        {
            StopCoroutine(wa);
        }
        initAnimator();
    }
    #endregion
}
