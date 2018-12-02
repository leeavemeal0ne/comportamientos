﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using comportamiento_personajes;

public class StrongZombieBehaviour : MonoBehaviour
{
    public Coroutine co = null, wa = null, blood = null;

    //Coroutine ended
    private bool coroutinePatrolEnded;

    public AIStateType currentState = AIStateType.Patrol;

    //Suffle de los waypoints así cada zombie tiene un path distinto
    private WayPoint_Manager wp = null;
    //path por el que va a moverse el zombie
    private List<Transform> wayPoints;
    //path por el que va a moverse el zombie
    public List<AnimationClip> animations = new List<AnimationClip>();
    private float animation_speed = 1.5f;

    //mouth position
    public Transform Mouth;

    //Hand position
    public Transform rightHand;

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
    public bool feeding = false;
    private bool seeking_food = false;


    //throw object
    public GameObject throwableObject;
    private float gravity = 9.8f;
    private float projectileAngle = 30.0f;
    private Vector3 targetPosition;
    private bool thrown;

    //zombie life
    public int life;
    private bool isDead;

    // Use this for initialization
    void Start()
    {

        life = 100;
        thrown = false;
        StopAllCoroutines();

        coroutinePatrolEnded = true;
        co = null; wa = null; blood = null;

        currentWayPoint = -1;
        currentState = AIStateType.Patrol;

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
        setAgentParameters(0.2f, 60);

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
        setAgentParameters(0.2f, 60);
    }
    #endregion

    // Update is called once per frame
    void FixedUpdate()
    {
        /*
        if (currentState.Equals(AIStateType.Throw))
        {
            throwableObject.GetComponent<Throwable>().updatePhysics();
        }
        */ 
        if (!currentState.Equals(AIStateType.Feeding) && !feeding && coroutinePatrolEnded)
        {
            hungry -= hungry_time;
            if (hungry < 0)
            {
                hungry = 0;
            }
            if (hungry < 0.1f && !currentState.Equals(AIStateType.Alerted) && !currentState.Equals(AIStateType.Attack))
            {
                currentState = AIStateType.Feeding;
            }
        }
        else if (feeding && coroutinePatrolEnded)
        {
            hungry += hungry_time * 50;
            Debug.Log("Hungry Level = " + hungry);
            if (hungry >= 90f && !currentState.Equals(AIStateType.Alerted) && !currentState.Equals(AIStateType.Attack))
            {
                Debug.Log("ENTRO IF FEEDING UPDATE");
                setAnimatorParameters("Feeding_bool", false);
                hungry = 100;
                currentState = AIStateType.Patrol;
                seeking_food = false;
                feeding = false;

                StartCoroutine(wait());
            }

        }


        checkStateBehaviour();
    }

    private void checkStateBehaviour()

    {

        Debug.Log("current state: " + currentState);

        switch (currentState)
        {
            case AIStateType.Patrol:
                if (agent.remainingDistance < 0.3f && !nextPathCalculated)
                {
                    nextPathCalculated = true;
                    setAgentParameters(0, 0);
                    setAnimatorParameters("Speed", 0);
                    co = StartCoroutine(loadAnimationSeek());
                }
                break;
            case AIStateType.Feeding:
                if (!seeking_food)
                {
                    setFoodPoint();
                }
                break;
            case AIStateType.Throw:
                if (agent.remainingDistance < 0.1f)
                {
                    setAnimatorTriggerParameters("ThrowObject");
                    setAnimatorParameters("Speed", 0);
                    setAgentParameters(0);
                    throwableObject.transform.position = rightHand.transform.position;
                    throwableObject.transform.parent = rightHand.transform;
                    currentState = AIStateType.Patrol;

                }
                break;
        }
    }

    private void throwObject()
    {

        Debug.Log("Metodo throw object entra");
        throwableObject.transform.parent = null;
        //Rigidbody r = throwableObject.gameObject.GetComponent<Rigidbody>();
        //r.useGravity = true;

        //esto no va todavía
        targetPosition = GetComponentInChildren<StrongZombieVision>().target.transform.position;
        throwableObject.GetComponent<Throwable>().setForce(throwableObject.transform.position, targetPosition, projectileAngle);
        throwableObject.GetComponent<Throwable>().updatePhysics();

    }

    #region feeding_state_methods
    public void startToEat()
    {
        if (!feeding && AIStateType.Feeding == currentState)
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

    #region Throwable objects
    void OnTriggerEnter(Collider collision)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.tag == "Throwable" && throwableObject == null)
        {
            agent.SetDestination(collision.transform.position);
            currentState = AIStateType.Throw;
            throwableObject = collision.gameObject;

        }

    }


    #endregion

    public void backToPatrol()
    {
        /*
        setAnimatorParameters("Speed", 0);
        setCurrentState(AIStates.Patrol);
        setNextRandomPoint();
        */ 
    }
}

