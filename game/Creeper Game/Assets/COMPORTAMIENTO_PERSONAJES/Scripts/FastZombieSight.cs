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
        public float distance = 100;
        private bool alerted = false;
        private bool lookingFor = false;
        public bool attacking = false;
        private List<string> zombieTags;
        private List<string> survivorTags;

        FastZombieBehaviour parent;

        private void setAlertedPoint()
        {
            parent.ResetAllPatrolTasks();
            parent.agent.SetDestination(target.transform.position);
            parent.setAnimatorParameters("Speed", 3);
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
            zombieTags = new List<string>(new string[] { Tags.NORMAL_ZOMBIE });
            survivorTags = new List<string>(new string[] { Tags.SURVIVOR, Tags.PLAYER });
            parent = GetComponentInParent<FastZombieBehaviour>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (parent.currentState.Equals(AIStates.Alerted))
            {
                float d = Vector3.Distance(target.transform.position, transform.position);
                if (d < 3)
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
                Vector3 direction = target.transform.position - transform.position;
                float d = Vector3.Distance(target.transform.position, transform.position);
                if (d < 3)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    parent.transform.rotation = Quaternion.RotateTowards(parent.transform.rotation, rotation, 20 * Time.deltaTime);
                }
                else
                {
                    distance = 100;
                    parent.currentState = AIStates.Alerted;
                    attacking = false;
                }
            }
            checkStateBehaviour();
        }

        void OnTriggerStay(Collider collision)
        {
            //Check for a match with the specified name on any GameObject that collides with your GameObject
            bool esZombi = zombieTags.Contains(collision.gameObject.tag);
            bool esSuperviviente = survivorTags.Contains(collision.gameObject.tag);

            //Comprobamos si el objeto dentro de nuestro radio de vista es <ombi o superviviente
            if (esZombi || esSuperviviente)
            {
                Vector3 direction = collision.gameObject.transform.position - transform.position;
                float angle = Vector3.Angle(direction, transform.forward);
                //Si se encuentra en el angulo de vista
                if (angle < 30.0f)
                {
                    RaycastHit hit;
                    //Lanzamos un rayo hacia el target con el tamaño del radio de nuesta zona de vista
                    if (Physics.Raycast(transform.position, direction, out hit, Vector3.Distance(collision.gameObject.transform.position, transform.position)))
                    {
                        //Comprobamos que no hay objetos entre medias que nos impida ver el target
                        if (hit.transform.tag == collision.gameObject.tag)
                        {
                            float d = Vector3.Distance(collision.gameObject.transform.position, transform.position);
                            //Si el anterior target era zombi y ahora es superviviente, vamos directos a por el superviviente
                            if (esSuperviviente && zombieTags.Contains(target.tag))
                            {
                                target = hit.transform.gameObject;
                                distance = d;
                                parent.currentState = AIStates.Alerted;
                            }
                            // Si el anterior target era del mismo tipo que el anterior, comprobamos cual está más cerca
                            else if (target == null || esSuperviviente && survivorTags.Contains(target.tag) || esZombi && zombieTags.Contains(target.tag))
                            {
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
                            //Debug.Log("Did not Hit");
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
