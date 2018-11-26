using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace comportamiento_personajes
{
    public class Coroutines_Script : MonoBehaviour
    {

        #region Patrol_state_methods
        Coroutine co, wa;
        float waitTime;

        private void Start()
        {
            waitTime = 0.25f;
            co = null;
            wa = null;
        }

        public void coroutineWait(AIStateMachine_zombie pb, bool nextPathCalculated, bool coroutinePatrolEnded)
        {
            wa = StartCoroutine(wait( pb,  nextPathCalculated,  coroutinePatrolEnded));
        }

        private IEnumerator wait(AIStateMachine_zombie pb, bool nextPathCalculated, bool coroutinePatrolEnded)
        {
            pb.setNextRandomPoint();
            yield return new WaitForSeconds(waitTime);
            nextPathCalculated = false;
            coroutinePatrolEnded = true;
            StopCoroutine(wa);
        }

        public void loadAnimSeek(AIStateMachine_zombie pb, bool coroutinePatrolEnded, bool nextPathCalculated, float idletime, float animation_speed)
        {
            co = StartCoroutine(loadAnimationSeek(pb, coroutinePatrolEnded, nextPathCalculated, idletime, animation_speed));
        }
        
        private IEnumerator loadAnimationSeek(AIStateMachine_zombie pb, bool coroutinePatrolEnded, bool nextPathCalculated, float idletime, float animation_speed)
        {
            coroutinePatrolEnded = false;

            yield return new WaitForSeconds((idletime) / animation_speed);

            wa = StartCoroutine(wait(pb, nextPathCalculated, coroutinePatrolEnded));

            StopCoroutine(co);
        }
        #endregion
    }
}
