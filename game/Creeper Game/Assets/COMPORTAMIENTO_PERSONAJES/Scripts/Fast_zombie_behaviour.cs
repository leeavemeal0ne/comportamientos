using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

namespace comportamiento_personajes
{
    // Public Enums of the AI System
    
    

    public class Fast_zombie_behaviour : Zombie
    {
        //gameobjects para modo PEACE
        public GameObject vision;
        public GameObject attack;

        //Agent variables
        private const float AGENT_SPEED = 0.05f;
        private const float AGENT_ROTATIONSPEED = 120;

        private Vector3? lastPositionKnown = null;
        //attack boolean
        [HideInInspector]
        public bool isAttacking;

        //coroutines para parar su ejecución
        private Coroutine co = null, wa = null, blood = null;

        //Coroutine ended
        private bool coroutinePatrolEnded;

        [HideInInspector]
        public AIStates currentState = AIStates.Patrol;

        //Suffle de los waypoints así cada zombie tiene un path distinto
        private WayPoint_Manager wp = null;
        //path por el que va a moverse el zombie
        private List<Transform> wayPoints;
        //path por el que va a moverse el zombie
        public List<AnimationClip> animations = new List<AnimationClip>();
        private float animation_speed = 1.5f;

        //mouth position para activar las partículas de sangre
        public Transform Mouth;

        //objetivo actual
        private int currentWayPoint;
        private bool nextPathCalculated = false;
        public NavMeshAgent agent;
        public Animator animator;

        //wait time
        private float waitTime = 0.25f;

        //particle System
        public ParticleSystem bloodParticles;

        //zombie hungry
        public float hungry = 100f;
        private float hungry_time = 0.005f;
        private float min_hungry = 0.1f;
        [HideInInspector]
        public bool feeding = false;
        private bool seeking_food = false;

        //zombie life
        public int life;
        private bool isDead;
        

        // Use this for initialization
        public void Start()
        {
            gameObject.tag = Tags.NORMAL_ZOMBIE;

            life = 100;

            isAttacking = false;

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

        #region getters/setters
        public NavMeshAgent getAgent()
        {
            return agent;
        }
        public WayPoint_Manager getWayPoint_Manager()
        {
            return wp;
        } 
        public void setCurrentState(AIStates state)
        {
            currentState = state;
        }
        public override bool getIsDead()
        {
            return isDead;
        }
        public float getAgentSpeed()
        {
            return AGENT_SPEED;
        }
        public float getAgentRotationSpeed()
        {
            return AGENT_ROTATIONSPEED;
        }
        #endregion

        #region TakeDamage_state
        /// <summary>
        /// Cuando golpean al zombie le resta vida
        /// </summary>
        /// <param name="damage"></param>
        public override void TakeDamage(int damage)
        {
            if (isDead) return;

            life -= damage;
            if(life <= 5 && !isDead)
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
            currentState = AIStates.Dead;
            setAgentParameters(0, 0);
            agent.ResetPath();
            //activar animación de daño
            StopAllCoroutines();

            Collider[] c = GetComponentsInChildren<Collider>();
            Destroy(GetComponent<NavMeshAgent>());
            foreach (Collider col in c)
            {
                Destroy(col);
            }

            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Rigidbody>().isKinematic = true;

        }

        public override void notifyPeace()
        {
            if(currentState != AIStates.Peace)
            {
                currentState = AIStates.Peace;
                vision.SetActive(false);
                attack.SetActive(false);
                backToPatrol();
            }           
        }
        public override void leavePeace()
        {
            if (currentState == AIStates.Peace)
            {
                currentState = AIStates.Patrol;
                vision.SetActive(true);
                attack.SetActive(true);
                backToPatrol();
                //GetComponentInChildren<Fast_zombie_sight>().enabled = true;
                //GetComponentInChildren<Fast_zombie_attack>().enabled = true;
            }
        }
        #endregion

        #region init animator
        /// <summary>
        /// reseteamos el animator cuando iniciamos el juego
        /// </summary>
        public virtual void initAnimator()
        {
            animator.Rebind();
        }
        #endregion

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
        /*private void setAnimatorParameters(string name, int value)
        {
            animator.SetInteger(name, value);
        }*/
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
            ResetAllPatrolTasks();
            //way point manager nos da el punto de comida más cercano
            Vector3 point = wp.get_closest_Zombie_food(this.transform).position;
            Debug.Log("Punto mas cercano = " + point);
            agent.SetDestination(point);

            Debug.Log("LLAMO A SETFOODPOINT---------------");
            setAnimatorParameters("Speed", 1);
            setAgentParameters(AGENT_SPEED, AGENT_ROTATIONSPEED);
        }
        #endregion
        
        

