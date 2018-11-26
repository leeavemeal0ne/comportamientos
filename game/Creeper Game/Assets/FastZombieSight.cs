using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.COMPORTAMIENTO_PERSONAJES.Constantes;

namespace comportamiento_personajes
{
    public class FastZombieSight : MonoBehaviour
    {
        //zombie vision
        public GameObject target;
        private float distance = 100;
        private bool alerted = false;
        private bool lookingFor = false;
        private bool attacking = false;

        FastZombieBehaviour parent;

        private void setAlertedPoint()
        {
            parent.ResetAllPatrolTasks();
            parent.agent.SetDestination(target.transform.position);
            parent.setAnimatorParameters("Speed", 1);
            parent.setAgentParameters(2, 120);
            lookingFor = true;
        }
        private void setAttackPoint()
        {
            parent.StopAllPatrolTasks();
            //agent.SetDestination(target.transform.position);
            parent.setAnimatorParameters("Atack", true);

            //setAnimatorParameters("Speed", 0);
            //setAnimatorTriggerParameters("Attack_trigger");
            //setAgentParameters(2, 120);
            attacking = true;
        }
        // Use this for initialization
        void Start()
        {
            parent = GetComponentInParent<FastZombieBehaviour>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (parent.currentState.Equals(AIStates.Alerted))
            {
                float d = Vector3.Distance(target.transform.position, transform.position);
                if (d < 2)
                {
                    parent.currentState = AIStates.Attack;
                    lookingFor = false;
                }
                else
                {
                    if (d > GetComponent<SphereCollider>().radius)
                    {
                        parent.currentState = AIStates.Patrol;
                        lookingFor = false;
                    }
                    else
                    {
                        parent.agent.SetDestination(target.transform.position);
                    }
                }
            }
            else if (parent.currentState.Equals(AIStates.Attack))
            {
                float d = Vector3.Distance(target.transform.position, transform.position);
                if (d < 2)
                {
                    //currentState = AIStates.Attack;
                    //lookingFor = false;
                }
                else
                {

                    parent.currentState = AIStates.Alerted;
                    attacking = false;
                }
            }
            checkStateBehaviour();
        }

        void OnTriggerStay(Collider collision)
        {
            //Check for a match with the specified name on any GameObject that collides with your GameObject
            if (collision.gameObject.tag == "Zombie")
            {
                Vector3 direction = collision.gameObject.transform.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);
                if (angle < 30.0f)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, direction, out hit, Vector3.Distance(collision.gameObject.transform.position, transform.position)))
                    {
                        if (hit.transform.tag == "Zombie")
                        {
                            float d = Vector3.Distance(collision.gameObject.transform.position, transform.position);
                            if (d < distance)
                            {
                                target = hit.transform.gameObject;
                                distance = d;
                                parent.currentState = AIStates.Alerted;

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
            switch (parent.currentState)
            {
                case AIStates.Alerted:
                    if (!lookingFor)
                    {
                        setAlertedPoint();
                    }
                    break;
                case AIStates.Attack:
                    if (!attacking)
                    {
                        setAttackPoint();
                    }
                    break;
            }
        }

        
    }

    
    
}
