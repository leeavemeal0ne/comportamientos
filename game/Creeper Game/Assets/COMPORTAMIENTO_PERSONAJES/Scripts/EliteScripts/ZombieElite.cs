using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;
using comportamiento_personajes;
using UnityEngine.AI;

public class ZombieElite : Zombie {

    //Agent variables
    private const float AGENT_SPEED = 0.05f;
    private const float AGENT_ROTATIONSPEED = 120;

    //wait time
    private float waitTime = 0.25f;

    //coroutines para parar su ejecución
    private Coroutine co = null, wa = null, blood = null;

    [HideInInspector]
    public AIStates currentState = AIStates.Patrol;
    //Suffle de los waypoints así cada zombie tiene un path distinto
    private WayPoint_Manager wp = null;
    //path por el que va a moverse el zombie
    private List<Transform> wayPoints;
    //path por el que va a moverse el zombie
    public AnimationClip animations;
    private float animation_speed = 1.5f;

    //zombie hungry
    public float hungry = 100f;
    private float hungry_time = 0.005f;
    private float min_hungry = 0.1f;

    //particle System
    public ParticleSystem bloodParticles;

    //objetivo actual
    private int currentWayPoint;
    public NavMeshAgent agent;
    private bool nextPathCalculated = false;
    public Animator animator;
    [HideInInspector]
    public bool feeding = false;
    private bool seeking_food = false;

    //zombie life
    public int life;
    private bool isDead;

    //Coroutine ended
    private bool coroutinePatrolEnded;

    

    // Use this for initialization
    void Start () {
        Cursor.visible = false;
        gameObject.tag = Tags.ELITE_ZOMBIE;

        life = 100;

        coroutinePatrolEnded = true;
        co = null; wa = null; blood = null;

        currentWayPoint = -1;
        currentState = AIStates.Patrol;

        animator = GetComponentInParent<Animator>();
        initAnimator();
        setAgentParameters(0, 0);

        wayPoints = new List<Transform>(15);
        wp = WayPoint_Manager.getInstance();
        wayPoints = wp.getWayPointsPath();

        setNextRandomPoint();
    }

    #region init animator
    /// <summary>
    /// reseteamos el animator cuando iniciamos el juego
    /// </summary>
    public virtual void initAnimator()
    {
        animator.Rebind();
    }
    #endregion

    public void setCurrentState(AIStates state)
    {
        currentState = state;
    }

    /**
     * En esta región simplemente son setters para animaciones y velocidad del agente para el navmesh
     * así uso siempre las mismas funciones
    */
    #region setAgentParameters
    public void setAgentParameters(float speed = 0, float angular_speed = 0)
    {
        agent.speed = speed;
        agent.angularSpeed = angular_speed;
    }
    #endregion

    #region setAnimatorParameters
    public void setAnimatorParameters(string name, float value)
    {
        animator.SetFloat(name, value);
    }
    public void setAnimatorParameters(string name, bool value)
    {
        animator.SetBool(name, value);
    }
    public void setAnimatorTriggerParameters(string name)
    {
        //animator.Rebind();

        animator.SetTrigger(name);
    }
    #endregion

    #region GetNextPoint_region for food, attack, nextWaypoint
    /// <summary>
    /// Encuentra el siguiente punto donde tiene que ir en su ronda de waypoints y le da una velocidad por defecto
    /// </summary>
    public virtual void setNextRandomPoint()
    {
        currentWayPoint++;

        if (currentWayPoint.Equals(wayPoints.Count))
        {
            currentWayPoint = 0;
        }
        agent.SetDestination(wayPoints[currentWayPoint].position);
        //Debug.Log("LLAMO A SETNEXTRANDOMPOINT-------------------------------");
        setAnimatorParameters("Speed", 1);
        setAgentParameters(AGENT_SPEED, AGENT_ROTATIONSPEED);

    }

    /// <summary>
    /// Cuando tiene hambre recibe el punto  ,donde se encuentra la comida más cercana
    /// </summary>
    private void setFoodPoint()
    {
        seeking_food = true;
        //way point manager nos da el punto de comida más cercano
        Vector3 point = wp.get_closest_Zombie_food(this.transform).position;
        Debug.Log("Punto mas cercano = " + point);
        agent.SetDestination(point);

        Debug.Log("LLAMO A SETFOODPOINT---------------");
        setAnimatorParameters("Speed", 1);
        setAgentParameters(AGENT_SPEED, AGENT_ROTATIONSPEED);
    }
    #endregion