        // Update is called once per frame
        public void FixedUpdate()
        {
            if(currentState == AIStates.Dead)
            {
                return;
            }

            //Si no está en alerta o attaque accede aquí así restamos si tiene hambre
            if(!currentState.Equals(AIStates.Alerted) && !currentState.Equals(AIStates.Attack) && !currentState.Equals(AIStates.Peace))
            {
                //coroutinePatrolEnded indica si las coroutines han terminado así no mezclamos animaciones ni estados durante las animaciones
                if (!currentState.Equals(AIStates.Feeding) && !feeding && coroutinePatrolEnded)
                {
                    hungry -= hungry_time;
                    if (hungry < 0)
                    {
                        hungry = 0;
                    }
                    if (hungry < min_hungry )
                    {
                        currentState = AIStates.Feeding;
                    }
                }
                //si está comiendo restauramos el hambre a 100 y volvemos al estado Patrol
                else if (feeding && coroutinePatrolEnded)
                {
                    hungry += hungry_time * 50;
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
                case AIStates.Peace:
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
                    //nada
                    setAnimatorParameters("Speed", 3);
                    break;
                case AIStates.Attack:
                    //nada
                    break;
                default:
                    currentState = AIStates.Patrol;
                    setNextRandomPoint();
                    break;
            }
        }

        #region feeding_state_methods
        /// <summary>
        /// Cuando hemos llegado al zombie_food para alimentarnos y entramos en su collider nos activa esta función
        /// Que nos hace entrar en el estado feed y activa las animaciones de comer
        /// </summary>
        public override void startToEat()
        {
            if (!feeding && AIStates.Feeding==currentState)
            {
                feeding = true;

                //ResetAllPatrolTasks();
                Debug.Log("START TO EAT DENTRO IF");

                setAnimatorParameters("Speed", 0);
                setAgentParameters(0,0);
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
            float idletime = animations[0].length;
            //float time = animations[1].length;

            yield return new WaitForSeconds((idletime)/animation_speed);

            wa = StartCoroutine(wait());
            StopCoroutine(co);
        }
        /// <summary>
        /// si está en estado alert vuelve a patrol
        /// </summary>
        public void backToPatrol()
        {
            agent.ResetPath();
            setAnimatorParameters("Speed", 0);
            setCurrentState(AIStates.Patrol);
            setNextRandomPoint();
        }
        /// <summary>
        /// Si está en estado attack lo llama para volver a alert
        /// </summary>
        public void backToAlert()
        {
            setisAttacking(false);
            setCurrentState(AIStates.Alerted);
            //lo dejamos en idle mientras calculamos el siguiente punto así no anda a lo loco mientras calcula
            setAnimatorParameters("Speed", 0);
            setNextRandomPoint();
        }
        #endregion

        #region Alerted_state_methods
        /// <summary>
        /// busca la posición del agente a perseguir y lo actualiza mientras está en nuestro campo de visión
        /// </summary>
        /// <param name="position"></param>
        public void setPersecutionPoint(Vector3 position)
        {
            if (!position.Equals(lastPositionKnown))
            {
                lastPositionKnown = (Vector3?)position;
                agent.SetDestination(position);
            }
            
        }
        /// <summary>
        /// reseteamos el path del agente
        /// </summary>
        public void resetPersecutionPoint()
        {
            lastPositionKnown = null;
            agent.ResetPath();
        }
        /// <summary>
        /// bool que indica si está en modo ataque el zombie
        /// </summary>
        /// <param name="value"></param>
        public void setisAttacking(bool value)
        {
            isAttacking = value;
        }
        public bool getisAttacking()
        {
            return isAttacking;
        }
        #endregion

        /// <summary>
        /// Cuando cerramos la aplicación reseteamos todas las coroutines por si acaso se queda alguna
        /// sin terminar no vaya a dar fallo al iniciar la aplicación
        /// Es más precaución que otra cosa
        /// </summary>
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
        }

        public void ResetAllFeedingTasks()
        {
            seeking_food = false;
            agent.ResetPath();
        }
        #endregion

    }
}
