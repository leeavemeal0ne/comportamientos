using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace comportamiento_personajes
{
    public class Feeding_state : AIStateMachine_zombie
    {
        private bool feeding = false;
        private bool seeking_food = false;

        //particle System
        [SerializeField]
        public ParticleSystem bloodParticles;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public void Start()
        {
            
        }

        public void StartState()
        {
            currentState = AIStateType_state.Feeding;
            seeking_food = true;
            setFoodPoint();
        }

        
        /// <summary>
        /// Cuando tiene hambre recibe el punto  ,donde se encuentra la comida más cercana
        /// </summary>
        private void setFoodPoint()
        {
            seeking_food = true;
            Debug.Log("setFoodPoint + wp: ");
            //way point manager nos da el punto de comida más cercano
            Vector3 point = wp.get_closest_Zombie_food(gameObject.transform).position;
            agent.SetDestination(point);

            setAnimatorParameters("Speed", 1);
            setAgentParameters(2, 120);
        }

        #region feeding_state_methods
        public void startToEat()
        {
            if (!feeding && AIStateType_state.Feeding == currentState)
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

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        public void Update() {

            if (!currentState.Equals(AIStateType_state.Feeding)) return;

            if (feeding)
            {
                hungry += hungry_time * 50;
                Debug.Log("Hungry Level = " + hungry);
                if (hungry >= 90f && !currentState.Equals(AIStateType.Alerted) && !currentState.Equals(AIStateType.Attack))
                {
                    Debug.Log("ENTRO IF FEEDING UPDATE");
                    setAnimatorParameters("Feeding_bool", false);
                    hungry = 100;
                    gameObject.GetComponent<Patrol_Behaviour>().StartState();
                    seeking_food = false;
                    feeding = false;

                    coroutine_script.coroutineWait(this, nextPathCalculated, coroutinePatrolEnded);
                    //setAnimatorTriggerParameters("Patrol_state");
                }
            }
        }

    }
}
