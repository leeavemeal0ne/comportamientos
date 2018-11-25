using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace comportamiento_personajes
{
    public class Patrol_Behaviour : AIStateMachine_zombie
    {
        private Coroutine co = null, wa = null, blood = null;

        //wait time
        private float waitTime = 0.25f;       

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public void Start() {
            coroutinePatrolEnded = true;
            co = null; wa = null; blood = null;
        }

        public void StartState()
        {
            currentState = AIStateType_state.Patrol;
            co = null; wa = null; blood = null;            
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        public void Update() {

            if (!currentState.Equals(AIStateType_state.Patrol)) return;

            if (!currentState.Equals(AIStateType_state.Feeding) && coroutinePatrolEnded)
                {
                hungry -= hungry_time;
                if (hungry < 0)
                {
                    hungry = 0;
                }
                if (hungry < 0.1f && !currentState.Equals(AIStateType_state.Alerted) && !currentState.Equals(AIStateType_state.Attack))
                {                   
                    gameObject.GetComponent<Feeding_state>().StartState();
                    //setAnimatorTriggerParameters("Feeding_state");
                    return;
                }              
            }

            CheckStateBehaviour();

        }
        
        public override void CheckStateBehaviour()
        {
            switch (currentState)
            {
                case AIStateType_state.Patrol:
                    if (agent.remainingDistance < 0.3f && !nextPathCalculated)
                    {
                        nextPathCalculated = true;
                        setAgentParameters(0, 0);
                        setAnimatorParameters("Speed", 0);
                        coroutine_script.loadAnimSeek(this, coroutinePatrolEnded, nextPathCalculated, idletime, animation_speed);
                    }
                    break;
            }
        }
        
    }
}