    #region FixedUpdate
    private void FixedUpdate()
    {
        if (currentState == AIStates.Dead)
        {
            return;
        }

        //coroutinePatrolEnded indica si las coroutines han terminado así no mezclamos animaciones ni estados durante las animaciones
        if (!currentState.Equals(AIStates.Feeding) && !feeding && coroutinePatrolEnded)
        {
            hungry -= hungry_time;
            if (hungry < 0)
            {
                hungry = 0;
            }
            if (hungry < min_hungry)
            {
                currentState = AIStates.Feeding;
            }
        }
        //si está comiendo restauramos el hambre a 100 y volvemos al estado Patrol
        else if (feeding && coroutinePatrolEnded)
        {
            hungry += hungry_time * 50;
            if (hungry >= 90f)
            {
                //Debug.Log("ENTRO IF FEEDING UPDATE");
                setAnimatorParameters("Feeding_bool", false);
                hungry = 100;
                currentState = AIStates.Patrol;
                seeking_food = false;
                feeding = false;

                StartCoroutine(wait());
            }
        }

        //chequeamos los estados aunque en realidad solo dos estados aquí los demás estados se activan por eventos
        // de los colliders VISION Y ATTACK_COLLIDER
        checkStateBehaviour();
    }
    protected void checkStateBehaviour()
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
        }
    }
    #endregion

    

    #region feeding_state_methods
    /// <summary>
    /// Cuando hemos llegado al zombie_food para alimentarnos y entramos en su collider nos activa esta función
    /// Que nos hace entrar en el estado feed y activa las animaciones de comer
    /// </summary>
    public override void startToEat()
    {
        if (!feeding && AIStates.Feeding == currentState)
        {
            feeding = true;

            //ResetAllPatrolTasks();
            Debug.Log("START TO EAT DENTRO IF");

            setAnimatorParameters("Speed", 0);
            setAgentParameters(0, 0);
            setAnimatorTriggerParameters("Feeding_trigger");
            setAnimatorParameters("Feeding_bool", true);
        }
    }
    #endregion

    /// <summary>
    /// Estas coroutines lo que me permiten es esperar hasta que la animación ha terminado como no me funciona muy bien coger
    /// el animator getCurrentAnimationClipInfo he guardado las animaciones que se ejecutan cuando estoy patrullando y espero
    /// el tiempo que dura la animación Idle antes de decirle al fixedUpdate que puede continuar poniendo todos los booleans a false
    /// para el siguiente camino calculado y true que ya hemos terminado las coroutines
    /// </summary>
    /// <returns></returns>
    #region Patrol_state_methods
    private IEnumerator wait()
    {
        setNextRandomPoint();
        yield return new WaitForSeconds(waitTime);
        nextPathCalculated = false;
        coroutinePatrolEnded = true;
        StopCoroutine(wa);
    }

    private IEnumerator loadAnimationSeek()
    {
        coroutinePatrolEnded = false;
        float idletime = animations.length;
        //float time = animations[1].length;

        yield return new WaitForSeconds((idletime) / animation_speed);

        wa = StartCoroutine(wait());
        StopCoroutine(co);
    }
    #endregion

    /// <summary>
    /// si está en estado alert vuelve a patrol
    /// </summary>
    public void backToPatrol()
    {
        setAnimatorParameters("Speed", 0);
        setCurrentState(AIStates.Patrol);
        setNextRandomPoint();
    }

    public override void TakeDamage(int dmg)
    {
        if (isDead) return;

        life -= dmg;
        if (life <= 0 && !isDead)
        {
            isDead = true;
            gameObject.tag = Tags.DEATH_ZOMBIE;
            setAnimatorTriggerParameters("Dead_trigger");
            ZombieIsDead();
        }
        else
        {
            setAnimatorTriggerParameters("Pain_trigger");
        }
    }
    private void ZombieIsDead()
    {
        Debug.Log("DEBERÍA ESTAR MUERTO");
        life = 0;
        //initAnimator();
        if (co != null)
        {
            StopCoroutine(co);
        }
        if (wa != null)
        {
            StopCoroutine(wa);
        }
        //cambiamos estado a muerto y reseteamos todo
        currentState = AIStates.Dead;
        setAgentParameters(0, 0);
        agent.ResetPath();
        //activar animación de daño
        StopAllCoroutines();

        List<GameObject> zombiesList = this.GetComponentInChildren<ZombieEliteVision>().zombiesList;
        foreach(GameObject g in zombiesList)
        {
            g.GetComponentInParent<Zombie>().leavePeace();
        }

        Collider[] c = GetComponentsInChildren<Collider>();
        Destroy(GetComponent<NavMeshAgent>());
        foreach (Collider col in c)
        {
            Destroy(col);
        }

        GetComponent<Rigidbody>().isKinematic = true;
    }

    public override bool getIsDead()
    {
        return life <= 0;
        //throw new System.NotImplementedException();
    }

    public override void notifyPeace()
    {
        //throw new System.NotImplementedException();
    }

    public override void leavePeace()
    {
        //throw new System.NotImplementedException();
    }

    public void ResetAllFeedingTasks()
    {
        seeking_food = false;
        agent.ResetPath();
    }
}
