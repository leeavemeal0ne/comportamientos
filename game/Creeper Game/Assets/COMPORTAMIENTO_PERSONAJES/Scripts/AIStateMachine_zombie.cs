using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

namespace comportamiento_personajes
{
    // Public Enums of the AI System
   

    public class AIStateMachine_zombie : MonoBehaviour
    {
        //GameObjects
        protected Coroutines_Script coroutine_script;

        [HideInInspector]
        protected AIStates currentState = AIStates.Patrol;

        //Suffle de los waypoints así cada zombie tiene un path distinto
        protected WayPoint_Manager wp = null;
        //path por el que va a moverse el zombie
        private List<Transform> wayPoints;
        //path por el que va a moverse el zombie
        [SerializeField]
        public List<AnimationClip> animations = new List<AnimationClip>();
        //tiempo de la animación idle
        protected float idletime;
        //velocidad de cada animación
        protected float animation_speed = 1.5f;
      

        //objetivo actual
        private int currentWayPoint;
        //Coroutine ended
        protected bool coroutinePatrolEnded;
        //Se ha calculado el siguiente path para el navmesh
        protected bool nextPathCalculated = false;
        //hemos llegado al punto de control del navmesh
        protected bool waypointReached = false;
        [SerializeField]
        protected NavMeshAgent agent;
        [SerializeField]
        protected Animator animator;

        //zombie hungry
        [SerializeField]
        protected float hungry = 100;
        protected float hungry_time = 0.005f;
        protected float min_hungry = 0.1f;

        private void Start()
        {
            agent = gameObject.GetComponent<NavMeshAgent>();
            coroutine_script = gameObject.GetComponent<Coroutines_Script>();
           
            hungry = 100;
            hungry_time = 0.005f;
            min_hungry = 0.1f;

            currentWayPoint = -1;
            currentState = AIStates.Patrol;

            //animator = gameObject.GetComponent<Animator>();
            idletime = animations[0].length;

            wayPoints = new List<Transform>(15);
            wp = WayPoint_Manager.getInstance();
            wayPoints = wp.getWayPointsPath();

            setNextRandomPoint();

            //setAnimatorTriggerParameters("Empty_state_loaded");
        }

        #region GetNextPoint_region for food, attack, nextWaypoint
        /// <summary>
        /// Encuentra el siguiente punto donde tiene que ir en su ronda
        /// </summary>
        public void setNextRandomPoint()
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
        
        #endregion

        #region setAgentParameters
        protected void setAgentParameters(float speed = 0, float angular_speed = 0)
        {
            agent.speed = speed;
            agent.angularSpeed = angular_speed;
        }
        #endregion

        #region setAnimatorParameters
        protected void setAnimatorParameters(string name, float value)
        {
            animator.SetFloat(name, value);
        }
        protected void setAnimatorParameters(string name, bool value)
        {
            animator.SetBool(name, value);
        }
        /*private void setAnimatorParameters(string name, int value)
        {
            animator.SetInteger(name, value);
        }*/
        protected void setAnimatorTriggerParameters(string name)
        {
            animator.Rebind();

            Debug.Log("Llamo a animator cargo animacion: " + name);
            animator.SetTrigger(name);
        }
        #endregion

        public virtual void CheckStateBehaviour() { }
        
    }  
}
