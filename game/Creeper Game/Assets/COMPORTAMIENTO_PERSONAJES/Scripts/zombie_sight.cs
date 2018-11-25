using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace comportamiento_personajes
{
    public class zombie_sight : MonoBehaviour
    {
        public string TAG = "Player";

        //public NavMeshAgent agent;
        protected Basic_zombie_behaviour zb;
        public float fieldOfViewAngle = 110f;
        private bool playerInSight;
        private Vector3 personalLastSighting;
        private Vector3 previousSighting;
        private List<GameObject> players;

        private float waitSeconds;

        private SphereCollider col;

        #region getters/setters
        public List<GameObject> getPlayers() { return players; }
        public Basic_zombie_behaviour GetBasic_Zombie_Behaviour() { return zb; }
        public string getTAG() { return TAG; }
        #endregion

        // Use this for initialization
        void Start()
        {
            //Buscamos todos los gameObjects con tag Player
            players = new List<GameObject>(5);
            players = GameObject.FindGameObjectsWithTag(TAG).ToList();
            Debug.Log("Numero de players en lista: " + players.Count);
            col = GetComponent<SphereCollider>();
            //player o superviviente es visible
            playerInSight = false;
            //tiempo que le damos por si vuelve a entrar en el collider el superviviente
            waitSeconds = 0.5f;
            zb = GetComponentInParent<Basic_zombie_behaviour>();
        }

        private void OnTriggerEnter(Collider other)
        {
            //Si la lista no contiene al objeto que entra sale
            if (!players.Contains(other.gameObject) || zb.feeding)
            {
                //Debug.Log("RETURN");
                return;
            }
            else
            {
                if (zb.currentState == AIStateType.Feeding)
                {
                    zb.ResetAllFeedingTasks();
                }

                    calculateSight(other);
                //Si veo al jugador cambio el estado a alerta
                if (playerInSight)
                {
                    Debug.Log("SIGHT ON TRIGGER ENTER LE VEO Y CAMBIO A ALERTED");
                    zb.setCurrentState(AIStateType.Alerted);
                }
                else
                {
                    Debug.Log("SIGHT ON TRIGGER ENTER ----NO LE VEO");
                }                   
            }
        }

        private void OnTriggerStay(Collider other)
        {
            //Si esta comiendo no hace nada
            if (zb.feeding)
            {
                return;
            }

            if (players.Contains(other.gameObject))
            {
                calculateSight(other);
                //Si veo al jugador y estamos en modo patrulla cambio el estado a alerta
                if (playerInSight && zb.currentState == AIStateType.Patrol)
                {
                    //Debug.Log("LE VEO ASIK VOY A POR EL -------------------------------------------------------");
                    zb.setCurrentState(AIStateType.Alerted);
                }
                    
                else if (!playerInSight)
                {
                    if(zb.currentState == AIStateType.Alerted)
                        zb.backToPatrol();
                }
                   
            }

        }

        private void OnTriggerExit(Collider other)
        {
            if (zb.feeding) return; 

            if (players.Contains(other.gameObject))
            {
                if (zb.currentState == AIStateType.Alerted)
                    StartCoroutine(wait());
                else
                {
                    playerInSight = false;
                }
            }
        }

        private IEnumerator wait()
        {
            yield return new WaitForSeconds(waitSeconds);
            playerInSight = false;
            zb.backToPatrol();
            
            //currentState = AIStateType.Patrol;
            StopAllCoroutines();
        }

        /// <summary>
        /// Calcula si vemos al personaje con un angulo prefijado si está dentro de ese ángulo el superviviente
        /// y no hay nada entre el y el zombie le perseguimos
        /// sino no hacemos nada
        /// </summary>
        /// <param name="other"></param>
        private void calculateSight(Collider other)
        {
            playerInSight = false;

            if (players.Contains(other.gameObject))
            {               
                Vector3 direction = other.transform.position - this.transform.position;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle < fieldOfViewAngle * 0.5f)
                {
                    RaycastHit hit;
                    Debug.DrawRay(transform.position, direction);
                    if (Physics.Raycast(transform.position, direction.normalized, out hit, col.radius))
                    {
                        if (players.Contains(hit.collider.gameObject))
                        {
                            //Debug.Log("TE VEOOO llego a persecution point");
                            playerInSight = true;
                            previousSighting = hit.collider.transform.position;
                            zb.setPersecutionPoint(previousSighting);
                            //agent.SetDestination(previousSighting);
                        }                        
                    }                   
                    direction = Quaternion.Euler(0, -40, 0) * direction;
                    Debug.DrawRay(transform.position, direction, Color.blue);
                    direction = Quaternion.Euler(0, 80, 0) * direction;
                    Debug.DrawRay(transform.position, direction, Color.blue);
                    Debug.DrawLine(transform.position, hit.point,Color.red);
                }
            }
        }
       
    }
}
